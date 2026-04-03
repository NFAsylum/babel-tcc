import * as vscode from 'vscode';
import { isTranslatedScheme } from './translatedContentProvider';
import { KeywordMapService } from './keywordMap';
import { LanguageDetector } from '../services/languageDetector';

/** Provides autocomplete suggestions for translated keywords in translated documents. */
export class CompletionProvider implements vscode.CompletionItemProvider {
  private keywordMapService: KeywordMapService;
  private languageDetector: LanguageDetector;

  constructor(keywordMapService: KeywordMapService, languageDetector: LanguageDetector) {
    this.keywordMapService = keywordMapService;
    this.languageDetector = languageDetector;
  }

  /**
   * Returns completion items for translated keywords matching the current word prefix.
   * Only active in documents using the translated URI scheme.
   * @param document - The text document in which completion was triggered.
   * @param position - The cursor position where completion was triggered.
   * @returns An array of completion items for matching translated keywords.
   */
  public provideCompletionItems(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.CompletionItem[] {
    if (!isTranslatedScheme(document.uri.scheme)) {
      return [];
    }

    const wordRange: vscode.Range | undefined = document.getWordRangeAtPosition(position);
    if (!wordRange) {
      return [];
    }

    const currentWord: string = document.getText(wordRange).toLowerCase();
    const items: vscode.CompletionItem[] = [];
    const keywordMap: Record<string, string> = this.keywordMapService.getMap(document.uri.path);

    for (const [translated, original] of Object.entries(keywordMap)) {
      if (translated.startsWith(currentWord)) {
        const item: vscode.CompletionItem = new vscode.CompletionItem(
          translated,
          vscode.CompletionItemKind.Keyword
        );
        const detectedLanguage: string = this.languageDetector.detectLanguage(document.uri.path) || 'keyword';
        item.detail = `${detectedLanguage} keyword: ${original}`;
        item.insertText = translated;
        items.push(item);
      }
    }

    return items;
  }
}
