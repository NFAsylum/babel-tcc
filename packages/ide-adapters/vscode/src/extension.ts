import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { execFile } from 'child_process';
import { CoreBridge } from './services/coreBridge';
import { LanguageDetector } from './services/languageDetector';
import { ConfigurationService } from './services/configurationService';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme } from './providers/translatedContentProvider';
import { CompletionProvider } from './providers/completionProvider';
import { HoverProvider } from './providers/hoverProvider';
import { KeywordMapService } from './providers/keywordMap';
import { StatusBar } from './ui/statusBar';
import { AutoTranslateManager } from './providers/autoTranslateManager';
import { SemanticKeywordProvider, SEMANTIC_TOKENS_LEGEND } from './providers/semanticKeywordProvider';
import { buildFileWatcherPattern, SUPPORTED_LANGUAGES } from './config/languages';
import { COMMANDS } from './config/constants';

const OUTPUT_CHANNEL_NAME = 'Babel TCC';

/** Item in the scope QuickPick for selectLanguage command. */
export type ScopeItem = vscode.QuickPickItem & { scope: 'global' | 'language'; language: string | undefined };

/**
 * Builds the list of scope items for the selectLanguage QuickPick.
 * Lists all registered languages, marking the active one with "(active)".
 */
export function buildScopeItems(activeProgrammingLanguage: string | undefined): ScopeItem[] {
  const items: ScopeItem[] = [
    { label: '$(globe) All languages (global)', scope: 'global', language: undefined },
  ];
  for (const lang of SUPPORTED_LANGUAGES) {
    const isActive: boolean = lang.name === activeProgrammingLanguage;
    items.push({
      label: `$(file-code) ${lang.name} only${isActive ? ' (active)' : ''}`,
      scope: 'language',
      language: lang.name,
    });
  }
  return items;
}

let outputChannel: vscode.OutputChannel;
let coreBridge: CoreBridge;
let languageDetector: LanguageDetector;
let configService: ConfigurationService;
let translatedContentProvider: TranslatedContentProvider;
let statusBar: StatusBar;
let autoTranslateManager: AutoTranslateManager;
let keywordMapService: KeywordMapService;

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
 * Checks if the .NET SDK is installed and accessible in the PATH.
 * Shows an error message with installation link if not found.
 * @param channel - The output channel for logging.
 */
function checkDotnetInstalled(channel: vscode.OutputChannel): void {
  execFile('dotnet', ['--version'], (error: Error | null, stdout: string): void => {
    if (error) {
      channel.appendLine('Babel TCC: .NET SDK not found in PATH.');
      vscode.window.showErrorMessage(
        'Babel TCC: .NET 8 SDK is required but was not found. ' +
        '[Install .NET](https://dotnet.microsoft.com/download/dotnet/8.0)',
        'Open Download Page'
      ).then((selection: string | undefined): void => {
        if (selection === 'Open Download Page') {
          vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download/dotnet/8.0'));
        }
      });
      return;
    }
    channel.appendLine(`Babel TCC: .NET SDK found: ${stdout.trim()}`);
  });
}

/**
 * Activates the Babel TCC extension, initializing all services, providers, and commands.
 * @param context - The VS Code extension context used for managing subscriptions and extension paths.
 */
export function activate(context: vscode.ExtensionContext): void {
  outputChannel = vscode.window.createOutputChannel(OUTPUT_CHANNEL_NAME);
  outputChannel.appendLine('Babel TCC extension activated.');

  cleanMultilingualCache(outputChannel);
  checkDotnetInstalled(outputChannel);

  coreBridge = new CoreBridge(context, outputChannel);
  languageDetector = new LanguageDetector();
  configService = new ConfigurationService();
  translatedContentProvider = new TranslatedContentProvider(
    coreBridge, languageDetector, configService, outputChannel
  );
  statusBar = new StatusBar(configService, languageDetector);
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

  keywordMapService = new KeywordMapService(
    coreBridge, configService, languageDetector, outputChannel
  );
  keywordMapService.warmCache();

  translatedContentProvider.onTranslationComplete = (language: string): void => {
    keywordMapService.refreshIdentifierCache(language);
  };

  const SCHEMES: string[] = [TRANSLATED_SCHEME, READONLY_SCHEME];

  const completionProvider: CompletionProvider = new CompletionProvider(keywordMapService, languageDetector);
  for (const scheme of SCHEMES) {
    context.subscriptions.push(
      vscode.languages.registerCompletionItemProvider({ scheme }, completionProvider)
    );
  }

  const hoverProviderInstance: HoverProvider = new HoverProvider(keywordMapService, languageDetector);
  for (const scheme of SCHEMES) {
    context.subscriptions.push(
      vscode.languages.registerHoverProvider({ scheme }, hoverProviderInstance)
    );
  }

  const semanticKeywordProvider: SemanticKeywordProvider = new SemanticKeywordProvider(keywordMapService);
  for (const scheme of SCHEMES) {
    context.subscriptions.push(
      vscode.languages.registerDocumentSemanticTokensProvider(
        { scheme }, semanticKeywordProvider, SEMANTIC_TOKENS_LEGEND
      )
    );
  }

  const fileWatcher: vscode.FileSystemWatcher = vscode.workspace.createFileSystemWatcher(buildFileWatcherPattern());
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
    COMMANDS.TOGGLE,
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
    COMMANDS.SELECT_LANGUAGE,
    async (): Promise<void> => {
      let languages: string[];
      try {
        languages = await coreBridge.getSupportedLanguages();
      } catch {
        languages = [configService.getLanguage()];
      }

      const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
      const filePath: string = editor ? editor.document.uri.path : '';
      const activeProgrammingLanguage: string | undefined = filePath
        ? languageDetector.detectLanguage(filePath)
        : undefined;

      const scopeItems: ScopeItem[] = buildScopeItems(activeProgrammingLanguage);

      const scopeChoice: ScopeItem | undefined = await vscode.window.showQuickPick(scopeItems, {
        placeHolder: 'Apply language change to...'
      });
      if (!scopeChoice) {
        return;
      }

      const selected: string | undefined = await vscode.window.showQuickPick(languages, {
        placeHolder: 'Select target language for translation'
      });
      if (!selected) {
        return;
      }

      if (scopeChoice.scope === 'language' && scopeChoice.language) {
        await configService.setLanguageOverride(scopeChoice.language, selected);
        outputChannel.appendLine(`Language for ${scopeChoice.language} set to: ${selected}`);
        vscode.window.showInformationMessage(`Babel TCC: Language for ${scopeChoice.language} set to ${selected}.`);
      } else {
        const overrides: Record<string, string> = configService.getLanguageOverrides();
        const activeOverrides: string[] = Object.entries(overrides)
          .filter(([, value]: [string, string]): boolean => value !== '')
          .map(([key, value]: [string, string]): string => `${key}: ${value}`);

        if (activeOverrides.length > 0) {
          const overrideChoice: string | undefined = await vscode.window.showWarningMessage(
            `Babel TCC: ${activeOverrides.length} language override(s) will block this change: ${activeOverrides.join(', ')}`,
            'Remove overrides and apply',
            'Keep overrides',
            'Cancel'
          );

          if (overrideChoice === 'Cancel' || overrideChoice === undefined) {
            return;
          }

          if (overrideChoice === 'Remove overrides and apply') {
            await configService.clearLanguageOverrides();
            outputChannel.appendLine('Cleared all language overrides.');
          }
        }

        await configService.setLanguage(selected);
        outputChannel.appendLine(`Language set to: ${selected}`);
        vscode.window.showInformationMessage(`Babel TCC: Language set to ${selected}.`);
      }
    }
  );

  const openTranslatedEditableCommand: vscode.Disposable = vscode.commands.registerCommand(
    COMMANDS.OPEN_TRANSLATED_EDITABLE,
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
    COMMANDS.OPEN_TRANSLATED_READONLY,
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
    COMMANDS.SHOW_ORIGINAL,
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
  // completion, hover, semantic registrations are pushed in the for-loops above
  context.subscriptions.push(configService);
  context.subscriptions.push(keywordMapService);
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
