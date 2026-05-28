import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { Uri, TabInputText, ViewColumn, window, workspace } from '../__mocks__/vscode';
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
    detectLanguage: ReturnType<typeof vi.fn>;
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
      detectLanguage: vi.fn().mockReturnValue('CSharp'),
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
      // A readonly view in a different language is already open for this path.
      const openTab = { input: new TabInputText(Uri.parse(`${READONLY_SCHEME}:/test/file.cs?lang=en`)) };
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

    it('should refresh tabs when language changes', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      const tab = { input: new TabInputText(Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();

      // New-language view opened, then the old-language tab closed.
      expect(workspace.openTextDocument).toHaveBeenCalled();
      expect(window.tabGroups.close).toHaveBeenCalledWith(tab);
    });

    it('should refresh tabs when language override changes', async () => {
      // Global language stays the same, but override for CSharp changes
      mockConfigService.getLanguageForProgrammingLanguage.mockReturnValue('es-es');
      const tab = { input: new TabInputText(Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();

      expect(workspace.openTextDocument).toHaveBeenCalled();
      expect(window.tabGroups.close).toHaveBeenCalledWith(tab);
    });

    it('should open the new-language URI carrying the lang in the query', async () => {
      mockConfigService.getLanguageForProgrammingLanguage.mockReturnValue('es-es');
      const tab = { input: new TabInputText(Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      await manager.handleConfigChange();

      const openedUri = vi.mocked(workspace.openTextDocument).mock.calls[0][0] as { query: string; path: string };
      expect(openedUri.path).toBe('/test/file.cs');
      expect(openedUri.query).toBe('lang=es-es');
    });

    it('should restore focus to the active file after a language switch', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      const tab = { input: new TabInputText(Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`)) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];
      window.activeTextEditor = {
        document: { uri: Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs?lang=es-es`) },
      } as any;

      await manager.handleConfigChange();

      // One showTextDocument for the refresh, one extra for the focus restore.
      expect(vi.mocked(window.showTextDocument).mock.calls.length).toBeGreaterThanOrEqual(2);
    });

    it('should switch scheme when readonly changes', async () => {
      mockConfigService.isReadonly.mockReturnValue(true);
      window.tabGroups.all = [];

      await manager.handleConfigChange();
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('switched tabs')
      );
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
    it('should return true when a translated tab (any scheme/language) is open for the path', () => {
      const tab = { input: new TabInputText(Uri.parse(`${READONLY_SCHEME}:/test/file.cs?lang=en`)) };
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

  describe('dispose', () => {
    it('should dispose without error', () => {
      expect(() => manager.dispose()).not.toThrow();
    });
  });
});
