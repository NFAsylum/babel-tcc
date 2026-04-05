import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { spawn, ChildProcess } from 'child_process';
import * as readline from 'readline';

const DEFAULT_TIMEOUT_MS = 10000;
const MAX_CRASHES = 3;
const DISPOSE_TIMEOUT_MS = 2000;

/** Response object returned by the .NET Core process. */
export interface CoreResponse {
  success: boolean;
  result: string;
  error: string;
}

/** Result of a syntax validation check. */
export interface ValidationResult {
  isValid: boolean;
  diagnostics: DiagnosticEntry[];
}

/** A single diagnostic entry from syntax validation. */
export interface DiagnosticEntry {
  severity: string;
  message: string;
  line: number;
  column: number;
}

/** Bridge to the .NET Core translation engine using a persistent long-lived process. */
export class CoreBridge {
  public coreDllPath: string;
  public translationsPath: string;
  public projectPath: string;
  public outputChannel: vscode.OutputChannel;
  public timeoutMs: number;

  private process: ChildProcess | null = null;
  private responseReader: readline.Interface | null = null;
  private pendingResolve: ((line: string) => void) | null = null;
  private requestQueue: Promise<void> = Promise.resolve();
  private crashCount: number = 0;
  private consecutiveTimeouts: number = 0;
  private disposed: boolean = false;

  constructor(
    context: vscode.ExtensionContext,
    outputChannel: vscode.OutputChannel,
    timeoutMs: number = DEFAULT_TIMEOUT_MS
  ) {
    this.coreDllPath = path.join(
      context.extensionPath,
      'bin',
      'MultiLingualCode.Core.Host.dll'
    );
    this.projectPath = '';
    this.outputChannel = outputChannel;
    this.timeoutMs = timeoutMs;

    const workspaceFolders: readonly vscode.WorkspaceFolder[] | undefined =
      vscode.workspace.workspaceFolders;
    if (workspaceFolders && workspaceFolders.length > 0) {
      this.projectPath = workspaceFolders[0].uri.fsPath;
    }

    this.translationsPath = this.resolveTranslationsPath(context);
  }

  /** Starts the persistent .NET Core process if not already running. */
  public startProcess(): void {
    if (this.process) {
      return;
    }

    const args: string[] = [this.coreDllPath];

    if (this.translationsPath) {
      args.push('--translations', this.translationsPath);
    }
    if (this.projectPath) {
      args.push('--project', this.projectPath);
    }

    this.outputChannel.appendLine('CoreBridge: starting persistent process');
    const coreProcess: ChildProcess = spawn('dotnet', args);

    coreProcess.on('exit', (code: number | null): void => {
      this.outputChannel.appendLine(`CoreBridge: process exited with code ${code}`);
      this.process = null;
      this.responseReader = null;
      this.pendingResolve = null;

      if (!this.disposed) {
        this.crashCount++;
      }
    });

    coreProcess.on('error', (err: Error): void => {
      this.outputChannel.appendLine(`CoreBridge: process error - ${err.message}`);
      this.process = null;
      this.responseReader = null;
      this.pendingResolve = null;

      if (!this.disposed) {
        this.crashCount++;
      }
    });

    if (coreProcess.stderr) {
      coreProcess.stderr.on('data', (data: Buffer): void => {
        this.outputChannel.appendLine(`CoreBridge stderr: ${data.toString()}`);
      });
    }

    this.responseReader = readline.createInterface({
      input: coreProcess.stdout!,
      terminal: false
    });

    this.responseReader.on('line', (line: string): void => {
      if (this.pendingResolve) {
        const resolve: (line: string) => void = this.pendingResolve;
        this.pendingResolve = null;
        resolve(line);
      }
    });

    this.process = coreProcess;
    this.crashCount = 0;
    this.consecutiveTimeouts = 0;
  }

  /** Ensures the process is running, restarting if necessary (crash recovery). */
  private ensureProcess(): void {
    if (this.process) {
      return;
    }

    if (this.crashCount >= MAX_CRASHES) {
      vscode.window.showWarningMessage(
        'Babel TCC: The translation engine is unstable. Check the Output panel for details.'
      );
      throw new Error('CoreBridge: process crashed too many times, refusing to restart');
    }

    this.outputChannel.appendLine('CoreBridge: restarting process after crash');
    this.startProcess();
  }

  /**
   * Sends a request to the persistent process and waits for a response.
   * This is the internal method — callers use invokeCore() which handles queueing.
   */
  private sendRequest(method: string, params: Record<string, unknown>): Promise<CoreResponse> {
    return new Promise<CoreResponse>((resolve, reject): void => {
      try {
        this.ensureProcess();
      } catch (err: unknown) {
        reject(err);
        return;
      }

      if (!this.process || !this.process.stdin) {
        reject(new Error('CoreBridge: process not available'));
        return;
      }

      const requestLine: string = JSON.stringify({ method, params }) + '\n';
      this.outputChannel.appendLine(`CoreBridge: invoking ${method}`);

      const timeoutHandle: NodeJS.Timeout = setTimeout((): void => {
        this.pendingResolve = null;
        this.consecutiveTimeouts++;

        if (this.consecutiveTimeouts >= 2 && this.process) {
          this.outputChannel.appendLine('CoreBridge: 2 consecutive timeouts, killing process');
          this.process.kill();
        }

        reject(new Error(`CoreBridge: timeout after ${this.timeoutMs}ms for method ${method}`));
      }, this.timeoutMs);

      this.pendingResolve = (line: string): void => {
        clearTimeout(timeoutHandle);
        this.consecutiveTimeouts = 0;

        try {
          const response: CoreResponse = JSON.parse(line) as CoreResponse;
          if (!response.success) {
            this.outputChannel.appendLine(`CoreBridge error: ${response.error}`);
            reject(new Error(`CoreBridge: ${response.error}`));
            return;
          }
          this.outputChannel.appendLine(`CoreBridge: ${method} completed successfully`);
          resolve(response);
        } catch (parseErr: unknown) {
          reject(new Error(`CoreBridge: failed to parse response - ${line}`));
        }
      };

      this.process.stdin.write(requestLine);
    });
  }

  /**
   * Enqueues a request to the serial queue. Each request waits for the previous one to complete.
   */
  public invokeCore(method: string, params: Record<string, unknown>): Promise<CoreResponse> {
    return new Promise<CoreResponse>((resolve, reject): void => {
      this.requestQueue = this.requestQueue.then((): Promise<void> => {
        return this.sendRequest(method, params).then(resolve, reject).then((): void => {});
      }).catch((): void => {});
    });
  }

  public async translateToNaturalLanguage(
    sourceCode: string,
    fileExtension: string,
    targetLanguage: string
  ): Promise<string> {
    const params: Record<string, string> = { sourceCode, fileExtension, targetLanguage };
    const response: CoreResponse = await this.invokeCore('TranslateToNaturalLanguage', params);
    return response.result;
  }

  public async translateFromNaturalLanguage(
    translatedCode: string,
    fileExtension: string,
    sourceLanguage: string
  ): Promise<string> {
    const params: Record<string, string> = { translatedCode, fileExtension, sourceLanguage };
    const response: CoreResponse = await this.invokeCore('TranslateFromNaturalLanguage', params);
    return response.result;
  }

  public async applyTranslatedEdits(
    originalCode: string,
    previousTranslatedCode: string,
    editedTranslatedCode: string,
    fileExtension: string,
    sourceLanguage: string
  ): Promise<string> {
    const params: Record<string, string> = {
      originalCode, previousTranslatedCode, editedTranslatedCode, fileExtension, sourceLanguage
    };
    const response: CoreResponse = await this.invokeCore('ApplyTranslatedEdits', params);
    return response.result;
  }

  public async getKeywordMap(
    fileExtension: string,
    targetLanguage: string
  ): Promise<Record<string, string>> {
    const params: Record<string, string> = { fileExtension, targetLanguage };
    const response: CoreResponse = await this.invokeCore('GetKeywordMap', params);
    const parsed: Record<string, string> = JSON.parse(response.result) as Record<string, string>;
    return parsed;
  }

  public async getIdentifierMap(
    targetLanguage: string
  ): Promise<Record<string, string>> {
    const params: Record<string, string> = { targetLanguage };
    const response: CoreResponse = await this.invokeCore('GetIdentifierMap', params);
    const parsed: Record<string, string> = JSON.parse(response.result) as Record<string, string>;
    return parsed;
  }

  public async validateSyntax(
    sourceCode: string,
    fileExtension: string
  ): Promise<ValidationResult> {
    const params: Record<string, string> = { sourceCode, fileExtension };
    const response: CoreResponse = await this.invokeCore('ValidateSyntax', params);
    const parsed: ValidationResult = JSON.parse(response.result) as ValidationResult;
    return parsed;
  }

  public async getSupportedLanguages(): Promise<string[]> {
    const response: CoreResponse = await this.invokeCore('GetSupportedLanguages', {});
    const parsed: string[] = JSON.parse(response.result) as string[];
    return parsed;
  }

  public resolveTranslationsPath(context: vscode.ExtensionContext): string {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration('babel-tcc');
    const configuredPath: string = config.get<string>('translationsPath', '');

    if (configuredPath && fs.existsSync(configuredPath)) {
      this.outputChannel.appendLine(`CoreBridge: translations via setting: ${configuredPath}`);
      return configuredPath;
    }

    if (this.projectPath) {
      const workspaceParent: string = path.dirname(this.projectPath);
      const siblingPath: string = path.join(workspaceParent, 'babel-tcc-translations');

      if (fs.existsSync(siblingPath)) {
        this.outputChannel.appendLine(`CoreBridge: translations auto-detected: ${siblingPath}`);
        return siblingPath;
      }
    }

    const embeddedPath: string = path.join(context.extensionPath, 'translations');
    this.outputChannel.appendLine(`CoreBridge: using embedded translations: ${embeddedPath}`);
    return embeddedPath;
  }

  public dispose(): void {
    this.disposed = true;
    this.outputChannel.appendLine('CoreBridge: disposing');

    if (!this.process) {
      return;
    }

    if (this.process.stdin) {
      this.process.stdin.write(JSON.stringify({ method: 'quit' }) + '\n');
    }

    const killTimeout: NodeJS.Timeout = setTimeout((): void => {
      if (this.process) {
        this.outputChannel.appendLine('CoreBridge: force killing process (quit timeout)');
        this.process.kill();
      }
    }, DISPOSE_TIMEOUT_MS);

    this.process.on('exit', (): void => {
      clearTimeout(killTimeout);
    });
  }
}
