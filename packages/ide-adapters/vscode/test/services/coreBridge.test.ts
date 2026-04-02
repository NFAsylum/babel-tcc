import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { EventEmitter as NodeEventEmitter } from 'events';
import * as fs from 'fs';
import { workspace, Uri } from '../__mocks__/vscode';

vi.mock('fs');
vi.mock('child_process', () => ({
  spawn: vi.fn(),
}));

import { spawn } from 'child_process';
import { CoreBridge } from '../../src/services/coreBridge';

// eslint-disable-next-line @typescript-eslint/explicit-function-return-type
function createMockProcess() {
  const proc = {
    stdout: new NodeEventEmitter(),
    stderr: new NodeEventEmitter(),
    on: vi.fn(),
    kill: vi.fn(),
  };
  proc.on.mockImplementation((event: string, cb: Function) => {
    if (event === 'close') (proc as any)._onClose = cb;
    if (event === 'error') (proc as any)._onError = cb;
    return proc;
  });
  return proc;
}

function makeContext(extensionPath: string = '/ext'): { extensionPath: string; subscriptions: never[] } {
  return {
    extensionPath,
    subscriptions: [],
  };
}

function makeOutputChannel(): { appendLine: ReturnType<typeof vi.fn>; show: ReturnType<typeof vi.fn>; dispose: ReturnType<typeof vi.fn> } {
  return { appendLine: vi.fn(), show: vi.fn(), dispose: vi.fn() };
}

describe('CoreBridge', () => {
  let bridge: CoreBridge;
  let mockProcess: ReturnType<typeof createMockProcess>;

  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();

    mockProcess = createMockProcess();
    vi.mocked(spawn).mockReturnValue(mockProcess as any);
    vi.mocked(fs.existsSync).mockReturnValue(false);

    vi.mocked(workspace.getConfiguration).mockReturnValue({
      get: vi.fn((_key: string, def?: unknown) => def),
      update: vi.fn(),
    } as any);
    workspace.workspaceFolders = undefined;

    bridge = new CoreBridge(makeContext() as any, makeOutputChannel() as any, 5000);
  });

  afterEach(() => {
    vi.useRealTimers();
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
      vi.mocked(fs.existsSync).mockImplementation((p) => {
        return String(p).includes('babel-tcc-translations');
      });

      const b = new CoreBridge(makeContext() as any, makeOutputChannel() as any);
      expect(b.translationsPath).toContain('babel-tcc-translations');
    });

    it('should fall back to embedded translations', () => {
      expect(bridge.translationsPath).toContain('translations');
    });
  });

  describe('invokeCore', () => {
    it('should resolve with CoreResponse on success', async () => {
      const response = JSON.stringify({ success: true, result: '"hello"', error: '' });
      const promise = bridge.invokeCore('TestMethod', { key: 'value' });

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result.success).toBe(true);
      expect(result.result).toBe('"hello"');
    });

    it('should reject when output is empty', async () => {
      const promise = bridge.invokeCore('TestMethod', {});
      (mockProcess as any)._onClose(0);

      await expect(promise).rejects.toThrow('no output');
    });

    it('should reject when response.success is false', async () => {
      const response = JSON.stringify({ success: false, result: '', error: 'something failed' });
      const promise = bridge.invokeCore('TestMethod', {});

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      await expect(promise).rejects.toThrow('something failed');
    });

    it('should throw when stdout contains invalid JSON', async () => {
      const promise = bridge.invokeCore('TestMethod', {});

      mockProcess.stdout.emit('data', Buffer.from('not valid json{{{'));

      // JSON.parse throws synchronously inside the close handler,
      // which becomes an unhandled rejection
      expect(() => (mockProcess as any)._onClose(0)).toThrow(SyntaxError);
    });

    it('should log stderr but not reject on stderr alone', async () => {
      const response = JSON.stringify({ success: true, result: '"ok"', error: '' });
      const promise = bridge.invokeCore('TestMethod', {});

      mockProcess.stderr.emit('data', Buffer.from('warning: something'));
      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result.success).toBe(true);
    });

    it('should reject on timeout and kill process', async () => {
      const promise = bridge.invokeCore('TestMethod', {});

      vi.advanceTimersByTime(6000);

      await expect(promise).rejects.toThrow('timeout');
      expect(mockProcess.kill).toHaveBeenCalled();
    });

    it('should reject on process error', async () => {
      const promise = bridge.invokeCore('TestMethod', {});
      (mockProcess as any)._onError(new Error('ENOENT'));

      await expect(promise).rejects.toThrow('failed to start');
    });

    it('should include translations path in args', () => {
      bridge.invokeCore('Test', {});
      const args = vi.mocked(spawn).mock.calls[0][1];
      expect(args).toContain('--translations');
    });
  });

  describe('translateToNaturalLanguage', () => {
    it('should call invokeCore with correct method', async () => {
      const response = JSON.stringify({ success: true, result: 'publico classe', error: '' });
      const promise = bridge.translateToNaturalLanguage('public class', '.cs', 'pt-br');

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result).toBe('publico classe');
      expect(vi.mocked(spawn).mock.calls[0][1]).toContain('TranslateToNaturalLanguage');
    });
  });

  describe('translateFromNaturalLanguage', () => {
    it('should call invokeCore with correct method', async () => {
      const response = JSON.stringify({ success: true, result: 'public class', error: '' });
      const promise = bridge.translateFromNaturalLanguage('publico classe', '.cs', 'pt-br');

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result).toBe('public class');
    });
  });

  describe('validateSyntax', () => {
    it('should parse validation result', async () => {
      const validation = { isValid: true, diagnostics: [] };
      const response = JSON.stringify({ success: true, result: JSON.stringify(validation), error: '' });
      const promise = bridge.validateSyntax('class Foo {}', '.cs');

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result.isValid).toBe(true);
      expect(result.diagnostics).toEqual([]);
    });
  });

  describe('getSupportedLanguages', () => {
    it('should parse language array', async () => {
      const langs = ['pt-br', 'es-es'];
      const response = JSON.stringify({ success: true, result: JSON.stringify(langs), error: '' });
      const promise = bridge.getSupportedLanguages();

      mockProcess.stdout.emit('data', Buffer.from(response));
      (mockProcess as any)._onClose(0);

      const result = await promise;
      expect(result).toEqual(['pt-br', 'es-es']);
    });
  });

  describe('dispose', () => {
    it('should dispose without error', () => {
      expect(() => bridge.dispose()).not.toThrow();
    });
  });
});
