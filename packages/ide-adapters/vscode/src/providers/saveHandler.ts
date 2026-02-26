import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';
import { TRANSLATED_SCHEME } from './translatedContentProvider';

/** Handles save events on translated documents by reverse-translating and writing the original file. */
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

  /**
   * Reverse-translates the content of a translated document back to original C# and writes it to disk.
   * Shows an information message on success or an error message if reverse translation fails.
   * @param document - The translated virtual document being saved.
   * @returns An empty array of text edits (no in-place edits are applied to the translated document).
   */
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

  /** Disposes of the save event subscription. */
  public dispose(): void {
    this.saveSubscription.dispose();
  }
}
