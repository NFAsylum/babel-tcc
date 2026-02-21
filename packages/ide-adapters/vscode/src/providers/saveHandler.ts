import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';
import { TRANSLATED_SCHEME } from './translatedContentProvider';

export class SaveHandler implements vscode.Disposable {
  public coreBridge: CoreBridge;
  public languageDetector: LanguageDetector;
  public configService: ConfigurationService;
  public outputChannel: vscode.OutputChannel;
  public saveSubscription: vscode.Disposable;

  constructor(
    coreBridge: CoreBridge,
    languageDetector: LanguageDetector,
    configService: ConfigurationService,
    outputChannel: vscode.OutputChannel
  ) {
    this.coreBridge = coreBridge;
    this.languageDetector = languageDetector;
    this.configService = configService;
    this.outputChannel = outputChannel;

    this.saveSubscription = vscode.workspace.onWillSaveTextDocument(
      (event: vscode.TextDocumentWillSaveEvent): void => {
        if (event.document.uri.scheme === TRANSLATED_SCHEME) {
          event.waitUntil(this.handleSave(event.document));
        }
      }
    );
  }

  public async handleSave(document: vscode.TextDocument): Promise<vscode.TextEdit[]> {
    const translatedContent: string = document.getText();
    const originalPath: string = document.uri.path;
    const fileExtension: string = this.languageDetector.getFileExtension(originalPath);
    const sourceLanguage: string = this.configService.getLanguage();

    this.outputChannel.appendLine(`SaveHandler: reverse translating ${originalPath}`);

    try {
      const originalCode: string = await this.coreBridge.translateFromNaturalLanguage(
        translatedContent, fileExtension, sourceLanguage
      );

      const originalUri: vscode.Uri = vscode.Uri.file(originalPath);
      const encoder: TextEncoder = new TextEncoder();
      await vscode.workspace.fs.writeFile(originalUri, encoder.encode(originalCode));

      this.outputChannel.appendLine(`SaveHandler: saved original C# to ${originalPath}`);
      vscode.window.showInformationMessage('Babel TCC: File saved successfully.');
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`SaveHandler: reverse translation failed - ${message}`);
      vscode.window.showErrorMessage(
        'Babel TCC: Failed to reverse translate. Original file was NOT overwritten.'
      );
    }

    return [];
  }

  public dispose(): void {
    this.saveSubscription.dispose();
  }
}
