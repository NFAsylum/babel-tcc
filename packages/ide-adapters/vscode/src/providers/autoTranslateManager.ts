import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme, buildTranslatedUri } from './translatedContentProvider';
import { SUPPORTED_LANGUAGES } from '../config/languages';

/** Manages automatic translation of .cs tabs based on the enabled/language configuration. */
export class AutoTranslateManager implements vscode.Disposable {
  public configService: ConfigurationService;
  public languageDetector: LanguageDetector;
  public contentProvider: TranslatedContentProvider;
  public outputChannel: vscode.OutputChannel;
  public processingUris: Set<string> = new Set<string>();
  public previousEnabled: boolean;
  public previousLanguageFingerprint: string;
  public previousReadonly: boolean;
  public editorSubscription: vscode.Disposable;
  public configSubscription: vscode.Disposable;
  public configChangeQueue: Promise<void> = Promise.resolve();

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
    this.previousLanguageFingerprint = this.getLanguageFingerprint();
    this.previousReadonly = configService.isReadonly();

    this.editorSubscription = vscode.window.onDidChangeActiveTextEditor(
      (editor: vscode.TextEditor | undefined): void => {
        if (editor) {
          void this.handleActiveEditorChange(editor);
        }
      }
    );

    this.configSubscription = configService.onDidChangeConfiguration((): void => {
      this.configChangeQueue = this.configChangeQueue
        .then((): Promise<void> => this.handleConfigChange())
        .catch((err: unknown): void => {
          const message: string = err instanceof Error ? err.message : String(err);
          this.outputChannel.appendLine(`AutoTranslate: config change failed - ${message}`);
        });
    });
  }

  /** Returns the active translated scheme based on the readonly setting. */
  public getActiveScheme(): string {
    if (this.configService.isReadonly()) {
      return READONLY_SCHEME;
    }
    return TRANSLATED_SCHEME;
  }

  /** Builds the translated URI for a path under a scheme, encoding the file's current target language. */
  public buildTranslatedUriForPath(scheme: string, path: string): vscode.Uri {
    const programmingLanguage: string = this.languageDetector.detectLanguage(path) || '';
    const language: string = this.configService.getLanguageForProgrammingLanguage(programmingLanguage);
    return buildTranslatedUri(scheme, path, language);
  }

  /** Returns true if any translated tab (any scheme/language) is open for the given original path. */
  public isAnyTranslatedTabOpenForPath(path: string): boolean {
    for (const group of vscode.window.tabGroups.all) {
      for (const tab of group.tabs) {
        if (tab.input instanceof vscode.TabInputText) {
          if (isTranslatedScheme(tab.input.uri.scheme) && tab.input.uri.path === path) {
            return true;
          }
        }
      }
    }
    return false;
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
    const translatedUri: vscode.Uri = this.buildTranslatedUriForPath(
      activeScheme, editor.document.uri.path
    );

    // Keep one translated view per file: if any language/scheme view is already open, do nothing.
    if (this.isAnyTranslatedTabOpenForPath(editor.document.uri.path)) {
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

  /**
   * Builds a fingerprint of all effective languages (global + per-language overrides).
   * Used to detect changes in any language setting, not just the global one.
   */
  public getLanguageFingerprint(): string {
    const parts: string[] = [this.configService.getLanguage()];
    for (const lang of SUPPORTED_LANGUAGES) {
      parts.push(`${lang.name}:${this.configService.getLanguageForProgrammingLanguage(lang.name)}`);
    }
    return parts.join('|');
  }

  /** Reacts to config changes: ON->OFF restores originals, OFF->ON translates, language/readonly change refreshes. */
  public async handleConfigChange(): Promise<void> {
    const currentEnabled: boolean = this.configService.isEnabled();
    const currentFingerprint: string = this.getLanguageFingerprint();
    const currentReadonly: boolean = this.configService.isReadonly();
    const wasEnabled: boolean = this.previousEnabled;
    const previousFingerprint: string = this.previousLanguageFingerprint;
    const wasReadonly: boolean = this.previousReadonly;

    this.previousEnabled = currentEnabled;
    this.previousReadonly = currentReadonly;

    if (wasEnabled && !currentEnabled) {
      this.previousLanguageFingerprint = currentFingerprint;
      await this.replaceTranslatedWithOriginals();
    } else if (!wasEnabled && currentEnabled) {
      this.previousLanguageFingerprint = currentFingerprint;
      await this.replaceOriginalsWithTranslated();
    } else if (currentEnabled && currentFingerprint !== previousFingerprint) {
      const previousGlobalLanguage: string = previousFingerprint.split('|')[0];
      await this.refreshTranslatedTabs(previousGlobalLanguage);
      this.previousLanguageFingerprint = this.getLanguageFingerprint();
    } else {
      this.previousLanguageFingerprint = currentFingerprint;
      if (currentEnabled && currentReadonly !== wasReadonly) {
        await this.switchScheme(wasReadonly ? READONLY_SCHEME : TRANSLATED_SCHEME);
      }
    }
  }

  /** Closes all tabs using the old scheme and reopens them with the current active scheme. */
  public async switchScheme(oldScheme: string): Promise<void> {
    const oldTabs: TabInfo[] = this.findTabsByScheme(oldScheme);
    const newScheme: string = this.getActiveScheme();
    const activePath: string | undefined = vscode.window.activeTextEditor?.document.uri.path;

    for (const { tab, path, viewColumn } of oldTabs) {
      try {
        const newUri: vscode.Uri = this.buildTranslatedUriForPath(newScheme, path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(newUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await vscode.window.tabGroups.close(tab);
      } catch (error: unknown) {
        const message: string = error instanceof Error ? error.message : String(error);
        this.outputChannel.appendLine(`AutoTranslate: failed to switch scheme - ${message}`);
      }
    }

    await this.restoreFocus(newScheme, activePath);
    this.outputChannel.appendLine(`AutoTranslate: switched tabs from ${oldScheme} to ${newScheme}`);
  }

  /** Re-focuses the translated view of the previously active file after a tab swap, if any. */
  public async restoreFocus(scheme: string, activePath: string | undefined): Promise<void> {
    if (activePath === undefined) {
      return;
    }
    try {
      const focusUri: vscode.Uri = this.buildTranslatedUriForPath(scheme, activePath);
      const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(focusUri);
      await vscode.window.showTextDocument(doc, { preview: false });
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`AutoTranslate: failed to restore focus - ${message}`);
    }
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
          const translatedUri: vscode.Uri = this.buildTranslatedUriForPath(activeScheme, path);
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

  /** Refreshes all translated tabs for a new language, handling unsaved edits. */
  public async refreshTranslatedTabs(oldLanguage: string): Promise<void> {
    const translatedTabs: TabInfo[] = [
      ...this.findTabsByScheme(TRANSLATED_SCHEME),
      ...this.findTabsByScheme(READONLY_SCHEME)
    ];

    // Check for unsaved edits in translated documents
    const dirtyDocs: vscode.TextDocument[] = vscode.workspace.textDocuments.filter(
      (doc: vscode.TextDocument): boolean =>
        isTranslatedScheme(doc.uri.scheme) && doc.isDirty
    );

    if (dirtyDocs.length > 0) {
      const saveLabel: string = vscode.l10n.t('Save and switch');
      const discardLabel: string = vscode.l10n.t('Discard and switch');
      const cancelLabel: string = vscode.l10n.t('Cancel');
      const choice: string | undefined = await vscode.window.showWarningMessage(
        vscode.l10n.t('Babel TCC: {0} translated file(s) have unsaved changes. What would you like to do before switching languages?', dirtyDocs.length),
        saveLabel,
        discardLabel,
        cancelLabel
      );

      if (choice === cancelLabel || choice === undefined) {
        // Revert language config to previous
        await this.configService.setLanguage(oldLanguage);
        this.outputChannel.appendLine('AutoTranslate: language switch cancelled by user');
        return;
      }

      if (choice === saveLabel) {
        for (const doc of dirtyDocs) {
          try {
            await doc.save();
          } catch (error: unknown) {
            const message: string = error instanceof Error ? error.message : String(error);
            this.outputChannel.appendLine(`AutoTranslate: failed to save ${doc.uri.path} - ${message}`);
          }
        }
      }
      // 'Discard and switch' — just proceed without saving
    }

    // Each language is a distinct URI (the lang is encoded in the query), so the new-language view
    // is a document VS Code has never loaded: opening it forces a fresh read from the provider, with
    // no reliance on change events or a stale cached working copy. Open the new-language view first,
    // then close the old-language tab, then restore focus to the file that was active.
    const activePath: string | undefined = vscode.window.activeTextEditor?.document.uri.path;
    const scheme: string = this.getActiveScheme();

    for (const { tab, path, viewColumn } of translatedTabs) {
      try {
        const newUri: vscode.Uri = this.buildTranslatedUriForPath(scheme, path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(newUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await vscode.window.tabGroups.close(tab);
      } catch (error: unknown) {
        const message: string = error instanceof Error ? error.message : String(error);
        this.outputChannel.appendLine(`AutoTranslate: failed to refresh tab - ${message}`);
      }
    }

    await this.restoreFocus(scheme, activePath);
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
