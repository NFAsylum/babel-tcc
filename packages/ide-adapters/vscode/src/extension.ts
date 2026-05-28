import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { execFile } from 'child_process';
import { CoreBridge } from './services/coreBridge';
import { LanguageDetector } from './services/languageDetector';
import { ConfigurationService } from './services/configurationService';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme, buildTranslatedUri } from './providers/translatedContentProvider';
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
    { label: vscode.l10n.t('$(globe) All languages (global)'), scope: 'global', language: undefined },
  ];
  for (const lang of SUPPORTED_LANGUAGES) {
    const isActive: boolean = lang.name === activeProgrammingLanguage;
    items.push({
      label: isActive
        ? vscode.l10n.t('$(file-code) {0} only (active)', lang.name)
        : vscode.l10n.t('$(file-code) {0} only', lang.name),
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
 * Checks if the .NET 8 runtime is installed and accessible in the PATH.
 * Shows an error message with installation link if not found.
 *
 * The extension ships a framework-dependent build run via `dotnet Host.dll`, so only the
 * .NET 8 runtime is required (not the SDK). We probe with `dotnet --list-runtimes` because
 * `dotnet --version` reports the SDK version and fails on runtime-only installs.
 * @param channel - The output channel for logging.
 */
function checkDotnetInstalled(channel: vscode.OutputChannel): void {
  execFile('dotnet', ['--list-runtimes'], (error: Error | null, stdout: string): void => {
    const hasNet8Runtime: boolean = !error && /^Microsoft\.NETCore\.App 8\./m.test(stdout);
    if (!hasNet8Runtime) {
      channel.appendLine('Babel TCC: .NET 8 runtime not found in PATH.');
      const downloadLabel: string = vscode.l10n.t('Open Download Page');
      vscode.window.showErrorMessage(
        vscode.l10n.t('Babel TCC: .NET 8 Runtime is required but was not found. [Install .NET](https://dotnet.microsoft.com/download/dotnet/8.0)'),
        downloadLabel
      ).then((selection: string | undefined): void => {
        if (selection === downloadLabel) {
          vscode.env.openExternal(vscode.Uri.parse('https://dotnet.microsoft.com/download/dotnet/8.0'));
        }
      });
      return;
    }
    channel.appendLine('Babel TCC: .NET 8 runtime found.');
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
      translatedContentProvider.invalidatePath(uri.path);
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
      vscode.window.showInformationMessage(
        !currentEnabled
          ? vscode.l10n.t('Babel TCC: Translation enabled.')
          : vscode.l10n.t('Babel TCC: Translation disabled.')
      );

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
        placeHolder: vscode.l10n.t('Apply language change to...')
      });
      if (!scopeChoice) {
        return;
      }

      const selected: string | undefined = await vscode.window.showQuickPick(languages, {
        placeHolder: vscode.l10n.t('Select target language for translation')
      });
      if (!selected) {
        return;
      }

      if (scopeChoice.scope === 'language' && scopeChoice.language) {
        await configService.setLanguageOverride(scopeChoice.language, selected);
        outputChannel.appendLine(`Language for ${scopeChoice.language} set to: ${selected}`);
        vscode.window.showInformationMessage(vscode.l10n.t('Babel TCC: Language for {0} set to {1}.', scopeChoice.language, selected));
      } else {
        const overrides: Record<string, string> = configService.getLanguageOverrides();
        const activeOverrides: string[] = Object.entries(overrides)
          .filter(([, value]: [string, string]): boolean => value !== '')
          .map(([key, value]: [string, string]): string => `${key}: ${value}`);

        if (activeOverrides.length > 0) {
          const removeLabel: string = vscode.l10n.t('Remove overrides and apply');
          const keepLabel: string = vscode.l10n.t('Keep overrides');
          const cancelLabel: string = vscode.l10n.t('Cancel');
          const overrideChoice: string | undefined = await vscode.window.showWarningMessage(
            vscode.l10n.t('Babel TCC: {0} language override(s) will block this change: {1}', activeOverrides.length, activeOverrides.join(', ')),
            removeLabel,
            keepLabel,
            cancelLabel
          );

          if (overrideChoice === cancelLabel || overrideChoice === undefined) {
            return;
          }

          if (overrideChoice === removeLabel) {
            await configService.clearLanguageOverrides();
            outputChannel.appendLine('Cleared all language overrides.');
          }
        }

        await configService.setLanguage(selected);
        outputChannel.appendLine(`Language set to: ${selected}`);
        vscode.window.showInformationMessage(vscode.l10n.t('Babel TCC: Language set to {0}.', selected));
      }
    }
  );

  const openTranslatedEditableCommand: vscode.Disposable = vscode.commands.registerCommand(
    COMMANDS.OPEN_TRANSLATED_EDITABLE,
    async (): Promise<void> => {
      const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
      if (!editor) {
        vscode.window.showWarningMessage(vscode.l10n.t('Babel TCC: No active editor.'));
        return;
      }

      const originalUri: vscode.Uri = editor.document.uri;
      if (!languageDetector.isSupported(originalUri.fsPath)) {
        vscode.window.showWarningMessage(vscode.l10n.t('Babel TCC: File type not supported for translation.'));
        return;
      }

      await configService.setReadonly(false);
      const editableLanguage: string = configService.getLanguageForProgrammingLanguage(
        languageDetector.detectLanguage(originalUri.path) || ''
      );
      const translatedUri: vscode.Uri = buildTranslatedUri(TRANSLATED_SCHEME, originalUri.path, editableLanguage);

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
        vscode.window.showWarningMessage(vscode.l10n.t('Babel TCC: No active editor.'));
        return;
      }

      const originalUri: vscode.Uri = editor.document.uri;
      if (!languageDetector.isSupported(originalUri.fsPath)) {
        vscode.window.showWarningMessage(vscode.l10n.t('Babel TCC: File type not supported for translation.'));
        return;
      }

      await configService.setReadonly(true);
      const readonlyLanguage: string = configService.getLanguageForProgrammingLanguage(
        languageDetector.detectLanguage(originalUri.path) || ''
      );
      const translatedUri: vscode.Uri = buildTranslatedUri(READONLY_SCHEME, originalUri.path, readonlyLanguage);

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
        vscode.window.showWarningMessage(vscode.l10n.t('Babel TCC: No active editor.'));
        return;
      }

      const uri: vscode.Uri = editor.document.uri;
      if (isTranslatedScheme(uri.scheme)) {
        const originalUri: vscode.Uri = vscode.Uri.file(uri.path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(originalUri);
        await vscode.window.showTextDocument(doc);
        outputChannel.appendLine(`Showing original for: ${uri.path}`);
      } else {
        vscode.window.showInformationMessage(vscode.l10n.t('Babel TCC: Already viewing original code.'));
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
