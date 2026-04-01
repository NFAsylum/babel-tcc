import * as vscode from 'vscode';
import { isTranslatedScheme } from './translatedContentProvider';
import { KeywordMapService } from './keywordMap';

/** Shows the original C# keyword when hovering over translated keywords. */
export class HoverProvider implements vscode.HoverProvider {
  private keywordMapService: KeywordMapService;

  constructor(keywordMapService: KeywordMapService) {
    this.keywordMapService = keywordMapService;
  }

  /**
   * Provides a hover tooltip displaying the original C# keyword for a translated keyword under the cursor.
   * Only active in documents using the translated URI scheme.
   * @param document - The text document where the hover was triggered.
   * @param position - The cursor position where the hover was triggered.
   * @returns A Hover with the original C# keyword, or undefined if the word is not a translated keyword.
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
      const markdown: vscode.MarkdownString = new vscode.MarkdownString();
      markdown.appendCodeblock(`${originalKeyword}`, 'csharp');
      markdown.appendMarkdown(`\n\nC# keyword: \`${originalKeyword}\``);
      return new vscode.Hover(markdown, wordRange);
    }

    return undefined;
  }
}
