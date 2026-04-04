import * as vscode from 'vscode';
import { KeywordMapService } from './keywordMap';
import { isTranslatedScheme } from './translatedContentProvider';

const TOKEN_TYPES = ['keyword'];
const TOKEN_MODIFIERS: string[] = [];

/** Semantic tokens legend used for keyword highlighting in translated documents. */
export const SEMANTIC_TOKENS_LEGEND = new vscode.SemanticTokensLegend(TOKEN_TYPES, TOKEN_MODIFIERS);

/** Provides semantic tokens for translated keywords, enabling dynamic syntax highlighting. */
export class SemanticKeywordProvider implements vscode.DocumentSemanticTokensProvider {
  private keywordMapService: KeywordMapService;

  constructor(keywordMapService: KeywordMapService) {
    this.keywordMapService = keywordMapService;
  }

  /**
   * Provides semantic tokens for all translated keywords in the document.
   * Each translated keyword is tagged as 'keyword' so the VS Code theme
   * highlights it appropriately.
   */
  public provideDocumentSemanticTokens(
    document: vscode.TextDocument
  ): vscode.SemanticTokens {
    const builder: vscode.SemanticTokensBuilder = new vscode.SemanticTokensBuilder(SEMANTIC_TOKENS_LEGEND);

    if (!isTranslatedScheme(document.uri.scheme)) {
      return builder.build();
    }

    const keywordMap: Record<string, string> = this.keywordMapService.getMap(document.uri.path);
    const translatedKeywords: Set<string> = new Set(Object.keys(keywordMap));

    if (translatedKeywords.size === 0) {
      return builder.build();
    }

    for (let line = 0; line < document.lineCount; line++) {
      const text: string = document.lineAt(line).text;
      const words: RegExpMatchArray | null = text.match(/\b[a-zA-ZÀ-ÿ_][a-zA-ZÀ-ÿ0-9_]*\b/g);

      if (!words) {
        continue;
      }

      let searchStart = 0;
      for (const word of words) {
        if (translatedKeywords.has(word.toLowerCase())) {
          const col: number = text.indexOf(word, searchStart);
          if (col >= 0) {
            builder.push(line, col, word.length, 0);
            searchStart = col + word.length;
          }
        }
      }
    }

    return builder.build();
  }
}
