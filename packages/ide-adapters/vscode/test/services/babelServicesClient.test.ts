import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import {
  BabelServicesClient,
  postProcess,
  buildSystemPrompt,
  buildUserPrompt,
} from '../../src/services/babelServicesClient';
import { __setConfigValue, __clearConfigValues } from '../__mocks__/vscode';

function jsonResponse(body: unknown, ok = true, status = 200): Response {
  return {
    ok,
    status,
    json: () => Promise.resolve(body),
  } as unknown as Response;
}

function chatResponse(content: string): Response {
  return jsonResponse({ choices: [{ message: { role: 'assistant', content } }] });
}

describe('BabelServicesClient', () => {
  const key = async (): Promise<string | undefined> => 'bk_live_test';
  const noKey = async (): Promise<string | undefined> => undefined;

  beforeEach(() => {
    __clearConfigValues();
    __setConfigValue('babel-tcc.local-llm.url', 'http://local:8080');
    __setConfigValue('babel-tcc.services.url', 'http://hosted:5000');
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it('local mode calls the local llama-server and returns the translation', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'local');
    const fetchMock = vi.fn().mockResolvedValue(chatResponse('Calculadora'));
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(noKey).translateIdentifier('Calculator', '', 'pt-br');

    expect(result).toBe('Calculadora');
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock.mock.calls[0][0]).toBe('http://local:8080/v1/chat/completions');
  });

  it('hosted mode with a key calls the backend and returns its translation', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'hosted');
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ translation: 'ProcessadorDePedidos', cached: false }));
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(key).translateIdentifier('OrderProcessor', '', 'pt-br');

    expect(result).toBe('ProcessadorDePedidos');
    expect(fetchMock.mock.calls[0][0]).toBe('http://hosted:5000/translate/identifier');
    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect((init.headers as Record<string, string>)['X-Api-Key']).toBe('bk_live_test');
  });

  it('hosted failure falls back to the local server (graceful degradation)', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'hosted');
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(jsonResponse({}, false, 503)) // hosted down
      .mockResolvedValueOnce(chatResponse('Calculadora')); // local ok
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(key).translateIdentifier('Calculator', '', 'pt-br');

    expect(result).toBe('Calculadora');
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock.mock.calls[0][0]).toBe('http://hosted:5000/translate/identifier');
    expect(fetchMock.mock.calls[1][0]).toBe('http://local:8080/v1/chat/completions');
  });

  it('hosted mode without a key goes straight to local', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'hosted');
    const fetchMock = vi.fn().mockResolvedValue(chatResponse('Calculadora'));
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(noKey).translateIdentifier('Calculator', '', 'pt-br');

    expect(result).toBe('Calculadora');
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock.mock.calls[0][0]).toBe('http://local:8080/v1/chat/completions');
  });

  it('returns null when both hosted and local are unreachable (never throws)', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'hosted');
    const fetchMock = vi.fn().mockRejectedValue(new Error('network down'));
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(key).translateIdentifier('Calculator', '', 'pt-br');

    expect(result).toBeNull();
  });

  it('returns null in local mode when no local URL is configured', async () => {
    __setConfigValue('babel-tcc.llm.mode', 'local');
    __setConfigValue('babel-tcc.local-llm.url', '');
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    const result = await new BabelServicesClient(noKey).translateIdentifier('Calculator', '', 'pt-br');

    expect(result).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('isHostedAvailable reflects backend health', async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse({ status: 'ok', version: '0.1.0', uptimeSeconds: 1 }));
    vi.stubGlobal('fetch', fetchMock);
    expect(await new BabelServicesClient(key).isHostedAvailable()).toBe(true);
    expect(fetchMock.mock.calls[0][0]).toBe('http://hosted:5000/health');
  });
});

describe('helpers', () => {
  it('postProcess strips wrapping noise and keeps the first line', () => {
    expect(postProcess('`Calculadora`')).toBe('Calculadora');
    expect(postProcess('Calculadora\nExplicação em português')).toBe('Calculadora');
    expect(postProcess('  "ProcessadorDePedidos".  ')).toBe('ProcessadorDePedidos');
  });

  it('buildUserPrompt includes context only when present', () => {
    expect(buildUserPrompt('Calculator', '')).toBe('Identifier: Calculator');
    expect(buildUserPrompt('Calculator', 'class X {}')).toContain('Context:');
  });

  it('buildSystemPrompt names the target language', () => {
    expect(buildSystemPrompt('pt-br')).toContain('pt-br');
  });
});
