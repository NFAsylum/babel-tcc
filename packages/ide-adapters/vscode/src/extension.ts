import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { CoreBridge } from './services/coreBridge';
import { LanguageDetector } from './services/languageDetector';
import { ConfigurationService } from './services/configurationService';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme } from './providers/translatedContentProvider';
import { CompletionProvider } from './providers/completionProvider';
import { HoverProvider } from './providers/hoverProvider';
import { StatusBar } from './ui/statusBar';
import { AutoTranslateManager } from './providers/autoTranslateManager';

const OUTPUT_CHANNEL_NAME = 'Babel TCC';

let outputChannel: vscode.OutputChannel;
let coreBridge: CoreBridge;
let languageDetector: LanguageDetector;
let configService: ConfigurationService;
let translatedContentProvider: TranslatedContentProvider;
let statusBar: StatusBar;
let autoTranslateManager: AutoTranslateManager;

/**
 * Removes the .multilingual cache directory from each workspace folder to prevent stale
 * identifier mappings from persisting between sessions.
 * @param channel - The output channel used for logging cleanup activity.
 */
function cleanMultilingualCache(channel: vscode.OutputChannel): void {
  const workspaceFolders: readonly vscode.WorkspaceFolder[] | undefined =
    vscode.workspace.workspaceFolders;
  if (!workspaceFolders) {
    return;
  }

  for (const folder of workspaceFolders) {
    const cachePath: string = path.join(folder.uri.fsPath, '.multilingual');
    if (fs.existsSync(cachePath)) {
      try {
        fs.rmSync(cachePath, { recursive: true, force: true });
        channel.appendLine(`Cleaned .multilingual cache: ${cachePath}`);
      } catch (err: unknown) {
        const message: string = err instanceof Error ? err.message : String(err);
        channel.appendLine(`Failed to clean .multilingual cache: ${message}`);
      }
    }
  }
}

/**
 * Activates the Babel TCC extension, initializing all services, providers, and commands.
 * @param context - The VS Code extension context used for managing subscriptions and extension paths.
 */
export function activate(context: vscode.ExtensionContext): void {
  outputChannel = vscode.window.createOutputChannel(OUTPUT_CHANNEL_NAME);
  outputChannel.appendLine('Babel TCC extension activated.');

  cleanMultilingualCache(outputChannel);

  coreBridge = new CoreBridge(context, outputChannel);
  languageDetector = new LanguageDetector();
  configService = new ConfigurationService();
  translatedContentProvider = new TranslatedContentProvider(
    coreBridge, languageDetector, configService, outputChannel
  );
  statusBar = new StatusBar(configService);
  autoTranslateManager = new AutoTranslateManager(
    configService, languageDetector, translatedContentProvider, outputChannel
  );

  const providerRegistration: vscode.Disposable = vscode.workspace.registerFileSystemProvider(
    TRANSLATED_SCHEME,
    translatedContentProvider
  );

  const readonlyProviderRegistration: vscode.Disposable = vscode.workspace.registerFileSystemProvider(
    READONLY_SCHEME,
    translatedContentProvider,
    { isReadonly: true }
  );

  const completionProvider: CompletionProvider = new CompletionProvider();
  const completionRegistration: vscode.Disposable = vscode.languages.registerCompletionItemProvider(
    { scheme: TRANSLATED_SCHEME },
    completionProvider
  );
  const completionRegistrationReadonly: vscode.Disposable = vscode.languages.registerCompletionItemProvider(
    { scheme: READONLY_SCHEME },
    completionProvider
  );

  const hoverProviderInstance: HoverProvider = new HoverProvider();
  const hoverRegistration: vscode.Disposable = vscode.languages.registerHoverProvider(
    { scheme: TRANSLATED_SCHEME },
    hoverProviderInstance
  );
  const hoverRegistrationReadonly: vscode.Disposable = vscode.languages.registerHoverProvider(
    { scheme: READONLY_SCHEME },
    hoverProviderInstance
  );

  const fileWatcher: vscode.FileSystemWatcher = vscode.workspace.createFileSystemWatcher('**/*.cs');
  const fileWatcherChangeHandler: vscode.Disposable = fileWatcher.onDidChange(
    (uri: vscode.Uri): void => {
      if (translatedContentProvider.writingPaths.has(uri.path)) {
        return;
      }
      const translatedUri: vscode.Uri = vscode.Uri.parse(`${TRANSLATED_SCHEME}:${uri.path}`);
      translatedContentProvider.invalidateCache(translatedUri);
      outputChannel.appendLine(`Original file changed, refreshing translation: ${uri.fsPath}`);
    }
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
      let languages: string[];
      try {
        languages = await coreBridge.getSupportedLanguages();
      } catch {
        languages = [configService.getLanguage()];
      }
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

  const openTranslatedEditableCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.openTranslatedEditable',
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

      await configService.setReadonly(false);
      const translatedUri: vscode.Uri = vscode.Uri.parse(
        `${TRANSLATED_SCHEME}:${originalUri.path}`
      );

      const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
      await vscode.window.showTextDocument(doc, { preview: false, viewColumn: vscode.ViewColumn.Beside });
      outputChannel.appendLine(`Opened editable translated view for: ${originalUri.fsPath}`);
    }
  );

  const openTranslatedReadonlyCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.openTranslatedReadonly',
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

      await configService.setReadonly(true);
      const translatedUri: vscode.Uri = vscode.Uri.parse(
        `${READONLY_SCHEME}:${originalUri.path}`
      );

      const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
      await vscode.window.showTextDocument(doc, { preview: false, viewColumn: vscode.ViewColumn.Beside });
      outputChannel.appendLine(`Opened readonly translated view for: ${originalUri.fsPath}`);
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
      if (isTranslatedScheme(uri.scheme)) {
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
  context.subscriptions.push(fileWatcher);
  context.subscriptions.push(fileWatcherChangeHandler);
  context.subscriptions.push(providerRegistration);
  context.subscriptions.push(readonlyProviderRegistration);
  context.subscriptions.push(completionRegistration);
  context.subscriptions.push(completionRegistrationReadonly);
  context.subscriptions.push(hoverRegistration);
  context.subscriptions.push(hoverRegistrationReadonly);
  context.subscriptions.push(configService);
  context.subscriptions.push(statusBar);
  context.subscriptions.push(autoTranslateManager);
  context.subscriptions.push(toggleCommand);
  context.subscriptions.push(selectLanguageCommand);
  context.subscriptions.push(openTranslatedEditableCommand);
  context.subscriptions.push(openTranslatedReadonlyCommand);
  context.subscriptions.push(showOriginalCommand);

  outputChannel.appendLine('All commands registered successfully.');
}

/** Deactivates the extension, disposing of the content provider, core bridge, and output channel. */
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
