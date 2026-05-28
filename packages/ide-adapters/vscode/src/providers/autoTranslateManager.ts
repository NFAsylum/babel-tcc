import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { TranslatedContentProvider, TRANSLATED_SCHEME, READONLY_SCHEME, isTranslatedScheme } from './translatedContentProvider';
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

  /** Returns true if any translated tab (any scheme) is open for the given original path. */
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
    const translatedUri: vscode.Uri = vscode.Uri.parse(
      `${activeScheme}:${editor.document.uri.path}`
    );

    // Keep one translated view per file: if any scheme's view is already open, do nothing.
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
      await this.refreshTranslatedTabs();
      this.previousLanguageFingerprint = this.getLanguageFingerprint();
    } else {
      this.previousLanguageFingerprint = currentFingerprint;
      if (currentEnabled && currentReadonly !== wasReadonly) {
        await this.switchScheme(wasReadonly ? READONLY_SCHEME : TRANSLATED_SCHEME);
      }
    }
  }

  /**
   * Toggles between the editable and readonly schemes. Read-only-ness is bound to the URI scheme
   * (each scheme is a separately registered provider), so this is the one flow that genuinely needs
   * to reopen the tab under a different URI. The old tab is closed by a fresh lookup (not a stored
   * reference, which can go stale after opening the new tab).
   */
  public async switchScheme(oldScheme: string): Promise<void> {
    const oldTabs: TabInfo[] = this.findTabsByScheme(oldScheme);
    const newScheme: string = this.getActiveScheme();
    // Only restore focus if a translated view was active; otherwise we would open a translated view
    // for an unrelated (possibly unsupported) file that just happened to be focused.
    const activeDoc: vscode.TextDocument | undefined = vscode.window.activeTextEditor?.document;
    const activePath: string | undefined =
      activeDoc && isTranslatedScheme(activeDoc.uri.scheme) ? activeDoc.uri.path : undefined;

    for (const { uri, path, viewColumn } of oldTabs) {
      try {
        const newUri: vscode.Uri = vscode.Uri.parse(`${newScheme}:${path}`);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(newUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await this.closeTab(uri);
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
      const focusUri: vscode.Uri = vscode.Uri.parse(`${scheme}:${activePath}`);
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

    for (const { uri, path, viewColumn } of translatedTabs) {
      try {
        const originalUri: vscode.Uri = vscode.Uri.file(path);
        const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(originalUri);
        await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
        await this.closeTab(uri);
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

    const uriStrings: string[] = csTabs.map(({ uri }): string => uri.toString());
    for (const uriString of uriStrings) {
      this.processingUris.add(uriString);
    }

    const activeScheme: string = this.getActiveScheme();
    try {
      for (const { uri, path, viewColumn } of csTabs) {
        try {
          const translatedUri: vscode.Uri = vscode.Uri.parse(`${activeScheme}:${path}`);
          const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(translatedUri);
          await vscode.window.showTextDocument(doc, { preview: false, viewColumn });
          await this.closeTab(uri);
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

  /**
   * Confirms what to do with unsaved edits in translated views BEFORE a language change. It must run
   * while the current language is still in effect, so that saving reverse-translates the content with
   * the language it is actually written in (a normal save) — saving after the config already changed
   * is what previously discarded the edits. Returns false if the user cancels (caller must not switch).
   */
  public async confirmUnsavedEditsBeforeLanguageChange(): Promise<boolean> {
    const dirtyDocs: vscode.TextDocument[] = vscode.workspace.textDocuments.filter(
      (doc: vscode.TextDocument): boolean =>
        isTranslatedScheme(doc.uri.scheme) && doc.isDirty
    );
    if (dirtyDocs.length === 0) {
      return true;
    }

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
      this.outputChannel.appendLine('AutoTranslate: language switch cancelled by user');
      return false;
    }

    for (const doc of dirtyDocs) {
      try {
        if (choice === saveLabel) {
          await doc.save();
        } else {
          // Discard: revert the buffer to its saved content (in the current language) so the view is
          // clean and the subsequent language change reloads it in the new language.
          await vscode.window.showTextDocument(doc, { preview: false });
          await vscode.commands.executeCommand('workbench.action.files.revert');
        }
      } catch (error: unknown) {
        const message: string = error instanceof Error ? error.message : String(error);
        this.outputChannel.appendLine(`AutoTranslate: failed to handle unsaved edits in ${doc.uri.path} - ${message}`);
      }
    }
    return true;
  }

  /**
   * Refreshes all open translated tabs for a new language, in place. The target language is read
   * from configuration, so the URI does not change: we notify the provider that each open file
   * changed and VS Code reloads the content in the SAME editor (the provider's stat reports the new
   * translated size plus an advanced mtime). No tab is closed or reopened, so focus never moves and no
   * stale tab is left behind. Unsaved edits are handled before the change, in
   * confirmUnsavedEditsBeforeLanguageChange (a dirty view simply is not reloaded by the change event).
   */
  public async refreshTranslatedTabs(): Promise<void> {
    const translatedTabs: TabInfo[] = [
      ...this.findTabsByScheme(TRANSLATED_SCHEME),
      ...this.findTabsByScheme(READONLY_SCHEME)
    ];

    const seen: Set<string> = new Set<string>();
    for (const { path } of translatedTabs) {
      if (seen.has(path)) {
        continue;
      }
      seen.add(path);
      this.contentProvider.invalidatePath(path);
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
            results.push({ uri: tab.input.uri, path: tab.input.uri.path, viewColumn: group.viewColumn });
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
            results.push({ uri: tab.input.uri, path: tab.input.uri.path, viewColumn: group.viewColumn });
          }
        }
      }
    }

    return results;
  }

  /** Closes the tab matching the given URI, found by a fresh scan of the current tab model. */
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
  uri: vscode.Uri;
  path: string;
  viewColumn: vscode.ViewColumn;
}
