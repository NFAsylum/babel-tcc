import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { window, workspace, Uri, __setConfigValue, __clearConfigValues } from '../__mocks__/vscode';
import { StatusBar } from '../../src/ui/statusBar';
import { ConfigurationService } from '../../src/services/configurationService';
import { LanguageDetector } from '../../src/services/languageDetector';

describe('StatusBar', () => {
  let configService: ConfigurationService;
  let statusBar: StatusBar;
  let mockStatusBarItem: { text: string; tooltip: string; command: string; show: ReturnType<typeof vi.fn>; dispose: ReturnType<typeof vi.fn> };
  let configChangeCallback: (event: { affectsConfiguration: (s: string) => boolean }) => void;

  beforeEach(() => {
    vi.clearAllMocks();
    __clearConfigValues();

    __setConfigValue('babel-tcc.enabled', true);
    __setConfigValue('babel-tcc.language', 'pt-br');

    mockStatusBarItem = {
      text: '',
      tooltip: '',
      command: '',
      show: vi.fn(),
      dispose: vi.fn(),
    };
    vi.mocked(window.createStatusBarItem).mockReturnValue(mockStatusBarItem as any);

    vi.mocked(workspace.onDidChangeConfiguration).mockImplementation((cb: any) => {
      configChangeCallback = cb;
      return { dispose: vi.fn() };
    });

    configService = new ConfigurationService();
    statusBar = new StatusBar(configService);
  });

  afterEach(() => {
    statusBar.dispose();
    configService.dispose();
  });

  describe('constructor', () => {
    it('should create status bar item and call show', () => {
      expect(window.createStatusBarItem).toHaveBeenCalled();
      expect(mockStatusBarItem.show).toHaveBeenCalled();
    });

    it('should set command to selectLanguage', () => {
      expect(mockStatusBarItem.command).toBe('babel-tcc.selectLanguage');
    });
  });

  describe('update', () => {
    it('should show globe with language when enabled', () => {
      statusBar.update();
      expect(mockStatusBarItem.text).toBe('$(globe) PT-BR');
      expect(mockStatusBarItem.tooltip).toContain('active');
    });

    it('should show globe OFF when disabled', () => {
      __setConfigValue('babel-tcc.enabled', false);
      statusBar.update();
      expect(mockStatusBarItem.text).toBe('$(globe) OFF');
      expect(mockStatusBarItem.tooltip).toContain('disabled');
    });

    it('should uppercase the language code', () => {
      __setConfigValue('babel-tcc.language', 'es-es');
      statusBar.update();
      expect(mockStatusBarItem.text).toBe('$(globe) ES-ES');
    });
  });

  describe('update with an active file', () => {
    afterEach(() => {
      window.activeTextEditor = undefined;
    });

    it('should show the per-language override of the active file', () => {
      __setConfigValue('babel-tcc.languageOverrides', { CSharp: 'es-es' });
      window.activeTextEditor = { document: { uri: Uri.file('/project/Program.cs') } } as any;
      const detectingStatusBar: StatusBar = new StatusBar(configService, new LanguageDetector());

      detectingStatusBar.update();

      expect(mockStatusBarItem.text).toBe('$(globe) ES-ES');
      detectingStatusBar.dispose();
    });

    it('should fall back to the global language for an unsupported active file', () => {
      __setConfigValue('babel-tcc.languageOverrides', { CSharp: 'es-es' });
      window.activeTextEditor = { document: { uri: Uri.file('/project/notes.txt') } } as any;
      const detectingStatusBar: StatusBar = new StatusBar(configService, new LanguageDetector());

      detectingStatusBar.update();

      expect(mockStatusBarItem.text).toBe('$(globe) PT-BR');
      detectingStatusBar.dispose();
    });
  });

  describe('config change', () => {
    it('should update status bar when config changes', () => {
      __setConfigValue('babel-tcc.enabled', false);
      configChangeCallback({ affectsConfiguration: (s: string) => s === 'babel-tcc' });
      expect(mockStatusBarItem.text).toBe('$(globe) OFF');
    });
  });

  // The $(globe) codicon is not read by screen readers: without an accessible label the item is
  // announced as just "PT-BR", with no hint of what it is or that it can be activated.
  describe('accessibility', () => {
    it('should name the item for the status bar context menu', () => {
      expect((mockStatusBarItem as any).name).toBe('Babel TCC');
    });

    it('should announce the item as a button', () => {
      statusBar.update();
      expect((mockStatusBarItem as any).accessibilityInformation.role).toBe('button');
    });

    it('should describe the active state and the language in words, without the codicon', () => {
      statusBar.update();
      const label: string = (mockStatusBarItem as any).accessibilityInformation.label;
      expect(label).toContain('translation active');
      expect(label).toContain('PT-BR');
      expect(label).not.toContain('$(');
    });

    it('should describe the disabled state in words, without the codicon', () => {
      __setConfigValue('babel-tcc.enabled', false);
      statusBar.update();
      const label: string = (mockStatusBarItem as any).accessibilityInformation.label;
      expect(label).toContain('translation disabled');
      expect(label).not.toContain('$(');
    });
  });

  describe('dispose', () => {
    it('should dispose status bar item and config subscription', () => {
      const configDisposeSpy = vi.spyOn(statusBar.configSubscription, 'dispose');
      statusBar.dispose();
      expect(mockStatusBarItem.dispose).toHaveBeenCalled();
      expect(configDisposeSpy).toHaveBeenCalled();
    });
  });
});
