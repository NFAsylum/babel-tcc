import * as vscode from 'vscode';
import { KeywordMapService } from './keywordMap';
import { isTranslatedScheme } from './translatedContentProvider';

const TOKEN_TYPES = ['keyword', 'variable'];
const TOKEN_MODIFIERS: string[] = [];

/** Semantic tokens legend used for keyword highlighting in translated documents. */
export const SEMANTIC_TOKENS_LEGEND = new vscode.SemanticTokensLegend(TOKEN_TYPES, TOKEN_MODIFIERS);

/**
 * Checks if a position in a line is inside a string or comment.
 * Scans from the start of the line tracking quote and comment state.
 */
function isInsideStringOrComment(text: string, position: number): boolean {
  let inSingleQuote = false;
  let inDoubleQuote = false;
  let inLineComment = false;

  for (let i = 0; i < position && i < text.length; i++) {
    if (inLineComment) {
      return true;
    }

    const ch: string = text[i];
    const next: string = i + 1 < text.length ? text[i + 1] : '';

    if (!inSingleQuote && !inDoubleQuote) {
      if (ch === '/' && next === '/') {
        inLineComment = true;
        continue;
      }
      if (ch === '"') {
        inDoubleQuote = true;
        continue;
      }
      if (ch === '\'') {
        inSingleQuote = true;
        continue;
      }
    } else if (inDoubleQuote) {
      if (ch === '\\') {
        i++;
        continue;
      }
      if (ch === '"') {
        inDoubleQuote = false;
      }
    } else if (inSingleQuote) {
      if (ch === '\\') {
        i++;
        continue;
      }
      if (ch === '\'') {
        inSingleQuote = false;
      }
    }
  }

  return inSingleQuote || inDoubleQuote || inLineComment;
}

/** Provides semantic tokens for translated keywords, enabling dynamic syntax highlighting. */
export class SemanticKeywordProvider implements vscode.DocumentSemanticTokensProvider {
  private keywordMapService: KeywordMapService;

  constructor(keywordMapService: KeywordMapService) {
    this.keywordMapService = keywordMapService;
  }

  /**
   * Provides semantic tokens for all translated keywords in the document.
   * Skips keywords inside strings and comments.
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

    const identifierMap: Record<string, string> = this.keywordMapService.getIdentifierMap(document.uri.path);
    const translatedIdentifiers: Set<string> = new Set(Object.keys(identifierMap));

    if (translatedKeywords.size === 0 && translatedIdentifiers.size === 0) {
      return builder.build();
    }

    for (let line = 0; line < document.lineCount; line++) {
      const text: string = document.lineAt(line).text;
      const wordRegex: RegExp = /\b[a-zA-ZÀ-ÿ_][a-zA-ZÀ-ÿ0-9_]*\b/g;
      let match: RegExpExecArray | null;

      while ((match = wordRegex.exec(text)) !== null) {
        const word: string = match[0];
        const col: number = match.index;

        if (isInsideStringOrComment(text, col)) {
          continue;
        }

        if (translatedKeywords.has(word.toLowerCase())) {
          builder.push(line, col, word.length, 0);
        } else if (translatedIdentifiers.has(word)) {
          builder.push(line, col, word.length, 1);
        }
      }
    }

    return builder.build();
  }
}
