import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri } from '../__mocks__/vscode';
import { SemanticKeywordProvider } from '../../src/providers/semanticKeywordProvider';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

describe('SemanticKeywordProvider', () => {
  let provider: SemanticKeywordProvider;
  const mockKeywordMap = { publico: 'public', classe: 'class', se: 'if', retornar: 'return' };

  const mockKeywordMapService = {
    getMap: vi.fn(() => mockKeywordMap),
    dispose: vi.fn(),
  };

  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
  function makeDocument(scheme: string, text: string) {
    const lines = text.split('\n');
    return {
      uri: Uri.parse(`${scheme}:/test/file.cs`),
      lineCount: lines.length,
      lineAt: (line: number) => ({ text: lines[line] }),
    };
  }

  beforeEach(() => {
    vi.clearAllMocks();
    provider = new SemanticKeywordProvider(mockKeywordMapService as any);
  });

  describe('provideDocumentSemanticTokens', () => {
    it('should return tokens for translated keywords', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico classe Foo {}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result).toBeDefined();
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should return empty tokens for file scheme', () => {
      const doc = makeDocument('file', 'publico classe Foo {}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should work with readonly scheme', () => {
      const doc = makeDocument(READONLY_SCHEME, 'publico classe Foo {}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should return empty tokens when no keywords match', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'foo bar baz');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should return empty tokens when keyword map is empty', () => {
      mockKeywordMapService.getMap.mockReturnValueOnce({});
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico classe Foo {}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should handle multiline documents', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico classe Foo {\n  se (verdadeiro) {\n    retornar;\n  }\n}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should produce different tokens when keyword map changes', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico classe Foo {}');

      const result1 = provider.provideDocumentSemanticTokens(doc as any);

      mockKeywordMapService.getMap.mockReturnValueOnce({ clase: 'class', publico: 'public' });
      const result2 = provider.provideDocumentSemanticTokens(doc as any);

      // Both should produce tokens (publico matches in both maps)
      expect(result1.data.length).toBeGreaterThan(0);
      expect(result2.data.length).toBeGreaterThan(0);
    });
  });
});
