import * as vscode from 'vscode';
import { isTranslatedScheme } from './translatedContentProvider';
import { KeywordMapService } from './keywordMap';
import { LanguageDetector } from '../services/languageDetector';

/** Maps internal programming language names to VS Code language IDs for syntax highlighting. */
const VSCODE_LANGUAGE_MAP: Record<string, string> = {
  'CSharp': 'csharp',
  'Python': 'python'
};

/** Shows the original keyword when hovering over translated keywords. */
export class HoverProvider implements vscode.HoverProvider {
  private keywordMapService: KeywordMapService;
  private languageDetector: LanguageDetector;

  constructor(keywordMapService: KeywordMapService, languageDetector: LanguageDetector) {
    this.keywordMapService = keywordMapService;
    this.languageDetector = languageDetector;
  }

  /**
   * Provides a hover tooltip displaying the original keyword for a translated keyword under the cursor.
   * Only active in documents using the translated URI scheme.
   * @param document - The text document where the hover was triggered.
   * @param position - The cursor position where the hover was triggered.
   * @returns A Hover with the original keyword, or undefined if the word is not a translated keyword.
   */
  public provideHover(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.Hover | undefined {
    if (!isTranslatedScheme(document.uri.scheme)) {
      return undefined;
    }

    const wordRange: vscode.Range | undefined = document.getWordRangeAtPosition(position);
    if (!wordRange) {
      return undefined;
    }

    const word: string = document.getText(wordRange);
    const keywordMap: Record<string, string> = this.keywordMapService.getMap(document.uri.path);
    const originalKeyword: string | undefined = keywordMap[word.toLowerCase()];

    if (originalKeyword) {
      const detectedLanguage: string | undefined = this.languageDetector.detectLanguage(document.uri.path);
      const vscodeLangId: string = VSCODE_LANGUAGE_MAP[detectedLanguage || 'CSharp'] || 'plaintext';
      const languageLabel: string = detectedLanguage || 'keyword';
      const markdown: vscode.MarkdownString = new vscode.MarkdownString();
      markdown.appendCodeblock(`${originalKeyword}`, vscodeLangId);
      markdown.appendMarkdown(`\n\n${languageLabel} keyword: \`${originalKeyword}\``);
      return new vscode.Hover(markdown, wordRange);
    }

    return undefined;
  }
}
