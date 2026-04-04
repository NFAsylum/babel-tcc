import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as fs from 'fs';
import { commands, workspace, window, languages, Uri } from './__mocks__/vscode';

vi.mock('fs');

import { activate, deactivate } from '../src/extension';

function makeContext(): { extensionPath: string; subscriptions: { dispose(): void }[] } {
  return {
    extensionPath: '/ext',
    subscriptions: [] as { dispose(): void }[],
  };
}

describe('extension', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(fs.existsSync).mockReturnValue(false);
    workspace.__configChangeCallbacks = [];

    workspace.workspaceFolders = undefined;
    vi.mocked(workspace.getConfiguration).mockReturnValue({
      get: vi.fn((_key: string, def?: unknown) => def),
      update: vi.fn(),
    } as any);
    vi.mocked(window.onDidChangeActiveTextEditor).mockReturnValue({ dispose: vi.fn() });
  });

  describe('activate', () => {
    it('should register 5 commands', () => {
      const context = makeContext();
      activate(context as any);
      expect(commands.registerCommand).toHaveBeenCalledTimes(5);
    });

    it('should register expected command names', () => {
      const context = makeContext();
      activate(context as any);
      const registeredCommands = vi.mocked(commands.registerCommand).mock.calls.map(c => c[0]);
      expect(registeredCommands).toContain('babel-tcc.toggle');
      expect(registeredCommands).toContain('babel-tcc.selectLanguage');
      expect(registeredCommands).toContain('babel-tcc.openTranslatedEditable');
      expect(registeredCommands).toContain('babel-tcc.openTranslatedReadonly');
      expect(registeredCommands).toContain('babel-tcc.showOriginal');
    });

    it('should register 2 file system providers', () => {
      const context = makeContext();
      activate(context as any);
      expect(workspace.registerFileSystemProvider).toHaveBeenCalledTimes(2);
    });

    it('should register completion and hover providers', () => {
      const context = makeContext();
      activate(context as any);
      expect(languages.registerCompletionItemProvider).toHaveBeenCalledTimes(2);
      expect(languages.registerHoverProvider).toHaveBeenCalledTimes(2);
    });

    it('should push exactly 20 subscriptions to context', () => {
      const context = makeContext();
      activate(context as any);
      expect(context.subscriptions.length).toBe(20);
    });

    it('should create file watcher for .cs and .py files', () => {
      const context = makeContext();
      activate(context as any);
      expect(workspace.createFileSystemWatcher).toHaveBeenCalledWith('**/*.{cs,py}');
    });
  });

  describe('command handlers', () => {
    it('toggle command should flip enabled state', async () => {
      const context = makeContext();
      const updateMock = vi.fn();
      vi.mocked(workspace.getConfiguration).mockReturnValue({
        get: vi.fn((key: string, def?: unknown) => {
          if (key === 'enabled') return true;
          return def;
        }),
        update: updateMock,
      } as any);

      activate(context as any);

      const toggleCall = vi.mocked(commands.registerCommand).mock.calls.find(c => c[0] === 'babel-tcc.toggle');
      expect(toggleCall).toBeDefined();
      await toggleCall![1]();

      expect(updateMock).toHaveBeenCalledWith('enabled', false, expect.anything());
    });

    it('showOriginal command should warn when no active editor', async () => {
      const context = makeContext();
      activate(context as any);
      window.activeTextEditor = undefined;

      const showOriginalCall = vi.mocked(commands.registerCommand).mock.calls.find(c => c[0] === 'babel-tcc.showOriginal');
      await showOriginalCall![1]();

      expect(window.showWarningMessage).toHaveBeenCalledWith('Babel TCC: No active editor.');
    });

    it('openTranslatedEditable should warn for unsupported file', async () => {
      const context = makeContext();
      activate(context as any);
      window.activeTextEditor = {
        document: { uri: Uri.file('/test/file.txt') },
      } as any;

      const editableCall = vi.mocked(commands.registerCommand).mock.calls.find(c => c[0] === 'babel-tcc.openTranslatedEditable');
      await editableCall![1]();

      expect(window.showWarningMessage).toHaveBeenCalledWith('Babel TCC: File type not supported for translation.');
    });

    it('openTranslatedReadonly should warn for unsupported file', async () => {
      const context = makeContext();
      activate(context as any);
      window.activeTextEditor = {
        document: { uri: Uri.file('/test/file.txt') },
      } as any;

      const readonlyCall = vi.mocked(commands.registerCommand).mock.calls.find(c => c[0] === 'babel-tcc.openTranslatedReadonly');
      await readonlyCall![1]();

      expect(window.showWarningMessage).toHaveBeenCalledWith('Babel TCC: File type not supported for translation.');
    });
  });

  describe('deactivate', () => {
    it('should not throw when called after activate', () => {
      const context = makeContext();
      activate(context as any);
      expect(() => deactivate()).not.toThrow();
    });

    it('should not throw when called without activate', () => {
      expect(() => deactivate()).not.toThrow();
    });
  });
});
