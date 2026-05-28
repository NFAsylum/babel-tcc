import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri, workspace, window, FileChangeType } from '../__mocks__/vscode';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme } from '../../src/providers/translatedContentProvider';

describe('TranslatedContentProvider', () => {
  let provider: TranslatedContentProvider;
  let mockCoreBridge: {
    translateToNaturalLanguage: ReturnType<typeof vi.fn>;
    translateFromNaturalLanguage: ReturnType<typeof vi.fn>;
    applyTranslatedEdits: ReturnType<typeof vi.fn>;
  };
  let mockLanguageDetector: {
    isSupported: ReturnType<typeof vi.fn>;
    getFileExtension: ReturnType<typeof vi.fn>;
    detectLanguage: ReturnType<typeof vi.fn>;
  };
  let mockConfigService: {
    isEnabled: ReturnType<typeof vi.fn>;
    getLanguage: ReturnType<typeof vi.fn>;
    getLanguageForProgrammingLanguage: ReturnType<typeof vi.fn>;
  };
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    vi.clearAllMocks();

    mockCoreBridge = {
      translateToNaturalLanguage: vi.fn().mockResolvedValue('publico classe Foo {}'),
      translateFromNaturalLanguage: vi.fn().mockResolvedValue('public class Foo {}'),
      applyTranslatedEdits: vi.fn().mockResolvedValue('public class Foo {}'),
    };
    mockLanguageDetector = {
      isSupported: vi.fn().mockReturnValue(true),
      getFileExtension: vi.fn().mockReturnValue('.cs'),
      detectLanguage: vi.fn().mockReturnValue('CSharp'),
    };
    mockConfigService = {
      isEnabled: vi.fn().mockReturnValue(true),
      getLanguage: vi.fn().mockReturnValue('pt-br'),
      getLanguageForProgrammingLanguage: vi.fn().mockReturnValue('pt-br'),
    };
    outputChannel = { appendLine: vi.fn() };

    vi.mocked(workspace.fs.readFile).mockResolvedValue(
      new TextEncoder().encode('public class Foo {}')
    );
    vi.mocked(workspace.fs.writeFile).mockResolvedValue(undefined);

    const mockOriginalDoc = {
      getText: vi.fn((): string => 'public class Foo {}'),
      lineAt: vi.fn((): { range: { end: { line: number; character: number } } } =>
        ({ range: { end: { line: 0, character: 19 } } })),
      lineCount: 1,
      save: vi.fn().mockResolvedValue(true),
      uri: Uri.file('/test/file.cs'),
    };
    vi.mocked(workspace.openTextDocument).mockResolvedValue(mockOriginalDoc as any);
    vi.mocked(workspace.applyEdit).mockResolvedValue(true);

    provider = new TranslatedContentProvider(
      mockCoreBridge as any,
      mockLanguageDetector as any,
      mockConfigService as any,
      outputChannel as any
    );
  });

  describe('isTranslatedScheme', () => {
    it('should return true for translated scheme', () => {
      expect(isTranslatedScheme(TRANSLATED_SCHEME)).toBe(true);
    });

    it('should return true for readonly scheme', () => {
      expect(isTranslatedScheme(READONLY_SCHEME)).toBe(true);
    });

    it('should return false for file scheme', () => {
      expect(isTranslatedScheme('file')).toBe(false);
    });
  });

  describe('provideContent', () => {
    it('should return cached content on cache hit', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      provider.cache.set('/test/file.cs::pt-br', 'cached content');

      const result = await provider.provideContent(uri);
      expect(result).toBe('cached content');
      expect(mockCoreBridge.translateToNaturalLanguage).not.toHaveBeenCalled();
    });

    it('should return original for unsupported file', async () => {
      mockLanguageDetector.isSupported.mockReturnValue(false);
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.txt`);

      const result = await provider.provideContent(uri);
      expect(result).toBe('public class Foo {}');
      expect(mockCoreBridge.translateToNaturalLanguage).not.toHaveBeenCalled();
    });

    it('should return original when translation is disabled', async () => {
      mockConfigService.isEnabled.mockReturnValue(false);
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      const result = await provider.provideContent(uri);
      expect(result).toBe('public class Foo {}');
    });

    it('should translate and cache for supported file', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      const result = await provider.provideContent(uri);
      expect(result).toBe('publico classe Foo {}');
      expect(provider.cache.has('/test/file.cs::pt-br')).toBe(true);
    });

    it('should return original when translation fails', async () => {
      mockCoreBridge.translateToNaturalLanguage.mockRejectedValue(new Error('fail'));
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      const result = await provider.provideContent(uri);
      expect(result).toBe('public class Foo {}');
    });
  });

  describe('readFile', () => {
    it('should return Uint8Array of translated content', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const result = await provider.readFile(uri);
      const text = new TextDecoder().decode(result);
      expect(text).toBe('publico classe Foo {}');
    });
  });

  describe('writeFile', () => {
    it('should apply translated edits and write original via applyEdit, with no deferred re-render', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const content = new TextEncoder().encode('publico classe Foo {}');

      workspace.textDocuments = [];
      await provider.writeFile(uri, content);

      expect(mockCoreBridge.applyTranslatedEdits).toHaveBeenCalled();
      expect(workspace.applyEdit).toHaveBeenCalled();
      // The save is linear: it reverse-translates and writes the original, and does NOT forward
      // re-translate the buffer (the old setTimeout re-render that caused the language desync).
      expect(mockCoreBridge.translateToNaturalLanguage).not.toHaveBeenCalled();
    });

    it('uses renderedContent as the reverse-translation baseline, immune to cache invalidation', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      // readFile renders the buffer (pt-br) and records that exact text as the baseline.
      await provider.readFile(uri);
      expect(provider.renderedContent.get('/test/file.cs')).toBe('publico classe Foo {}');

      // A language switch / file-watcher clears the cache — but the baseline MUST survive.
      provider.invalidatePath('/test/file.cs');
      expect(provider.cache.has('/test/file.cs::pt-br')).toBe(false);
      expect(provider.renderedContent.get('/test/file.cs')).toBe('publico classe Foo {}');

      // The next save reverse-translates against that baseline, not an empty string (which is what
      // previously made the merge write translated tokens into the original).
      workspace.textDocuments = [];
      await provider.writeFile(uri, new TextEncoder().encode('publico classe Foo editado {}'));
      const call = mockCoreBridge.applyTranslatedEdits.mock.calls[0];
      expect(call[1]).toBe('publico classe Foo {}');
    });

    it('should reverse-translate FROM the displayed language even after a config switch + stat', async () => {
      vi.mocked(workspace.fs.stat).mockResolvedValue({ type: 1, ctime: 0, mtime: 0, size: 100 } as any);
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      // readFile loads the buffer in pt-br and records it as the displayed language.
      await provider.readFile(uri);
      // The user switches the configured language to es-es (the dirty buffer still shows pt-br)...
      mockConfigService.getLanguageForProgrammingLanguage.mockReturnValue('es-es');
      // ...and a stat() happens (VS Code stats frequently, including around save). It must NOT
      // overwrite the displayed language — this is the regression that previously corrupted the file.
      await provider.stat(uri);
      workspace.textDocuments = [];

      await provider.writeFile(uri, new TextEncoder().encode('publico classe Foo {}'));

      // sourceLanguage (5th arg) must be the displayed pt-br, not the new config es-es.
      const call = mockCoreBridge.applyTranslatedEdits.mock.calls[0];
      expect(call[4]).toBe('pt-br');
    });

    it('should drop other-language caches for the path after saving', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      provider.cache.set('/test/file.cs::fr', 'stale fr');
      provider.cache.set('/other/file.cs::pt-br', 'keep');
      workspace.textDocuments = [];

      await provider.writeFile(uri, new TextEncoder().encode('translated'));

      // The original changed on disk, so every cached language for this file is dropped...
      expect(provider.cache.has('/test/file.cs::fr')).toBe(false);
      // ...the current language's cache and baseline are set to exactly what the buffer holds (the
      // saved text), not a re-translation, and other files are untouched.
      expect(provider.cache.get('/test/file.cs::pt-br')).toBe('translated');
      expect(provider.renderedContent.get('/test/file.cs')).toBe('translated');
      expect(provider.cache.has('/other/file.cs::pt-br')).toBe(true);
    });

    it('should show error when reverse translation fails', async () => {
      mockCoreBridge.applyTranslatedEdits.mockRejectedValue(new Error('fail'));
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      await provider.writeFile(uri, new TextEncoder().encode('test'));
      expect(window.showErrorMessage).toHaveBeenCalled();
    });
  });

  describe('invalidateAll', () => {
    it('should clear entire cache', () => {
      provider.cache.set('a', 'x');
      provider.cache.set('b', 'y');

      provider.invalidateAll();

      expect(provider.cache.size).toBe(0);
    });
  });

  describe('buildCacheKey', () => {
    it('should combine path and the configured target language', () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      expect(provider.buildCacheKey(uri)).toBe('/test/file.cs::pt-br');
    });
  });

  describe('stat', () => {
    it('should report the translated content size (not the original) and stat the original', async () => {
      vi.mocked(workspace.fs.stat).mockResolvedValue({ type: 1, ctime: 0, mtime: 0, size: 100 } as any);
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const stat = await provider.stat(uri);
      expect(workspace.fs.stat).toHaveBeenCalled();
      // size MUST match the translated bytes readFile returns, so the change event refreshes editors.
      expect(stat.size).toBe(Buffer.byteLength('publico classe Foo {}', 'utf-8'));
    });
  });

  describe('watch', () => {
    it('should return a disposable', () => {
      const disposable = provider.watch(Uri.file('/test'), 0, []);
      expect(disposable).toBeDefined();
      expect(typeof disposable.dispose).toBe('function');
    });
  });

  describe('invalidatePath', () => {
    it('should clear every cached language for the path and fire events for open views', () => {
      provider.cache.set('/test/file.cs::pt-br', 'a');
      provider.cache.set('/test/file.cs::en', 'b');
      provider.cache.set('/other/file.cs::pt-br', 'c');

      const openDoc = { uri: Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`) };
      workspace.textDocuments = [openDoc];

      const events: unknown[] = [];
      provider.onDidChangeFile((e: unknown) => events.push(e));

      provider.invalidatePath('/test/file.cs');

      expect(provider.cache.has('/test/file.cs::pt-br')).toBe(false);
      expect(provider.cache.has('/test/file.cs::en')).toBe(false);
      expect(provider.cache.has('/other/file.cs::pt-br')).toBe(true);
      expect(events.length).toBe(1);
      const fired = events[0] as Array<{ type: number; uri: { path: string } }>;
      expect(fired[0].uri.path).toBe('/test/file.cs');
      expect(fired[0].type).toBe(FileChangeType.Changed);
    });

    it('should not fire when no open view matches the path', () => {
      provider.cache.set('/test/file.cs::pt-br', 'a');
      workspace.textDocuments = [];

      const events: unknown[] = [];
      provider.onDidChangeFile((e: unknown) => events.push(e));

      provider.invalidatePath('/test/file.cs');

      expect(events.length).toBe(0);
      expect(provider.cache.has('/test/file.cs::pt-br')).toBe(false);
    });
  });

  describe('dispose', () => {
    it('should clear cache on dispose', () => {
      provider.cache.set('key', 'value');
      provider.dispose();
      expect(provider.cache.size).toBe(0);
    });
  });
});
