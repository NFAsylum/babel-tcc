import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';
import { TRANSLATED_SCHEME, TranslatedContentProvider } from './translatedContentProvider';

/** Intercepts edits in translated documents and invalidates the translation cache. */
export class EditInterceptor implements vscode.Disposable {
  public coreBridge: CoreBridge;
  public languageDetector: LanguageDetector;
  public configService: ConfigurationService;
  public contentProvider: TranslatedContentProvider;
  public outputChannel: vscode.OutputChannel;
  public changeSubscription: vscode.Disposable;

  constructor(
    coreBridge: CoreBridge,
    languageDetector: LanguageDetector,
    configService: ConfigurationService,
    contentProvider: TranslatedContentProvider,
    outputChannel: vscode.OutputChannel
  ) {
    this.coreBridge = coreBridge;
    this.languageDetector = languageDetector;
    this.configService = configService;
    this.contentProvider = contentProvider;
    this.outputChannel = outputChannel;

    this.changeSubscription = vscode.workspace.onDidChangeTextDocument(
      (event: vscode.TextDocumentChangeEvent): void => {
        this.handleDocumentChange(event);
      }
    );
  }

  /**
   * Handles a text document change event. If the change occurred in a translated document,
   * invalidates its cached translation so it will be re-translated on next access.
   * @param event - The document change event from VS Code.
   */
  public handleDocumentChange(event: vscode.TextDocumentChangeEvent): void {
    const document: vscode.TextDocument = event.document;

    if (document.uri.scheme !== TRANSLATED_SCHEME) {
      return;
    }

    if (event.contentChanges.length === 0) {
      return;
    }

    this.outputChannel.appendLine(
      `EditInterceptor: ${event.contentChanges.length} change(s) in translated document`
    );

    const translatedUri: vscode.Uri = vscode.Uri.parse(
      `${TRANSLATED_SCHEME}:${document.uri.path}`
    );
    this.contentProvider.invalidateCache(translatedUri);
  }

  /** Disposes of the document change subscription. */
  public dispose(): void {
    this.changeSubscription.dispose();
  }
}
