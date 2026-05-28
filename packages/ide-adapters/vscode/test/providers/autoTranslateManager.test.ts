import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { Uri, TabInputText, ViewColumn, window, workspace, commands } from '../__mocks__/vscode';
import { AutoTranslateManager } from '../../src/providers/autoTranslateManager';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

describe('AutoTranslateManager', () => {
  let manager: AutoTranslateManager;
  let mockConfigService: {
    isEnabled: ReturnType<typeof vi.fn>;
    getLanguage: ReturnType<typeof vi.fn>;
    getLanguageForProgrammingLanguage: ReturnType<typeof vi.fn>;
    isReadonly: ReturnType<typeof vi.fn>;
    onDidChangeConfiguration: ReturnType<typeof vi.fn>;
  };
  let mockLanguageDetector: {
    isSupported: ReturnType<typeof vi.fn>;
  };
  let mockContentProvider: {
    invalidatePath: ReturnType<typeof vi.fn>;
  };
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };
  let configChangeListener: () => void;

  beforeEach(() => {
    vi.clearAllMocks();

    mockConfigService = {
      isEnabled: vi.fn().mockReturnValue(true),
      getLanguage: vi.fn().mockReturnValue('pt-br'),
      getLanguageForProgrammingLanguage: vi.fn().mockReturnValue('pt-br'),
      isReadonly: vi.fn().mockReturnValue(false),
      onDidChangeConfiguration: vi.fn((cb: () => void) => {
        configChangeListener = cb;
        return { dispose: vi.fn() };
      }),
    };
    mockLanguageDetector = {
      isSupported: vi.fn().mockReturnValue(true),
    };
    mockContentProvider = {
      invalidatePath: vi.fn(),
    };
    outputChannel = { appendLine: vi.fn() };

    vi.mocked(window.onDidChangeActiveTextEditor).mockImplementation((_cb: any) => ({
      dispose: vi.fn(),
    }));

    vi.mocked(workspace.openTextDocument).mockResolvedValue({} as any);
    vi.mocked(window.showTextDocument).mockResolvedValue({} as any);
    window.tabGroups.all = [];
    workspace.textDocuments = [];
    window.activeTextEditor = undefined;
    vi.mocked(window.tabGroups.close).mockResolvedValue(undefined as any);

    manager = new AutoTranslateManager(
      mockConfigService as any,
      mockLanguageDetector as any,
      mockContentProvider as any,
      outputChannel as any
    );
  });

  afterEach(() => {
    manager.dispose();
  });

  describe('getActiveScheme', () => {
    it('should return TRANSLATED_SCHEME when not readonly', () => {
      mockConfigService.isReadonly.mockReturnValue(false);
      expect(manager.getActiveScheme()).toBe(TRANSLATED_SCHEME);
    });

    it('should return READONLY_SCHEME when readonly', () => {
      mockConfigService.isReadonly.mockReturnValue(true);
      expect(manager.getActiveScheme()).toBe(READONLY_SCHEME);
    });
  });

  describe('handleActiveEditorChange', () => {
    it('should do nothing when translation is disabled', async () => {
      mockConfigService.isEnabled.mockReturnValue(false);
      const editor = {
        document: { uri: Uri.file('/test/file.cs'), scheme: 'file' },
        viewColumn: ViewColumn.One,
      };
      (editor.document.uri as any).fsPath = '/test/file.cs';

      await manager.handleActiveEditorChange(editor as any);
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });

    it('should do nothing for non-file scheme', async () => {
      const editor = {
        document: { uri: Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`) },
        viewColumn: ViewColumn.One,
      };

      await manager.handleActiveEditorChange(editor as any);
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });

    it('should do nothing for unsupported file', async () => {
      mockLanguageDetector.isSupported.mockReturnValue(false);
      const editor = {
        document: { uri: Uri.file('/test/file.txt') },
        viewColumn: ViewColumn.One,
      };

      await manager.handleActiveEditorChange(editor as any);
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });

    it('should do nothing when uri is already being processed', async () => {
      const uri = Uri.file('/test/file.cs');
      manager.processingUris.add(uri.toString());
      const editor = {
        document: { uri },
        viewColumn: ViewColumn.One,
      };

      await manager.handleActiveEditorChange(editor as any);
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });

    it('should open translated view and close original for supported file', async () => {
      const uri = Uri.file('/test/file.cs');
      const editor = {
        document: { uri },
        viewColumn: ViewColumn.One,
      };
      window.tabGroups.all = [];

      await manager.handleActiveEditorChange(editor as any);

      expect(workspace.openTextDocument).toHaveBeenCalled();
      expect(window.showTextDocument).toHaveBeenCalled();
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('replaced')
      );
      expect(manager.processingUris.has(uri.toString())).toBe(false);
    });

    it('should do nothing when any translated view for the path is already open', async () => {
      const editor = {
        document: { uri: Uri.file('/test/file.cs') },
        viewColumn: ViewColumn.One,
      };
      (editor.document.uri as any).fsPath = '/test/file.cs';
      // A readonly view is already open for this path.
      const openTab = { input: new TabInputText(Uri.parse(`${READONLY_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [openTab], viewColumn: ViewColumn.One }];

      await manager.handleActiveEditorChange(editor as any);
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });
  });

  describe('handleConfigChange', () => {
    it('should restore originals when disabled with open translated tabs', async () => {
      mockConfigService.isEnabled.mockReturnValue(false);
      const translatedUri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(translatedUri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();
      expect(workspace.openTextDocument).toHaveBeenCalled();
      expect(window.showTextDocument).toHaveBeenCalled();
      expect(window.tabGroups.close).toHaveBeenCalledWith(tab);
    });

    it('should translate tabs when enabled with open cs tabs', async () => {
      manager.previousEnabled = false;
      mockConfigService.isEnabled.mockReturnValue(true);
      mockLanguageDetector.isSupported = vi.fn().mockReturnValue(true);
      const fileUri = Uri.file('/test/file.cs');
      const tab = { input: new TabInputText(fileUri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();
      expect(workspace.openTextDocument).toHaveBeenCalled();
      expect(window.tabGroups.close).toHaveBeenCalledWith(tab);
    });

    it('should reload tabs in place via revert (no tab close) when language changes', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(uri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];
      workspace.textDocuments = [{ uri, isDirty: false }];

      await manager.handleConfigChange();

      // The cache is cleared and the open view is reloaded in place by reverting it; no tab is closed.
      expect(mockContentProvider.invalidatePath).toHaveBeenCalledWith('/test/file.cs');
      expect(commands.executeCommand).toHaveBeenCalledWith('workbench.action.files.revert');
      expect(window.tabGroups.close).not.toHaveBeenCalled();
    });

    it('should NOT revert a view with unsaved edits (preserves edits) on language change', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(uri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];
      workspace.textDocuments = [{ uri, isDirty: true }];

      await manager.handleConfigChange();

      // Cache is still cleared, but a dirty view must not be reverted (that would discard edits).
      expect(mockContentProvider.invalidatePath).toHaveBeenCalledWith('/test/file.cs');
      expect(commands.executeCommand).not.toHaveBeenCalledWith('workbench.action.files.revert');
    });

    it('should reload tabs in place when a language override changes', async () => {
      // Global language stays the same, but override for CSharp changes
      mockConfigService.getLanguageForProgrammingLanguage.mockReturnValue('es-es');
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(uri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];
      workspace.textDocuments = [{ uri, isDirty: false }];

      await manager.handleConfigChange();

      expect(mockContentProvider.invalidatePath).toHaveBeenCalledWith('/test/file.cs');
      expect(commands.executeCommand).toHaveBeenCalledWith('workbench.action.files.revert');
    });

    it('should refresh each open file once even across both schemes', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      const editableTab = { input: new TabInputText(Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [editableTab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();

      expect(mockContentProvider.invalidatePath).toHaveBeenCalledTimes(1);
    });

    it('should switch scheme when readonly changes', async () => {
      mockConfigService.isReadonly.mockReturnValue(true);
      window.tabGroups.all = [];

      await manager.handleConfigChange();
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('switched tabs')
      );
    });

    it('should not restore focus to a non-translated active editor on scheme switch', async () => {
      mockConfigService.isReadonly.mockReturnValue(true);
      window.tabGroups.all = [];
      // A plain file (non-translated) is focused when readonly is toggled.
      window.activeTextEditor = { document: { uri: Uri.file('/test/file.cs') } } as any;

      await manager.handleConfigChange();

      // restoreFocus must not open a translated view for the unrelated file.
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });
  });

  describe('findTabsByScheme', () => {
    it('should return tabs matching the scheme', () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(uri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      const results = manager.findTabsByScheme(TRANSLATED_SCHEME);
      expect(results.length).toBe(1);
      expect(results[0].path).toBe('/test/file.cs');
    });

    it('should return empty when no tabs match', () => {
      window.tabGroups.all = [];
      expect(manager.findTabsByScheme(TRANSLATED_SCHEME)).toEqual([]);
    });
  });

  describe('isAnyTranslatedTabOpenForPath', () => {
    it('should return true when a translated tab (any scheme) is open for the path', () => {
      const tab = { input: new TabInputText(Uri.parse(`${READONLY_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      expect(manager.isAnyTranslatedTabOpenForPath('/test/file.cs')).toBe(true);
    });

    it('should return false when only a non-translated tab is open for the path', () => {
      const tab = { input: new TabInputText(Uri.file('/test/file.cs')) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      expect(manager.isAnyTranslatedTabOpenForPath('/test/file.cs')).toBe(false);
    });

    it('should return false when no tab matches the path', () => {
      window.tabGroups.all = [];
      expect(manager.isAnyTranslatedTabOpenForPath('/test/file.cs')).toBe(false);
    });
  });

  describe('confirmUnsavedEditsBeforeLanguageChange', () => {
    function dirtyTranslatedDoc() {
      return {
        uri: Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`),
        isDirty: true,
        save: vi.fn().mockResolvedValue(undefined),
      };
    }

    it('should return true without prompting when there are no unsaved edits', async () => {
      workspace.textDocuments = [];
      const proceed = await manager.confirmUnsavedEditsBeforeLanguageChange();
      expect(proceed).toBe(true);
      expect(window.showWarningMessage).not.toHaveBeenCalled();
    });

    it('should save dirty docs and proceed on "Save and switch"', async () => {
      const doc = dirtyTranslatedDoc();
      workspace.textDocuments = [doc];
      vi.mocked(window.showWarningMessage).mockResolvedValue('Save and switch' as any);

      const proceed = await manager.confirmUnsavedEditsBeforeLanguageChange();

      expect(doc.save).toHaveBeenCalled();
      expect(proceed).toBe(true);
    });

    it('should abort (return false) on Cancel', async () => {
      const doc = dirtyTranslatedDoc();
      workspace.textDocuments = [doc];
      vi.mocked(window.showWarningMessage).mockResolvedValue('Cancel' as any);

      const proceed = await manager.confirmUnsavedEditsBeforeLanguageChange();

      expect(doc.save).not.toHaveBeenCalled();
      expect(proceed).toBe(false);
    });

    it('should revert dirty docs and proceed on "Discard and switch"', async () => {
      const doc = dirtyTranslatedDoc();
      workspace.textDocuments = [doc];
      vi.mocked(window.showWarningMessage).mockResolvedValue('Discard and switch' as any);

      const proceed = await manager.confirmUnsavedEditsBeforeLanguageChange();

      expect(doc.save).not.toHaveBeenCalled();
      expect(commands.executeCommand).toHaveBeenCalledWith('workbench.action.files.revert');
      expect(proceed).toBe(true);
    });
  });

  describe('dispose', () => {
    it('should dispose without error', () => {
      expect(() => manager.dispose()).not.toThrow();
    });
  });
});
