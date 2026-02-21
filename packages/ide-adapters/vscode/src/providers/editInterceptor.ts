import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';
import { TRANSLATED_SCHEME, TranslatedContentProvider } from './translatedContentProvider';

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

  public dispose(): void {
    this.changeSubscription.dispose();
  }
}
