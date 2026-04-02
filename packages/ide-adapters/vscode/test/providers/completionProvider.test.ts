import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Uri, Position, Range, CompletionItemKind } from '../__mocks__/vscode';
import { CompletionProvider } from '../../src/providers/completionProvider';
import { TRANSLATED_SCHEME } from '../../src/providers/translatedContentProvider';

describe('CompletionProvider', () => {
  let provider: CompletionProvider;
  const mockKeywordMap = { publico: 'public', classe: 'class', vazio: 'void' };

  const mockKeywordMapService = {
    getMap: vi.fn(() => mockKeywordMap),
    dispose: vi.fn(),
  };

  function makeDocument(scheme: string, word: string | null) {
    return {
      uri: Uri.parse(`${scheme}:/test/file.cs`),
      getWordRangeAtPosition: vi.fn(() =>
        word ? new Range(new Position(0, 0), new Position(0, word.length)) : undefined
      ),
      getText: vi.fn(() => word ?? ''),
    };
  }

  beforeEach(() => {
    vi.clearAllMocks();
    provider = new CompletionProvider(mockKeywordMapService as any);
  });

  describe('provideCompletionItems', () => {
    it('should return matching items for translated scheme', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'pub');
      const items = provider.provideCompletionItems(doc as any, new Position(0, 3));
      expect(items.length).toBe(1);
      expect(items[0].label).toBe('publico');
      expect(items[0].kind).toBe(CompletionItemKind.Keyword);
      expect(items[0].detail).toBe('C# keyword: public');
    });

    it('should return empty array for file scheme', () => {
      const doc = makeDocument('file', 'pub');
      const items = provider.provideCompletionItems(doc as any, new Position(0, 3));
      expect(items).toEqual([]);
    });

    it('should return empty array when no word range', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, null);
      const items = provider.provideCompletionItems(doc as any, new Position(0, 0));
      expect(items).toEqual([]);
    });

    it('should return all matching items for common prefix', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'v');
      const items = provider.provideCompletionItems(doc as any, new Position(0, 1));
      expect(items.length).toBe(1);
      expect(items[0].label).toBe('vazio');
    });

    it('should return empty array when no keyword matches', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'xyz');
      const items = provider.provideCompletionItems(doc as any, new Position(0, 3));
      expect(items).toEqual([]);
    });

    it('should set insertText on completion items', () => {
      const doc = makeDocument(TRANSLATED_SCHEME, 'cla');
      const items = provider.provideCompletionItems(doc as any, new Position(0, 3));
      expect(items[0].insertText).toBe('classe');
    });
  });
});
