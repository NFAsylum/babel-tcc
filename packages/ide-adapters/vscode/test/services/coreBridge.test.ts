import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { EventEmitter as NodeEventEmitter } from 'events';
import * as fs from 'fs';
import { workspace, window, Uri } from '../__mocks__/vscode';

vi.mock('fs');
vi.mock('child_process', () => ({
  spawn: vi.fn(),
}));
vi.mock('readline', () => ({
  createInterface: vi.fn(),
}));

import { spawn } from 'child_process';
import { createInterface } from 'readline';
import { CoreBridge } from '../../src/services/coreBridge';

// eslint-disable-next-line @typescript-eslint/explicit-function-return-type
function createMockProcess() {
  const stdout = new NodeEventEmitter();
  const stderr = new NodeEventEmitter();
  const stdinWrite = vi.fn();
  const proc = {
    stdin: { write: stdinWrite, end: vi.fn() },
    stdout,
    stderr,
    on: vi.fn(),
    kill: vi.fn(),
    killed: false,
    pid: 1234,
  };
  proc.on.mockImplementation((event: string, cb: Function) => {
    if (event === 'exit') (proc as any)._onExit = cb;
    if (event === 'error') (proc as any)._onError = cb;
    return proc;
  });
  return proc;
}

/**
 * Sets up readline mock. Returns a function to configure auto-responses:
 * when stdin.write is called, the next line callback fires with the response.
 */
function setupReadlineMock(mockProcess: ReturnType<typeof createMockProcess>): {
  setNextResponse: (response: string) => void;
  lineCallback: ((line: string) => void) | null;
} {
  let lineCallback: ((line: string) => void) | null = null;
  let pendingResponse: string | null = null;

  const mockRl = {
    on: vi.fn((event: string, cb: (line: string) => void) => {
      if (event === 'line') lineCallback = cb;
      return mockRl;
    }),
    close: vi.fn(),
  };

  vi.mocked(createInterface).mockReturnValue(mockRl as any);

  // When stdin.write is called, schedule the response via queueMicrotask
  mockProcess.stdin.write.mockImplementation((_data: unknown): boolean => {
    if (pendingResponse !== null) {
      const resp = pendingResponse;
      pendingResponse = null;
      queueMicrotask(() => {
        if (lineCallback) lineCallback(resp);
      });
    }
    return true;
  });

  return {
    setNextResponse: (response: string): void => { pendingResponse = response; },
    get lineCallback(): ((line: string) => void) | null { return lineCallback; },
  };
}

function makeContext(extensionPath: string = '/ext'): { extensionPath: string; subscriptions: never[] } {
  return { extensionPath, subscriptions: [] };
}

function makeOutputChannel(): { appendLine: ReturnType<typeof vi.fn>; show: ReturnType<typeof vi.fn>; dispose: ReturnType<typeof vi.fn> } {
  return { appendLine: vi.fn(), show: vi.fn(), dispose: vi.fn() };
}

describe('CoreBridge', () => {
  let bridge: CoreBridge;
  let mockProcess: ReturnType<typeof createMockProcess>;
  let rl: ReturnType<typeof setupReadlineMock>;

  beforeEach(() => {
    vi.clearAllMocks();

    mockProcess = createMockProcess();
    vi.mocked(spawn).mockReturnValue(mockProcess as any);
    rl = setupReadlineMock(mockProcess);
    vi.mocked(fs.existsSync).mockReturnValue(false);

    vi.mocked(workspace.getConfiguration).mockReturnValue({
      get: vi.fn((_key: string, def?: unknown) => def),
      update: vi.fn(),
    } as any);
    workspace.workspaceFolders = undefined;

    bridge = new CoreBridge(makeContext() as any, makeOutputChannel() as any, 5000);
  });

  describe('constructor', () => {
    it('should set coreDllPath based on extensionPath', () => {
      expect(bridge.coreDllPath).toContain('MultiLingualCode.Core.Host.dll');
    });

    it('should set projectPath from workspace folders', () => {
      workspace.workspaceFolders = [{ uri: Uri.file('/workspace'), name: 'ws', index: 0 }] as any;
      const b = new CoreBridge(makeContext() as any, makeOutputChannel() as any);
      expect(b.projectPath).toBe('/workspace');
    });

    it('should set empty projectPath when no workspace', () => {
      expect(bridge.projectPath).toBe('');
    });
  });

  describe('resolveTranslationsPath', () => {
    it('should use configured path when it exists', () => {
      vi.mocked(workspace.getConfiguration).mockReturnValue({
        get: vi.fn((_key: string, _def?: unknown) => '/custom/translations'),
        update: vi.fn(),
      } as any);
      vi.mocked(fs.existsSync).mockReturnValue(true);

      const b = new CoreBridge(makeContext() as any, makeOutputChannel() as any);
      expect(b.translationsPath).toBe('/custom/translations');
    });

    it('should auto-detect sibling repo', () => {
      workspace.workspaceFolders = [{ uri: Uri.file('/projects/babel-tcc'), name: 'ws', index: 0 }] as any;
      vi.mocked(workspace.getConfiguration).mockReturnValue({
        get: vi.fn((_key: string, def?: unknown) => def),
        update: vi.fn(),
      } as any);
      vi.mocked(fs.existsSync).mockImplementation((p) => String(p).includes('babel-tcc-translations'));

      const b = new CoreBridge(makeContext() as any, makeOutputChannel() as any);
      expect(b.translationsPath).toContain('babel-tcc-translations');
    });

    it('should fall back to embedded translations', () => {
      expect(bridge.translationsPath).toContain('translations');
    });
  });

  describe('startProcess', () => {
    it('should spawn process once', () => {
      bridge.startProcess();
      expect(spawn).toHaveBeenCalledTimes(1);
      expect(vi.mocked(spawn).mock.calls[0][0]).toBe('dotnet');
    });

    it('should not spawn again if already running', () => {
      bridge.startProcess();
      bridge.startProcess();
      expect(spawn).toHaveBeenCalledTimes(1);
    });

    it('should include translations path in spawn args', () => {
      bridge.startProcess();
      const args = vi.mocked(spawn).mock.calls[0][1] as string[];
      expect(args).toContain('--translations');
    });
  });

  describe('invokeCore', () => {
    it('should start process and send request on stdin', async () => {
      rl.setNextResponse(JSON.stringify({ success: true, result: '"hello"', error: '' }));

      const result = await bridge.invokeCore('TestMethod', { key: 'value' });

      expect(spawn).toHaveBeenCalledTimes(1);
      expect(mockProcess.stdin.write).toHaveBeenCalled();
      const written = mockProcess.stdin.write.mock.calls[0][0] as string;
      expect(written).toContain('TestMethod');
      expect(result.success).toBe(true);
      expect(result.result).toBe('"hello"');
    });

    it('should reject when response.success is false', async () => {
      rl.setNextResponse(JSON.stringify({ success: false, result: '', error: 'something failed' }));

      await expect(bridge.invokeCore('TestMethod', {})).rejects.toThrow('something failed');
    });

    it('should reject when response is invalid JSON', async () => {
      rl.setNextResponse('not valid json{{{');

      await expect(bridge.invokeCore('TestMethod', {})).rejects.toThrow('failed to parse');
    });

    it('should reuse process for second request', async () => {
      rl.setNextResponse(JSON.stringify({ success: true, result: '"first"', error: '' }));
      await bridge.invokeCore('Method1', {});

      rl.setNextResponse(JSON.stringify({ success: true, result: '"second"', error: '' }));
      const result2 = await bridge.invokeCore('Method2', {});

      expect(spawn).toHaveBeenCalledTimes(1);
      expect(result2.result).toBe('"second"');
    });

    it('should serialize requests (second waits for first)', async () => {
      const order: string[] = [];
      const resp = JSON.stringify({ success: true, result: '""', error: '' });

      // Queue both responses — the mock stdin.write will consume them in order
      let writeCount = 0;
      mockProcess.stdin.write.mockImplementation((_data: unknown): boolean => {
        writeCount++;
        queueMicrotask(() => {
          if (rl.lineCallback) rl.lineCallback(resp);
        });
        return true;
      });

      const p1 = bridge.invokeCore('First', {}).then(() => { order.push('first'); });
      const p2 = bridge.invokeCore('Second', {}).then(() => { order.push('second'); });

      await p1;
      await p2;

      expect(order).toEqual(['first', 'second']);
      expect(writeCount).toBe(2);
    });
  });

  describe('timeout', () => {
    beforeEach(() => {
      vi.useFakeTimers();
    });

    afterEach(() => {
      vi.useRealTimers();
    });

    it('should reject on timeout without killing process', async () => {
      mockProcess.stdin.write.mockReturnValue(true);
      const promise = bridge.invokeCore('SlowMethod', {});

      const expectation = expect(promise).rejects.toThrow('timeout');
      await vi.advanceTimersByTimeAsync(6000);
      await expectation;

      expect(mockProcess.kill).not.toHaveBeenCalled();
    });

    it('should kill process after 2 consecutive timeouts', async () => {
      mockProcess.stdin.write.mockReturnValue(true);

      const p1 = bridge.invokeCore('Slow1', {});
      const e1 = expect(p1).rejects.toThrow('timeout');
      await vi.advanceTimersByTimeAsync(6000);
      await e1;

      const p2 = bridge.invokeCore('Slow2', {});
      const e2 = expect(p2).rejects.toThrow('timeout');
      await vi.advanceTimersByTimeAsync(6000);
      await e2;

      expect(mockProcess.kill).toHaveBeenCalled();
    });
  });

  describe('crash recovery', () => {
    it('should restart process after crash', async () => {
      bridge.startProcess();
      expect(spawn).toHaveBeenCalledTimes(1);

      // Simulate crash
      (mockProcess as any)._onExit(1);

      // New mock for restarted process
      const newProcess = createMockProcess();
      vi.mocked(spawn).mockReturnValue(newProcess as any);
      const newRl = setupReadlineMock(newProcess);

      newRl.setNextResponse(JSON.stringify({ success: true, result: '"recovered"', error: '' }));
      const result = await bridge.invokeCore('AfterCrash', {});

      expect(spawn).toHaveBeenCalledTimes(2);
      expect(result.result).toBe('"recovered"');
    });

    it('should reject after MAX_CRASHES (3) and show warning', async () => {
      // Simulate 3 crash-restart cycles by manually setting crashCount
      bridge.startProcess();
      (bridge as any).crashCount = 3;
      (bridge as any).process = null;

      await expect(bridge.invokeCore('TooManyCrashes', {}))
        .rejects.toThrow('crashed too many times');
      expect(window.showWarningMessage).toHaveBeenCalled();
    });
  });

  describe('translateToNaturalLanguage', () => {
    it('should call invokeCore with correct method and params', async () => {
      rl.setNextResponse(JSON.stringify({ success: true, result: 'publico classe', error: '' }));

      const result = await bridge.translateToNaturalLanguage('public class', '.cs', 'pt-br');

      expect(result).toBe('publico classe');
      const written = mockProcess.stdin.write.mock.calls[0][0] as string;
      expect(written).toContain('TranslateToNaturalLanguage');
    });
  });

  describe('translateFromNaturalLanguage', () => {
    it('should call invokeCore with correct method', async () => {
      rl.setNextResponse(JSON.stringify({ success: true, result: 'public class', error: '' }));

      const result = await bridge.translateFromNaturalLanguage('publico classe', '.cs', 'pt-br');
      expect(result).toBe('public class');
    });
  });

  describe('validateSyntax', () => {
    it('should parse validation result', async () => {
      const validation = { isValid: true, diagnostics: [] };
      rl.setNextResponse(JSON.stringify({ success: true, result: JSON.stringify(validation), error: '' }));

      const result = await bridge.validateSyntax('class Foo {}', '.cs');
      expect(result.isValid).toBe(true);
      expect(result.diagnostics).toEqual([]);
    });
  });

  describe('getSupportedLanguages', () => {
    it('should parse language array', async () => {
      const langs = ['pt-br', 'es-es'];
      rl.setNextResponse(JSON.stringify({ success: true, result: JSON.stringify(langs), error: '' }));

      const result = await bridge.getSupportedLanguages();
      expect(result).toEqual(['pt-br', 'es-es']);
    });
  });

  describe('dispose', () => {
    it('should send quit command on stdin', () => {
      bridge.startProcess();
      bridge.dispose();
      const writes = mockProcess.stdin.write.mock.calls.map((c: unknown[]) => c[0] as string);
      expect(writes.some((w: string) => w.includes('quit'))).toBe(true);
    });

    it('should force kill if quit does not respond', () => {
      vi.useFakeTimers();
      bridge.startProcess();
      bridge.dispose();

      vi.advanceTimersByTime(3000);
      expect(mockProcess.kill).toHaveBeenCalled();
      vi.useRealTimers();
    });

    it('should not throw when no process running', () => {
      expect(() => bridge.dispose()).not.toThrow();
    });
  });
});
