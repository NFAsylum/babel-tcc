import * as vscode from 'vscode';
import { KeywordMapService } from './keywordMap';
import { isTranslatedScheme } from './translatedContentProvider';

const TOKEN_TYPES = [
  'keywordControl',   // 0: if, for, while, return, break, etc.
  'keywordType',      // 1: int, string, void, bool, etc.
  'keywordModifier',  // 2: public, static, abstract, etc.
  'keywordLiteral',   // 3: true, false, null/None
  'keywordOther',     // 4: using, namespace, class, new, etc.
  'variable',         // 5: identifiers traduzidos
];
const TOKEN_MODIFIERS: string[] = [];

/** Semantic tokens legend used for keyword highlighting in translated documents. */
export const SEMANTIC_TOKENS_LEGEND = new vscode.SemanticTokensLegend(TOKEN_TYPES, TOKEN_MODIFIERS);

/** Token type index for identifiers traduzidos. */
const TOKEN_VARIABLE = 5;

/**
 * Maps original keyword text to its token type index.
 * Covers C# (89 keywords) and Python (35 keywords).
 * Keywords not in this map fall back to keywordOther (4).
 */
export const KEYWORD_CATEGORIES: Record<string, number> = {
  // --- control (0) ---
  'if': 0, 'else': 0, 'elif': 0, 'for': 0, 'foreach': 0, 'while': 0,
  'do': 0, 'switch': 0, 'case': 0, 'default': 0, 'return': 0, 'break': 0,
  'continue': 0, 'throw': 0, 'try': 0, 'catch': 0, 'except': 0,
  'finally': 0, 'goto': 0, 'yield': 0, 'pass': 0, 'raise': 0, 'with': 0,
  'await': 0,

  // --- type (1) ---
  'void': 1, 'int': 1, 'string': 1, 'bool': 1, 'decimal': 1, 'float': 1,
  'double': 1, 'long': 1, 'short': 1, 'byte': 1, 'sbyte': 1, 'char': 1,
  'object': 1, 'dynamic': 1, 'var': 1, 'uint': 1, 'ulong': 1, 'ushort': 1,
  'class': 1, 'struct': 1, 'interface': 1, 'enum': 1, 'delegate': 1,
  'record': 1, 'def': 1,

  // --- modifier (2) ---
  'public': 2, 'protected': 2, 'private': 2, 'internal': 2, 'static': 2,
  'const': 2, 'readonly': 2, 'virtual': 2, 'override': 2, 'abstract': 2,
  'sealed': 2, 'async': 2, 'partial': 2, 'extern': 2, 'volatile': 2,
  'unsafe': 2, 'fixed': 2, 'required': 2,

  // --- literal (3) ---
  'true': 3, 'false': 3, 'null': 3,
  'True': 3, 'False': 3, 'None': 3,
};

/**
 * Scans a line tracking string/comment state. Returns the state after the line ends
 * and whether each position is inside a string or comment.
 * Handles: line comments (// and # for Python), block comments, regular/verbatim/interpolated strings,
 * and Python triple-quoted strings. Tracks state across lines for multiline constructs.
 */
function scanLine(text: string, inBlockComment: boolean, inBlockString: boolean, blockStringDelimiter: string, isPython: boolean): {
  insideAt: boolean[];
  outBlockComment: boolean;
  outBlockString: boolean;
  outBlockStringDelimiter: string;
} {
  const insideAt: boolean[] = new Array<boolean>(text.length).fill(false);
  let inString = false;
  let inLineComment = false;
  let stringEscapeNext = false;
  let verbatim = false;

  for (let i = 0; i < text.length; i++) {
    const ch: string = text[i];
    const next: string = i + 1 < text.length ? text[i + 1] : '';
    const next2: string = i + 2 < text.length ? text[i + 2] : '';

    if (inBlockString) {
      insideAt[i] = true;
      if (ch + next + next2 === blockStringDelimiter) {
        insideAt[i + 1] = true;
        insideAt[i + 2] = true;
        i += 2;
        inBlockString = false;
        blockStringDelimiter = '';
      }
      continue;
    }

    if (inBlockComment) {
      insideAt[i] = true;
      if (ch === '*' && next === '/') {
        insideAt[i + 1] = true;
        i++;
        inBlockComment = false;
      }
      continue;
    }

    if (inLineComment) {
      insideAt[i] = true;
      continue;
    }

    if (inString) {
      insideAt[i] = true;
      if (stringEscapeNext) {
        stringEscapeNext = false;
        continue;
      }
      if (verbatim) {
        if (ch === '"' && next === '"') {
          insideAt[i + 1] = true;
          i++;
          continue;
        }
        if (ch === '"') {
          inString = false;
          verbatim = false;
        }
      } else {
        if (ch === '\\') {
          stringEscapeNext = true;
          continue;
        }
        if (ch === '"') {
          inString = false;
        }
      }
      continue;
    }

    // Not inside anything — detect starts
    if (ch === '/' && next === '/') {
      inLineComment = true;
      insideAt[i] = true;
      continue;
    }
    if (ch === '/' && next === '*') {
      inBlockComment = true;
      insideAt[i] = true;
      insideAt[i + 1] = true;
      i++;
      continue;
    }
    if (isPython && ch === '#') {
      inLineComment = true;
      insideAt[i] = true;
      continue;
    }

    // Triple-quoted strings (Python)
    if ((ch === '"' && next === '"' && next2 === '"') || (ch === '\'' && next === '\'' && next2 === '\'')) {
      const delim: string = ch + next + next2;
      inBlockString = true;
      blockStringDelimiter = delim;
      insideAt[i] = true;
      insideAt[i + 1] = true;
      insideAt[i + 2] = true;
      i += 2;
      continue;
    }

    // Verbatim/interpolated strings: @", $", $@", @$"
    if ((ch === '@' || ch === '$') && next === '"') {
      inString = true;
      verbatim = ch === '@';
      insideAt[i] = true;
      insideAt[i + 1] = true;
      i++;
      continue;
    }
    if ((ch === '$' && next === '@' && next2 === '"') || (ch === '@' && next === '$' && next2 === '"')) {
      inString = true;
      verbatim = true;
      insideAt[i] = true;
      insideAt[i + 1] = true;
      insideAt[i + 2] = true;
      i += 2;
      continue;
    }

    if (ch === '"') {
      inString = true;
      insideAt[i] = true;
      continue;
    }
    if (ch === '\'') {
      inString = true;
      insideAt[i] = true;
      continue;
    }
  }

  return {
    insideAt,
    outBlockComment: inBlockComment,
    outBlockString: inBlockString,
    outBlockStringDelimiter: blockStringDelimiter,
  };
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

    let inBlockComment = false;
    let inBlockString = false;
    let blockStringDelimiter = '';
    const isPython: boolean = document.uri.path.endsWith('.py');

    for (let line = 0; line < document.lineCount; line++) {
      const text: string = document.lineAt(line).text;
      const scan: ReturnType<typeof scanLine> = scanLine(text, inBlockComment, inBlockString, blockStringDelimiter, isPython);
      inBlockComment = scan.outBlockComment;
      inBlockString = scan.outBlockString;
      blockStringDelimiter = scan.outBlockStringDelimiter;

      const wordRegex: RegExp = /\b[a-zA-ZÀ-ÿ_][a-zA-ZÀ-ÿ0-9_]*\b/g;
      let match: RegExpExecArray | null;

      while ((match = wordRegex.exec(text)) !== null) {
        const word: string = match[0];
        const col: number = match.index;

        if (scan.insideAt[col]) {
          continue;
        }

        const lowerWord: string = word.toLowerCase();
        if (translatedKeywords.has(lowerWord)) {
          const original: string = keywordMap[lowerWord];
          const tokenType: number = KEYWORD_CATEGORIES[original] ?? 4;
          builder.push(line, col, word.length, tokenType);
        } else if (translatedIdentifiers.has(word)) {
          builder.push(line, col, word.length, TOKEN_VARIABLE);
        }
      }
    }

    return builder.build();
  }
}
