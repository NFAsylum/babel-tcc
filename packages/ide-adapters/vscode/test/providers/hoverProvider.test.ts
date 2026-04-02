import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri, Position, Range } from '../__mocks__/vscode';
import { HoverProvider } from '../../src/providers/hoverProvider';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

describe('HoverProvider', () => {
  let provider: HoverProvider;
  const mockKeywordMap = { publico: 'public', classe: 'class' };

  const mockKeywordMapService = {
    getMap: vi.fn(() => mockKeywordMap),
    dispose: vi.fn(),
  };

  const mockLanguageDetector = {
    detectLanguage: vi.fn((filePath: string) => {
      if (filePath.endsWith('.py')) return 'Python';
      return 'CSharp';
    }),
    isSupported: vi.fn(() => true),
    getFileExtension: vi.fn((filePath: string) => filePath.slice(filePath.lastIndexOf('.'))),
    getSupportedExtensions: vi.fn(() => ['.cs', '.py']),
    supportedExtensions: { '.cs': 'CSharp', '.py': 'Python' },
  };

  function makeDocument(scheme: string, word: string | null, filePath: string = '/test/file.cs') {
    return {
      uri: Uri.parse(`${scheme}:${filePath}`),
      getWordRangeAtPosition: vi.fn(() =>
        word ? new Range(new Position(0, 0), new Position(0, word.length)) : undefined
      ),
      getText: vi.fn(() => word ?? ''),
    };
  }

  beforeEach(() => {
    vi.clearAllMocks();
    provider = new HoverProvider(mockKeywordMapService as any, mockLanguageDetector as any);
  });

  describe('provideHover', () => {
    it('should return hover with original keyword for known translated keyword', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeDefined();
      expect(hover!.contents.value).toContain('public');
    });

    it('should work with readonly scheme', () => {
      const doc = makeDocument(READONLY_SCHEME, 'classe');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeDefined();
      expect(hover!.contents.value).toContain('class');
    });

    it('should return undefined for unknown word', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'foobar');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeUndefined();
    });

    it('should return undefined for file scheme', () => {
      const doc = makeDocument('file', 'publico');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeUndefined();
    });

    it('should return undefined when no word range', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, null);
      const hover = provider.provideHover(doc as any, new Position(0, 0));
      expect(hover).toBeUndefined();
    });

    it('should match case-insensitively (uppercase input)', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'PUBLICO');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeDefined();
      expect(hover!.contents.value).toContain('public');
    });

    it('should format hover with codeblock and keyword text for C# files', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'classe', '/test/file.cs');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeDefined();
      expect(hover!.contents.value).toContain('```csharp');
      expect(hover!.contents.value).toContain('CSharp keyword: `class`');
    });

    it('should use python language in codeblock for .py files', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'classe', '/test/file.py');
      const hover = provider.provideHover(doc as any, new Position(0, 3));
      expect(hover).toBeDefined();
      expect(hover!.contents.value).toContain('```python');
      expect(hover!.contents.value).toContain('Python keyword: `class`');
    });
  });
});
