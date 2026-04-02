import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { Uri, TabInputText, ViewColumn, window, workspace } from '../__mocks__/vscode';
import { AutoTranslateManager } from '../../src/providers/autoTranslateManager';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

describe('AutoTranslateManager', () => {
  let manager: AutoTranslateManager;
  let mockConfigService: {
    isEnabled: ReturnType<typeof vi.fn>;
    getLanguage: ReturnType<typeof vi.fn>;
    isReadonly: ReturnType<typeof vi.fn>;
    onDidChangeConfiguration: ReturnType<typeof vi.fn>;
  };
  let mockLanguageDetector: {
    isSupported: ReturnType<typeof vi.fn>;
  };
  let mockContentProvider: {
    invalidateAll: ReturnType<typeof vi.fn>;
    invalidateCache: ReturnType<typeof vi.fn>;
  };
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };
  let configChangeListener: () => void;

  beforeEach(() => {
    vi.clearAllMocks();

    mockConfigService = {
      isEnabled: vi.fn().mockReturnValue(true),
      getLanguage: vi.fn().mockReturnValue('pt-br'),
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
      invalidateAll: vi.fn(),
      invalidateCache: vi.fn(),
    };
    outputChannel = { appendLine: vi.fn() };

    vi.mocked(window.onDidChangeActiveTextEditor).mockImplementation((_cb: any) => ({
      dispose: vi.fn(),
    }));

    vi.mocked(workspace.openTextDocument).mockResolvedValue({} as any);
    vi.mocked(window.showTextDocument).mockResolvedValue({} as any);
    window.tabGroups.all = [];
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
  });

  describe('handleConfigChange', () => {
    it('should restore originals when disabled', async () => {
      // Manager starts with enabled=true
      mockConfigService.isEnabled.mockReturnValue(false);
      window.tabGroups.all = [];

      await manager.handleConfigChange();
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('replaced all translated tabs with originals')
      );
    });

    it('should translate tabs when enabled', async () => {
      // Start disabled
      manager.previousEnabled = false;
      mockConfigService.isEnabled.mockReturnValue(true);
      window.tabGroups.all = [];

      await manager.handleConfigChange();
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('replaced all .cs tabs with translated views')
      );
    });

    it('should refresh tabs when language changes', async () => {
      mockConfigService.getLanguage.mockReturnValue('es-es');
      window.tabGroups.all = [];

      await manager.handleConfigChange();
      expect(mockContentProvider.invalidateAll).toHaveBeenCalled();
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

  describe('isTabOpen', () => {
    it('should return true when tab exists', () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const tab = { input: new TabInputText(uri) };
      window.tabGroups.all = [{ tabs: [tab], viewColumn: ViewColumn.One }];

      expect(manager.isTabOpen(uri)).toBe(true);
    });

    it('should return false when tab does not exist', () => {
      window.tabGroups.all = [];
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      expect(manager.isTabOpen(uri)).toBe(false);
    });
  });

  describe('dispose', () => {
    it('should dispose without error', () => {
      expect(() => manager.dispose()).not.toThrow();
    });
  });
});
