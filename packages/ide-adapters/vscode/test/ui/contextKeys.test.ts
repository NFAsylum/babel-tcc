import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { window, workspace, Uri, __setConfigValue, __clearConfigValues, __getContextKeys } from '../__mocks__/vscode';
import { ContextKeyManager } from '../../src/ui/contextKeys';
import { ConfigurationService } from '../../src/services/configurationService';
import { LanguageDetector } from '../../src/services/languageDetector';

function makeEditorOn(scheme: string, filePath: string): { document: { uri: Uri } } {
  return { document: { uri: Uri.parse(`${scheme}:${filePath}`) } };
}

describe('ContextKeyManager', () => {
  let configService: ConfigurationService;
  let languageDetector: LanguageDetector;
  let manager: ContextKeyManager;
  let editorChangeCallback: () => void;

  beforeEach(() => {
    vi.clearAllMocks();
    __clearConfigValues();
    workspace.__configChangeCallbacks = [];

    __setConfigValue('babel-tcc.enabled', true);

    window.activeTextEditor = undefined;
    vi.mocked(window.onDidChangeActiveTextEditor).mockImplementation((cb: any) => {
      editorChangeCallback = cb;
      return { dispose: vi.fn() };
    });

    // workspace.onDidChangeConfiguration keeps its default mock on purpose: ConfigurationService
    // listens to it and re-emits on its own emitter, which is what the manager subscribes to.
    configService = new ConfigurationService();
    languageDetector = new LanguageDetector();
  });

  afterEach(() => {
    if (manager) {
      manager.dispose();
    }
    configService.dispose();
  });

  describe('create', () => {
    it('should publish all four context keys', () => {
      manager = ContextKeyManager.create(configService, languageDetector);

      const publishedKeys = __getContextKeys();
      expect(Object.keys(publishedKeys).sort()).toEqual([
        'babelTcc.enabled',
        'babelTcc.readonlyView',
        'babelTcc.supportedFile',
        'babelTcc.translatedView',
      ]);
    });

    it('should subscribe to active editor changes', () => {
      manager = ContextKeyManager.create(configService, languageDetector);

      expect(window.onDidChangeActiveTextEditor).toHaveBeenCalledTimes(1);
    });
  });

  describe('refresh', () => {
    it('should report no supported file when there is no active editor', () => {
      window.activeTextEditor = undefined;

      manager = ContextKeyManager.create(configService, languageDetector);

      const publishedKeys = __getContextKeys();
      expect(publishedKeys['babelTcc.supportedFile']).toBe(false);
      expect(publishedKeys['babelTcc.translatedView']).toBe(false);
      expect(publishedKeys['babelTcc.readonlyView']).toBe(false);
    });

    it('should report a supported file when the active editor holds one', () => {
      window.activeTextEditor = makeEditorOn('file', '/project/Program.cs');

      manager = ContextKeyManager.create(configService, languageDetector);

      expect(__getContextKeys()['babelTcc.supportedFile']).toBe(true);
    });

    it('should not report an unsupported extension as supported', () => {
      window.activeTextEditor = makeEditorOn('file', '/project/notes.txt');

      manager = ContextKeyManager.create(configService, languageDetector);

      expect(__getContextKeys()['babelTcc.supportedFile']).toBe(false);
    });

    it('should report a translated view for the editable translated scheme', () => {
      window.activeTextEditor = makeEditorOn('babel-tcc-translated', '/project/Program.cs');

      manager = ContextKeyManager.create(configService, languageDetector);

      const publishedKeys = __getContextKeys();
      expect(publishedKeys['babelTcc.translatedView']).toBe(true);
      expect(publishedKeys['babelTcc.readonlyView']).toBe(false);
    });

    it('should report both translated and readonly for the readonly scheme', () => {
      window.activeTextEditor = makeEditorOn('babel-tcc-readonly', '/project/Program.cs');

      manager = ContextKeyManager.create(configService, languageDetector);

      const publishedKeys = __getContextKeys();
      expect(publishedKeys['babelTcc.translatedView']).toBe(true);
      expect(publishedKeys['babelTcc.readonlyView']).toBe(true);
    });

    it('should mirror the enabled setting', () => {
      __setConfigValue('babel-tcc.enabled', false);

      manager = ContextKeyManager.create(configService, languageDetector);

      expect(__getContextKeys()['babelTcc.enabled']).toBe(false);
    });

    it('should recompute the keys when the active editor changes', () => {
      window.activeTextEditor = undefined;
      manager = ContextKeyManager.create(configService, languageDetector);
      expect(__getContextKeys()['babelTcc.supportedFile']).toBe(false);

      window.activeTextEditor = makeEditorOn('file', '/project/script.py');
      editorChangeCallback();

      expect(__getContextKeys()['babelTcc.supportedFile']).toBe(true);
    });

    it('should recompute the keys when the configuration changes', () => {
      manager = ContextKeyManager.create(configService, languageDetector);
      expect(__getContextKeys()['babelTcc.enabled']).toBe(true);

      __setConfigValue('babel-tcc.enabled', false);
      workspace.__fireConfigChange('babel-tcc');

      expect(__getContextKeys()['babelTcc.enabled']).toBe(false);
    });
  });

  describe('dispose', () => {
    it('should dispose both subscriptions', () => {
      manager = ContextKeyManager.create(configService, languageDetector);
      const editorDispose = vi.spyOn(manager.editorSubscription, 'dispose');
      const configDispose = vi.spyOn(manager.configSubscription, 'dispose');

      manager.dispose();

      expect(editorDispose).toHaveBeenCalledTimes(1);
      expect(configDispose).toHaveBeenCalledTimes(1);
    });
  });
});
