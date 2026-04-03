import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri, workspace, window, FileChangeType } from '../__mocks__/vscode';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme } from '../../src/providers/translatedContentProvider';

describe('TranslatedContentProvider', () => {
  let provider: TranslatedContentProvider;
  let mockCoreBridge: {
    translateToNaturalLanguage: ReturnType<typeof vi.fn>;
    translateFromNaturalLanguage: ReturnType<typeof vi.fn>;
  };
  let mockLanguageDetector: {
    isSupported: ReturnType<typeof vi.fn>;
    getFileExtension: ReturnType<typeof vi.fn>;
    detectLanguage: ReturnType<typeof vi.fn>;
  };
  let mockConfigService: {
    isEnabled: ReturnType<typeof vi.fn>;
    getLanguage: ReturnType<typeof vi.fn>;
  };
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    vi.clearAllMocks();

    mockCoreBridge = {
      translateToNaturalLanguage: vi.fn().mockResolvedValue('publico classe Foo {}'),
      translateFromNaturalLanguage: vi.fn().mockResolvedValue('public class Foo {}'),
    };
    mockLanguageDetector = {
      isSupported: vi.fn().mockReturnValue(true),
      getFileExtension: vi.fn().mockReturnValue('.cs'),
      detectLanguage: vi.fn().mockReturnValue('CSharp'),
    };
    mockConfigService = {
      isEnabled: vi.fn().mockReturnValue(true),
      getLanguage: vi.fn().mockReturnValue('pt-br'),
    };
    outputChannel = { appendLine: vi.fn() };

    vi.mocked(workspace.fs.readFile).mockResolvedValue(
      new TextEncoder().encode('public class Foo {}')
    );
    vi.mocked(workspace.fs.writeFile).mockResolvedValue(undefined);

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
    it('should skip when path is in refreshingPaths', async () => {
      provider.refreshingPaths.add('/test/file.cs');
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      await provider.writeFile(uri, new TextEncoder().encode('test'));
      expect(mockCoreBridge.translateFromNaturalLanguage).not.toHaveBeenCalled();
    });

    it('should reverse translate and write original', async () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const content = new TextEncoder().encode('publico classe Foo {}');

      workspace.textDocuments = [];
      await provider.writeFile(uri, content);

      expect(mockCoreBridge.translateFromNaturalLanguage).toHaveBeenCalledWith(
        'publico classe Foo {}', '.cs', 'pt-br'
      );
      expect(workspace.fs.writeFile).toHaveBeenCalled();
    });

    it('should show error when reverse translation fails', async () => {
      mockCoreBridge.translateFromNaturalLanguage.mockRejectedValue(new Error('fail'));
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);

      await provider.writeFile(uri, new TextEncoder().encode('test'));
      expect(window.showErrorMessage).toHaveBeenCalled();
    });

    it('should clean refreshingPaths even when applyEdit fails', async () => {
      vi.useFakeTimers();
      try {
        const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
        const content = new TextEncoder().encode('publico classe Foo {}');

        // Setup: mock document that triggers the refresh path
        const mockDoc = {
          uri: Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`),
          getText: vi.fn((): string => 'old content'),
          lineAt: vi.fn((): { range: { end: { line: 0, character: 0 } } } =>
            ({ range: { end: { line: 0, character: 0 } } })),
          lineCount: 1,
        };
        workspace.textDocuments = [mockDoc];

        // Make applyEdit reject
        workspace.applyEdit = vi.fn().mockRejectedValue(new Error('applyEdit failed'));

        await provider.writeFile(uri, content);

        // Advance past the setTimeout(100ms)
        await vi.advanceTimersByTimeAsync(200);

        // refreshingPaths should be clean even though applyEdit failed
        expect(provider.refreshingPaths.has('/test/file.cs')).toBe(false);
      } finally {
        vi.useRealTimers();
      }
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
    it('should combine path and language', () => {
      expect(provider.buildCacheKey('/test/file.cs')).toBe('/test/file.cs::pt-br');
    });
  });

  describe('stat', () => {
    it('should call workspace.fs.stat with original file uri', async () => {
      vi.mocked(workspace.fs.stat).mockResolvedValue({ type: 1, ctime: 0, mtime: 0, size: 100 } as any);
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      const stat = await provider.stat(uri);
      expect(workspace.fs.stat).toHaveBeenCalled();
      expect(stat.size).toBe(100);
    });
  });

  describe('watch', () => {
    it('should return a disposable', () => {
      const disposable = provider.watch(Uri.file('/test'), 0, []);
      expect(disposable).toBeDefined();
      expect(typeof disposable.dispose).toBe('function');
    });
  });

  describe('invalidateCache', () => {
    it('should fire change events for both schemes', () => {
      const uri = Uri.parse(`${TRANSLATED_SCHEME}:/test/file.cs`);
      provider.cache.set('/test/file.cs::pt-br', 'cached');

      const events: unknown[] = [];
      provider.onDidChangeFile((e: unknown) => events.push(e));

      provider.invalidateCache(uri);

      expect(events.length).toBe(1);
      const fired = events[0] as Array<{ type: number; uri: { scheme: string } }>;
      const schemes = fired.map(e => e.uri.scheme);
      expect(schemes).toContain(TRANSLATED_SCHEME);
      expect(schemes).toContain(READONLY_SCHEME);
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
