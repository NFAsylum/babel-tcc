import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as fs from 'fs';
import { commands, workspace, window, languages } from './__mocks__/vscode';

vi.mock('fs');

import { activate, deactivate } from '../src/extension';

function makeContext() {
  return {
    extensionPath: '/ext',
    subscriptions: [] as { dispose(): void }[],
  };
}

describe('extension', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(fs.existsSync).mockReturnValue(false);

    workspace.workspaceFolders = undefined;
    vi.mocked(workspace.getConfiguration).mockReturnValue({
      get: vi.fn((_key: string, def?: unknown) => def),
      update: vi.fn(),
    } as any);
    vi.mocked(workspace.onDidChangeConfiguration).mockReturnValue({ dispose: vi.fn() });
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

    it('should push subscriptions to context', () => {
      const context = makeContext();
      activate(context as any);
      expect(context.subscriptions.length).toBeGreaterThan(10);
    });

    it('should create file watcher for .cs files', () => {
      const context = makeContext();
      activate(context as any);
      expect(workspace.createFileSystemWatcher).toHaveBeenCalledWith('**/*.cs');
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
