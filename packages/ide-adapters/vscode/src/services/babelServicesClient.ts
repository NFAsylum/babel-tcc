import * as vscode from 'vscode';
import { CONFIG_SECTION, CONFIG_KEYS } from '../config/constants';

/** How identifier translation is resolved. */
export type LlmMode = 'local' | 'hosted';

/** Resolved settings for a translation request. */
interface LlmSettings {
  mode: LlmMode;
  servicesUrl: string;
  localLlmUrl: string;
}

/** Retrieves the hosted-backend API key (SecretStorage preferred, config fallback). */
export type ApiKeyProvider = () => Promise<string | undefined>;

const REQUEST_TIMEOUT_MS = 5000;

/**
 * TypeScript port of the C# `BabelServicesClient`, mirroring its graceful-degradation contract:
 * a translation request never throws for transport/backend failures — it returns `null` and the
 * caller falls back to local behavior. Two resolution paths:
 *
 * - **hosted** (mode = hosted + API key present): call the hosted backend
 *   (`POST /translate/identifier`). If it's unavailable, silently fall back to the local
 *   llama-server so the user is never blocked by a backend outage.
 * - **local** (mode = local, or hosted unavailable): call the user's local llama-server directly
 *   (OpenAI-compatible `POST /v1/chat/completions`).
 *
 * The free/offline path of the extension never constructs this client — it is only used when the
 * user has opted into LLM-assisted identifier translation.
 */
export class BabelServicesClient {
  constructor(private readonly getApiKey: ApiKeyProvider) {}

  /**
   * Suggests a translation for `identifier`, or `null` if no suggestion is available (backend and
   * local both unreachable, or no local URL configured). Never throws.
   */
  public async translateIdentifier(
    identifier: string,
    contextCode: string,
    targetLanguage: string
  ): Promise<string | null> {
    const settings: LlmSettings = this.readSettings();

    if (settings.mode === 'hosted') {
      const apiKey: string | undefined = await this.getApiKey();
      if (apiKey) {
        const hosted: string | null = await this.translateViaHosted(
          settings.servicesUrl, apiKey, identifier, contextCode, targetLanguage
        );
        if (hosted !== null) {
          return hosted;
        }
        // Hosted unavailable → graceful fallback to local.
      }
    }

    return this.translateViaLocal(settings.localLlmUrl, identifier, contextCode, targetLanguage);
  }

  /** True if the hosted backend is reachable and healthy. Never throws. */
  public async isHostedAvailable(): Promise<boolean> {
    const settings: LlmSettings = this.readSettings();
    if (!settings.servicesUrl) {
      return false;
    }
    const health: unknown = await this.getJson(this.join(settings.servicesUrl, '/health'), {});
    return typeof health === 'object' && health !== null &&
      (health as { status?: string }).status === 'ok';
  }

  private readSettings(): LlmSettings {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return {
      mode: config.get<LlmMode>(this.key(CONFIG_KEYS.LLM_MODE), 'local'),
      servicesUrl: config.get<string>(this.key(CONFIG_KEYS.SERVICES_URL), '').trim(),
      localLlmUrl: config.get<string>(this.key(CONFIG_KEYS.LOCAL_LLM_URL), '').trim(),
    };
  }

  /** Strips the `babel-tcc.` prefix — getConfiguration(section) keys are relative to it. */
  private key(fullKey: string): string {
    return fullKey.startsWith(`${CONFIG_SECTION}.`)
      ? fullKey.slice(CONFIG_SECTION.length + 1)
      : fullKey;
  }

  private async translateViaHosted(
    servicesUrl: string, apiKey: string,
    identifier: string, contextCode: string, targetLanguage: string
  ): Promise<string | null> {
    if (!servicesUrl) {
      return null;
    }
    const body = {
      identifier,
      context_code: contextCode,
      target_language: targetLanguage,
      source_language: 'csharp',
    };
    const json: unknown = await this.postJson(
      this.join(servicesUrl, '/translate/identifier'),
      body,
      { 'X-Api-Key': apiKey }
    );
    const translation: unknown = (json as { translation?: unknown } | null)?.translation;
    return typeof translation === 'string' && translation.length > 0 ? translation : null;
  }

  private async translateViaLocal(
    localLlmUrl: string, identifier: string, contextCode: string, targetLanguage: string
  ): Promise<string | null> {
    if (!localLlmUrl) {
      return null;
    }
    const body = {
      model: 'qwen',
      temperature: 0.2,
      messages: [
        { role: 'system', content: buildSystemPrompt(targetLanguage) },
        { role: 'user', content: buildUserPrompt(identifier, contextCode) },
      ],
    };
    const json: unknown = await this.postJson(
      this.join(localLlmUrl, '/v1/chat/completions'), body, {}
    );
    const content: unknown =
      (json as { choices?: Array<{ message?: { content?: unknown } }> } | null)
        ?.choices?.[0]?.message?.content;
    if (typeof content !== 'string') {
      return null;
    }
    const cleaned: string = postProcess(content);
    return cleaned.length > 0 ? cleaned : null;
  }

  private async postJson(url: string, body: unknown, headers: Record<string, string>): Promise<unknown> {
    return this.fetchJson(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...headers },
      body: JSON.stringify(body),
    });
  }

  private async getJson(url: string, headers: Record<string, string>): Promise<unknown> {
    return this.fetchJson(url, { method: 'GET', headers });
  }

  /** Performs a fetch with a timeout; returns parsed JSON or `null` on any failure. Never throws. */
  private async fetchJson(url: string, init: RequestInit): Promise<unknown> {
    const controller: AbortController = new AbortController();
    const timer: ReturnType<typeof setTimeout> = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);
    try {
      const response: Response = await fetch(url, { ...init, signal: controller.signal });
      if (!response.ok) {
        return null;
      }
      return await response.json();
    } catch {
      return null; // timeout, network error, bad JSON — degrade gracefully
    } finally {
      clearTimeout(timer);
    }
  }

  private join(base: string, path: string): string {
    return `${base.replace(/\/+$/, '')}/${path.replace(/^\/+/, '')}`;
  }
}

/** Compact local-mode system prompt (mirrors the backend's intent for offline fallback). */
export function buildSystemPrompt(targetLanguage: string): string {
  return (
    `You translate C# code identifiers into ${targetLanguage}. Translate the meaning of every ` +
    `word and rejoin using the input's casing (PascalCase/camelCase; an "I"-prefixed interface ` +
    `keeps the I). Keep acronyms (HTTP, XML, JSON, SQL, URL, API, ID) unchanged. Output ONLY the ` +
    `translated identifier — no quotes, no explanation.`
  );
}

/** Builds the user prompt, optionally including disambiguation context. */
export function buildUserPrompt(identifier: string, contextCode: string): string {
  const context: string = contextCode.trim();
  return context
    ? `Identifier: ${identifier}\nContext:\n${context}`
    : `Identifier: ${identifier}`;
}

/** Mirrors the backend post-processing: first line, trimmed of wrapping quotes/punctuation. */
export function postProcess(raw: string): string {
  const firstLine: string = raw.replace(/\r/g, '').split('\n', 1)[0] ?? '';
  return firstLine.trim().replace(/^[`"'.,;:]+|[`"'.,;:]+$/g, '').trim();
}
