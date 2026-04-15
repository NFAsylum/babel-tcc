import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri } from '../__mocks__/vscode';
import { SemanticKeywordProvider } from '../../src/providers/semanticKeywordProvider';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

describe('SemanticKeywordProvider', () => {
  let provider: SemanticKeywordProvider;
  const mockKeywordMap = { publico: 'public', classe: 'class', se: 'if', retornar: 'return' };

  const mockCategories: Record<string, string> = {
    'public': 'modifier', 'class': 'type', 'if': 'control', 'return': 'control',
  };

  const mockIdentifierMap = { calcularTotal: 'calculateTotal', nomeUsuario: 'userName' };

  const mockKeywordMapService = {
    getMap: vi.fn(() => mockKeywordMap),
    getCategories: vi.fn(() => mockCategories),
    getIdentifierMap: vi.fn(() => mockIdentifierMap),
    dispose: vi.fn(),
  };

  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
  function makeDocument(scheme: string, text: string, ext = '.cs') {
    const lines = text.split('\n');
    return {
      uri: Uri.parse(`${scheme}:/test/file${ext}`),
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

    it('should not highlight keywords inside double-quoted strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'texto msg = "publico classe nao destacar";');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // "publico" and "classe" inside string should NOT be highlighted
      // No keywords outside the string in this line
      expect(result.data.length).toBe(0);
    });

    it('should not highlight keywords inside line comments', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico classe Foo {} // publico classe aqui');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // "publico" and "classe" before comment are highlighted
      // "publico" and "classe" inside comment are NOT
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should not highlight keywords inside single-quoted strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, "texto msg = 'se retornar';");
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // "se" and "retornar" are inside single quotes — should not be highlighted
      expect(result.data.length).toBe(0);
    });

    it('should highlight translated identifiers with variable token type', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'resultado = calcularTotal(nomeUsuario);');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // calcularTotal and nomeUsuario should be highlighted as identifiers (type index 1)
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should not highlight identifiers inside strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'texto msg = "calcularTotal nomeUsuario";');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should return empty when both maps are empty', () => {
      mockKeywordMapService.getMap.mockReturnValueOnce({});
      mockKeywordMapService.getIdentifierMap.mockReturnValueOnce({});
      const doc = makeDocument(TRANSLATED_SCHEME, 'foo bar baz');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should not highlight keywords inside block comments', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, '/* publico classe */ retornar;');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // Only "retornar" outside the block comment should be highlighted
      expect(result.data.length).toBeGreaterThan(0);
      // retornar is at col 21, type 0 (keyword)
      expect(result.data[1]).toBe(21);
    });

    it('should not highlight keywords inside multiline block comments', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, '/* publico\nclasse */ retornar;');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // Line 0: entire line is inside block comment
      // Line 1: "classe" inside comment, "retornar" outside
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should not highlight keywords inside verbatim strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'texto x = @"publico classe";');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should not highlight keywords inside interpolated strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'texto x = $"o {valor} publico";');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should not highlight keywords inside Python comments', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, '# publico classe se', '.py');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(0);
    });

    it('should not highlight keywords inside Python triple-quoted strings', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'x = """publico\nclasse"""\nretornar', '.py');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // Only "retornar" on line 2 should be highlighted
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should not treat # as comment in C# files', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, '#region publico\npublico classe Foo {}');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      // "publico" on line 0 after #region should be highlighted (not a comment in C#)
      // "publico" and "classe" on line 1 should also be highlighted
      expect(result.data.length).toBeGreaterThan(0);
    });

    it('should distinguish keyword tokens from identifier tokens', () => {
      // "publico" is a keyword (public → modifier = 2), "calcularTotal" is an identifier (variable = 5)
      const doc = makeDocument(TRANSLATED_SCHEME, 'publico calcularTotal');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBeGreaterThan(0);
      // Semantic tokens data: [deltaLine, deltaCol, length, tokenType, tokenModifiers]
      expect(result.data[3]).toBe(2); // publico → public → keywordModifier
      expect(result.data[8]).toBe(5); // calcularTotal → variable
    });

    it('should assign different token types per keyword category', () => {
      // se → if → control(0), publico → public → modifier(2), classe → class → type(1)
      const doc = makeDocument(TRANSLATED_SCHEME, 'se publico classe');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBe(15); // 3 tokens × 5 values
      expect(result.data[3]).toBe(0);  // se → if → keywordControl
      expect(result.data[8]).toBe(2);  // publico → public → keywordModifier
      expect(result.data[13]).toBe(1); // classe → class → keywordType
    });

    it('should classify return as control keyword', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'retornar;');
      const result = provider.provideDocumentSemanticTokens(doc as any);
      expect(result.data.length).toBeGreaterThan(0);
      expect(result.data[3]).toBe(0); // retornar → return → keywordControl
    });
  });
});
