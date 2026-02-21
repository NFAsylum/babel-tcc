import * as vscode from 'vscode';
import { CoreBridge } from './services/coreBridge';
import { LanguageDetector } from './services/languageDetector';
import { ConfigurationService } from './services/configurationService';
import { TranslatedContentProvider, TRANSLATED_SCHEME } from './providers/translatedContentProvider';
import { EditInterceptor } from './providers/editInterceptor';
import { SaveHandler } from './providers/saveHandler';
import { CompletionProvider } from './providers/completionProvider';
import { HoverProvider } from './providers/hoverProvider';
import { StatusBar } from './ui/statusBar';

const OUTPUT_CHANNEL_NAME = 'Babel TCC';

let outputChannel: vscode.OutputChannel;
let coreBridge: CoreBridge;
let languageDetector: LanguageDetector;
let configService: ConfigurationService;
let translatedContentProvider: TranslatedContentProvider;
let editInterceptor: EditInterceptor;
let saveHandler: SaveHandler;
let statusBar: StatusBar;

export function activate(context: vscode.ExtensionContext): void {
  outputChannel = vscode.window.createOutputChannel(OUTPUT_CHANNEL_NAME);
  outputChannel.appendLine('Babel TCC extension activated.');

  coreBridge = new CoreBridge(context, outputChannel);
  languageDetector = new LanguageDetector();
  configService = new ConfigurationService();
  translatedContentProvider = new TranslatedContentProvider(
    coreBridge, languageDetector, configService, outputChannel
  );
  editInterceptor = new EditInterceptor(
    coreBridge, languageDetector, configService, translatedContentProvider, outputChannel
  );
  saveHandler = new SaveHandler(
    coreBridge, languageDetector, configService, outputChannel
  );
  statusBar = new StatusBar(configService);

  const providerRegistration: vscode.Disposable = vscode.workspace.registerTextDocumentContentProvider(
    TRANSLATED_SCHEME,
    translatedContentProvider
  );

  const completionRegistration: vscode.Disposable = vscode.languages.registerCompletionItemProvider(
    { scheme: TRANSLATED_SCHEME },
    new CompletionProvider()
  );

  const hoverRegistration: vscode.Disposable = vscode.languages.registerHoverProvider(
    { scheme: TRANSLATED_SCHEME },
    new HoverProvider()
  );

  outputChannel.appendLine('All services and providers initialized.');

  const toggleCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.toggle',
    async (): Promise<void> => {
      const currentEnabled: boolean = configService.isEnabled();
      await configService.setEnabled(!currentEnabled);
      const status: string = !currentEnabled ? 'enabled' : 'disabled';
      outputChannel.appendLine(`Translation ${status}.`);
      vscode.window.showInformationMessage(`Babel TCC: Translation ${status}.`);

      if (!currentEnabled) {
        translatedContentProvider.invalidateAll();
      }
    }
  );

  const selectLanguageCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.selectLanguage',
    async (): Promise<void> => {
      const languages: string[] = ['pt-br'];
      const selected: string | undefined = await vscode.window.showQuickPick(languages, {
        placeHolder: 'Select target language for translation'
      });
      if (selected) {
        await configService.setLanguage(selected);
        translatedContentProvider.invalidateAll();
        outputChannel.appendLine(`Language set to: ${selected}`);
        vscode.window.showInformationMessage(`Babel TCC: Language set to ${selected}.`);
      }
    }
  );

  const openTranslatedCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.openTranslated',
    async (): Promise<void> => {
      const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
      if (!editor) {
        vscode.window.showWarningMessage('Babel TCC: No active editor.');
        return;
      }

      const originalUri: vscode.Uri = editor.document.uri;
      if (!languageDetector.isSupported(originalUri.fsPath)) {
        vscode.window.showWarningMessage('Babel TCC: File type not supported for translation.');
        return;
      }

      const translatedUri: vscode.Uri = vscode.Uri.parse(
        `${TRANSLATED_SCHEME}:${originalUri.path}`
      );

      const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
      await vscode.window.showTextDocument(doc, { preview: false, viewColumn: vscode.ViewColumn.Beside });
      outputChannel.appendLine(`Opened translated view for: ${originalUri.fsPath}`);
    }
  );

  const showOriginalCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.showOriginal',
    async (): Promise<void> => {
      const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
      if (!editor) {
        vscode.window.showWarningMessage('Babel TCC: No active editor.');
        return;
      }

      const uri: vscode.Uri = editor.document.uri;
      if (uri.scheme === TRANSLATED_SCHEME) {
        const originalUri: vscode.Uri = vscode.Uri.file(uri.path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(originalUri);
        await vscode.window.showTextDocument(doc);
        outputChannel.appendLine(`Showing original for: ${uri.path}`);
      } else {
        vscode.window.showInformationMessage('Babel TCC: Already viewing original code.');
      }
    }
  );

  context.subscriptions.push(outputChannel);
  context.subscriptions.push(providerRegistration);
  context.subscriptions.push(completionRegistration);
  context.subscriptions.push(hoverRegistration);
  context.subscriptions.push(configService);
  context.subscriptions.push(editInterceptor);
  context.subscriptions.push(saveHandler);
  context.subscriptions.push(statusBar);
  context.subscriptions.push(toggleCommand);
  context.subscriptions.push(selectLanguageCommand);
  context.subscriptions.push(openTranslatedCommand);
  context.subscriptions.push(showOriginalCommand);

  outputChannel.appendLine('All commands registered successfully.');
}

export function deactivate(): void {
  if (translatedContentProvider) {
    translatedContentProvider.dispose();
  }
  if (coreBridge) {
    coreBridge.dispose();
  }
  if (outputChannel) {
    outputChannel.appendLine('Babel TCC extension deactivated.');
  }
}
