import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME } from './translatedContentProvider';

/** Manages automatic translation of .cs tabs based on the enabled/language configuration. */
export class AutoTranslateManager implements vscode.Disposable {
  public configService: ConfigurationService;
  public languageDetector: LanguageDetector;
  public contentProvider: TranslatedContentProvider;
  public outputChannel: vscode.OutputChannel;
  public processingUris: Set<string> = new Set<string>();
  public previousEnabled: boolean;
  public previousLanguage: string;
  public previousReadonly: boolean;
  public editorSubscription: vscode.Disposable;
  public configSubscription: vscode.Disposable;

  constructor(
    configService: ConfigurationService,
    languageDetector: LanguageDetector,
    contentProvider: TranslatedContentProvider,
    outputChannel: vscode.OutputChannel
  ) {
    this.configService = configService;
    this.languageDetector = languageDetector;
    this.contentProvider = contentProvider;
    this.outputChannel = outputChannel;
    this.previousEnabled = configService.isEnabled();
    this.previousLanguage = configService.getLanguage();
    this.previousReadonly = configService.isReadonly();

    this.editorSubscription = vscode.window.onDidChangeActiveTextEditor(
      (editor: vscode.TextEditor | undefined): void => {
        if (editor) {
          void this.handleActiveEditorChange(editor);
        }
      }
    );

    this.configSubscription = configService.onDidChangeConfiguration((): void => {
      void this.handleConfigChange();
    });
  }

  /** Returns the active translated scheme based on the readonly setting. */
  public getActiveScheme(): string {
    if (this.configService.isReadonly()) {
      return READONLY_SCHEME;
    }
    return TRANSLATED_SCHEME;
  }

  /**
   * When a .cs file tab becomes active and translation is ON, replaces it with the translated view.
   * Guards against event loops via processingUris set and scheme check.
   */
  public async handleActiveEditorChange(editor: vscode.TextEditor): Promise<void> {
    if (!this.configService.isEnabled()) {
      return;
    }

    if (editor.document.uri.scheme !== 'file') {
      return;
    }

    if (!this.languageDetector.isSupported(editor.document.uri.fsPath)) {
      return;
    }

    const uriString: string = editor.document.uri.toString();
    if (this.processingUris.has(uriString)) {
      return;
    }

    const activeScheme: string = this.getActiveScheme();
    const translatedUri: vscode.Uri = vscode.Uri.parse(
      `${activeScheme}:${editor.document.uri.path}`
    );

    const editableUri: vscode.Uri = vscode.Uri.parse(
      `${TRANSLATED_SCHEME}:${editor.document.uri.path}`
    );
    const readonlyUri: vscode.Uri = vscode.Uri.parse(
      `${READONLY_SCHEME}:${editor.document.uri.path}`
    );

    if (this.isTabOpen(editableUri) || this.isTabOpen(readonlyUri)) {
      return;
    }

    this.processingUris.add(uriString);
    try {
      const viewColumn: vscode.ViewColumn = editor.viewColumn ?? vscode.ViewColumn.One;

      const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
      await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
      await this.closeTab(editor.document.uri);

      this.outputChannel.appendLine(
        `AutoTranslate: replaced ${editor.document.uri.fsPath} with translated view`
      );
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`AutoTranslate: failed - ${message}`);
    } finally {
      this.processingUris.delete(uriString);
    }
  }

  /** Reacts to config changes: ON->OFF restores originals, OFF->ON translates, language/readonly change refreshes. */
  public async handleConfigChange(): Promise<void> {
    const currentEnabled: boolean = this.configService.isEnabled();
    const currentLanguage: string = this.configService.getLanguage();
    const currentReadonly: boolean = this.configService.isReadonly();
    const wasEnabled: boolean = this.previousEnabled;
    const previousLanguage: string = this.previousLanguage;
    const wasReadonly: boolean = this.previousReadonly;

    this.previousEnabled = currentEnabled;
    this.previousLanguage = currentLanguage;
    this.previousReadonly = currentReadonly;

    if (wasEnabled && !currentEnabled) {
      await this.replaceTranslatedWithOriginals();
    } else if (!wasEnabled && currentEnabled) {
      await this.replaceOriginalsWithTranslated();
    } else if (currentEnabled && currentLanguage !== previousLanguage) {
      await this.refreshTranslatedTabs();
    } else if (currentEnabled && currentReadonly !== wasReadonly) {
      await this.switchScheme(wasReadonly ? READONLY_SCHEME : TRANSLATED_SCHEME);
    }
  }

  /** Closes all tabs using the old scheme and reopens them with the current active scheme. */
  public async switchScheme(oldScheme: string): Promise<void> {
    const oldTabs: TabInfo[] = this.findTabsByScheme(oldScheme);
    const newScheme: string = this.getActiveScheme();

    for (const { tab, path, viewColumn } of oldTabs) {
      try {
        const newUri: vscode.Uri = vscode.Uri.parse(`${newScheme}:${path}`);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(newUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await vscode.window.tabGroups.close(tab);
      } catch (error: unknown) {
        const message: string = error instanceof Error ? error.message : String(error);
        this.outputChannel.appendLine(`AutoTranslate: failed to switch scheme - ${message}`);
      }
    }

    this.outputChannel.appendLine(`AutoTranslate: switched tabs from ${oldScheme} to ${newScheme}`);
  }

  /** Replaces all open translated tabs with their original .cs file tabs. */
  public async replaceTranslatedWithOriginals(): Promise<void> {
    const translatedTabs: TabInfo[] = [
      ...this.findTabsByScheme(TRANSLATED_SCHEME),
      ...this.findTabsByScheme(READONLY_SCHEME)
    ];

    for (const { tab, path, viewColumn } of translatedTabs) {
      try {
        const originalUri: vscode.Uri = vscode.Uri.file(path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(originalUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await vscode.window.tabGroups.close(tab);
      } catch (error: unknown) {
        const message: string = error instanceof Error ? error.message : String(error);
        this.outputChannel.appendLine(`AutoTranslate: failed to restore original - ${message}`);
      }
    }

    this.outputChannel.appendLine('AutoTranslate: replaced all translated tabs with originals');
  }

  /** Replaces all open .cs file tabs with their translated views. */
  public async replaceOriginalsWithTranslated(): Promise<void> {
    const csTabs: TabInfo[] = this.findSupportedFileTabs();

    const uriStrings: string[] = csTabs.map(
      ({ path }): string => vscode.Uri.file(path).toString()
    );
    for (const uriString of uriStrings) {
      this.processingUris.add(uriString);
    }

    const activeScheme: string = this.getActiveScheme();
    try {
      for (const { tab, path, viewColumn } of csTabs) {
        try {
          const translatedUri: vscode.Uri = vscode.Uri.parse(
            `${activeScheme}:${path}`
          );
          const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
          await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
          await vscode.window.tabGroups.close(tab);
        } catch (error: unknown) {
          const message: string = error instanceof Error ? error.message : String(error);
          this.outputChannel.appendLine(`AutoTranslate: failed to translate tab - ${message}`);
        }
      }
    } finally {
      for (const uriString of uriStrings) {
        this.processingUris.delete(uriString);
      }
    }

    this.outputChannel.appendLine('AutoTranslate: replaced all .cs tabs with translated views');
  }

  /** Invalidates cache and fires change events to refresh all open translated tabs. */
  public async refreshTranslatedTabs(): Promise<void> {
    const translatedTabs: TabInfo[] = [
      ...this.findTabsByScheme(TRANSLATED_SCHEME),
      ...this.findTabsByScheme(READONLY_SCHEME)
    ];

    this.contentProvider.invalidateAll();

    for (const { tab } of translatedTabs) {
      this.contentProvider.invalidateCache(
        (tab.input as vscode.TabInputText).uri
      );
    }

    this.outputChannel.appendLine('AutoTranslate: refreshed all translated tabs for new language');
  }

  /** Finds all open tabs matching a given URI scheme. */
  public findTabsByScheme(scheme: string): TabInfo[] {
    const results: TabInfo[] = [];

    for (const group of vscode.window.tabGroups.all) {
      for (const tab of group.tabs) {
        if (tab.input instanceof vscode.TabInputText) {
          if (tab.input.uri.scheme === scheme) {
            results.push({ tab, path: tab.input.uri.path, viewColumn: group.viewColumn });
          }
        }
      }
    }

    return results;
  }

  /** Finds all open file tabs for supported languages (.cs). */
  public findSupportedFileTabs(): TabInfo[] {
    const results: TabInfo[] = [];

    for (const group of vscode.window.tabGroups.all) {
      for (const tab of group.tabs) {
        if (tab.input instanceof vscode.TabInputText) {
          if (
            tab.input.uri.scheme === 'file' &&
            this.languageDetector.isSupported(tab.input.uri.fsPath)
          ) {
            results.push({ tab, path: tab.input.uri.path, viewColumn: group.viewColumn });
          }
        }
      }
    }

    return results;
  }

  /** Checks whether a tab with the given URI is already open. */
  public isTabOpen(uri: vscode.Uri): boolean {
    const uriString: string = uri.toString();

    for (const group of vscode.window.tabGroups.all) {
      for (const tab of group.tabs) {
        if (tab.input instanceof vscode.TabInputText) {
          if (tab.input.uri.toString() === uriString) {
            return true;
          }
        }
      }
    }

    return false;
  }

  /** Closes the tab matching the given URI. */
  public async closeTab(uri: vscode.Uri): Promise<void> {
    const uriString: string = uri.toString();

    for (const group of vscode.window.tabGroups.all) {
      for (const tab of group.tabs) {
        if (tab.input instanceof vscode.TabInputText) {
          if (tab.input.uri.toString() === uriString) {
            await vscode.window.tabGroups.close(tab);
            return;
          }
        }
      }
    }
  }

  /** Disposes of event subscriptions. */
  public dispose(): void {
    this.editorSubscription.dispose();
    this.configSubscription.dispose();
  }
}

interface TabInfo {
  tab: vscode.Tab;
  path: string;
  viewColumn: vscode.ViewColumn;
}
