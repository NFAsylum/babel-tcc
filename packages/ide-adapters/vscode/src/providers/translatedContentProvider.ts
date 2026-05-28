import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';

/** The URI scheme used for editable translated document views. */
export const TRANSLATED_SCHEME = 'babel-tcc-translated';

/** The URI scheme used for readonly translated document views. */
export const READONLY_SCHEME = 'babel-tcc-readonly';

/** Returns true if the given scheme is any translated scheme (editable or readonly). */
export function isTranslatedScheme(scheme: string): boolean {
  return scheme === TRANSLATED_SCHEME || scheme === READONLY_SCHEME;
}

/**
 * How long (ms) after the extension writes an original file to treat file-watcher events for that
 * path as our own write and suppress them. A single save can emit several coalesced watcher events,
 * and a save can also emit none (a no-op or failed write); a time window covers the whole burst and
 * self-expires, so the marker neither leaks (suppressing a later genuine external change forever) nor
 * under-suppresses, unlike a consume-once flag.
 */
const SELF_WRITE_SUPPRESS_MS = 1000;

/** Provides a virtual filesystem for translated documents, supporting read and write operations. */
export class TranslatedContentProvider implements vscode.FileSystemProvider {
  public coreBridge: CoreBridge;
  public languageDetector: LanguageDetector;
  public configService: ConfigurationService;
  public outputChannel: vscode.OutputChannel;
  public cache: Map<string, string> = new Map<string, string>();
  public onTranslationComplete: ((language: string) => void) | null = null;
  /** Timestamp (ms) of the last time WE wrote each original path, used to suppress our own
   * file-watcher events for SELF_WRITE_SUPPRESS_MS. Keyed by original file path. */
  public recentWrites: Map<string, number> = new Map<string, number>();
  public saveQueue: Map<string, Promise<void>> = new Map<string, Promise<void>>();
  public mtimeMap: Map<string, number> = new Map<string, number>();
  /** The natural language each open translated view is currently displaying, keyed by file path. */
  public displayLanguages: Map<string, string> = new Map<string, string>();
  /**
   * The exact translated text each open view was last rendered with — set in readFile and after a
   * save — keyed by file path. This is the baseline for the reverse-translation 3-way merge. It is
   * deliberately SEPARATE from `cache`: a cache invalidation (language switch, file-watcher) must
   * never blank out the baseline, or the next save would merge against nothing and write the
   * translated tokens straight into the original (corruption).
   */
  public renderedContent: Map<string, string> = new Map<string, string>();
  public changeEmitter: vscode.EventEmitter<vscode.FileChangeEvent[]> =
    new vscode.EventEmitter<vscode.FileChangeEvent[]>();
  public onDidChangeFile: vscode.Event<vscode.FileChangeEvent[]> = this.changeEmitter.event;

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
  }

  public watch(): vscode.Disposable {
    return new vscode.Disposable((): void => {});
  }

  public async stat(uri: vscode.Uri): Promise<vscode.FileStat> {
    const originalUri: vscode.Uri = vscode.Uri.file(uri.path);
    const originalStat: vscode.FileStat = await vscode.workspace.fs.stat(originalUri);
    // The size MUST reflect the translated content that readFile returns (not the original file's
    // size), and the mtime MUST advance on a change. VS Code skips refreshing an open editor on a
    // change event unless both hold (FileSystemProvider.onDidChangeFile contract). This is what makes
    // an in-place language switch — invalidatePath fires the change event — actually re-translate.
    const content: string = await this.provideContent(uri);
    const virtualMtime: number = this.mtimeMap.get(uri.toString()) || originalStat.mtime;
    return {
      type: originalStat.type,
      ctime: originalStat.ctime,
      mtime: virtualMtime,
      size: Buffer.byteLength(content, 'utf-8'),
    };
  }

  public async readFile(uri: vscode.Uri): Promise<Uint8Array> {
    const content: string = await this.provideContent(uri);
    // Record the displayed language AND the rendered baseline ONLY here, not in provideContent:
    // readFile is when VS Code loads content into the editor buffer, so both reflect what the buffer
    // actually holds. provideContent is also called by stat() (frequently, including around save), and
    // recording there would let a stat after a language switch overwrite this before the buffer
    // actually reloads. Keeping the baseline keyed here (not via cache) is what makes a save always
    // reverse-translate from the correct content/language.
    this.displayLanguages.set(uri.path, this.getTargetLanguage(uri));
    this.renderedContent.set(uri.path, content);
    const encoder = new TextEncoder();
    return encoder.encode(content);
  }

  public async writeFile(uri: vscode.Uri, content: Uint8Array): Promise<void> {
    const originalPath: string = uri.path;

    // Queue saves per path: if a save is already in progress, the next one
    // waits for it to complete before starting. This prevents the second save
    // from reading the partially-written file as "original".
    const previousSave: Promise<void> = this.saveQueue.get(originalPath) || Promise.resolve();
    const currentSave: Promise<void> = previousSave.then((): Promise<void> => this.doWriteFile(uri, content));
    this.saveQueue.set(originalPath, currentSave);
    await currentSave;
  }

  public async doWriteFile(uri: vscode.Uri, content: Uint8Array): Promise<void> {
    const originalPath: string = uri.path;
    const translatedContent: string = Buffer.from(content).toString('utf-8');
    const fileExtension: string = this.languageDetector.getFileExtension(originalPath);
    // Reverse-translate FROM the language the open view is actually displaying. This can differ from
    // the current config when the language was just switched while this view had unsaved edits: a
    // dirty document is not reloaded, so it keeps its old language until saved. Using the config here
    // would reverse-translate old-language content as if it were the new language and corrupt the
    // original on disk.
    const sourceLanguage: string = this.displayLanguageFor(uri);

    this.outputChannel.appendLine(`TranslatedContentProvider: reverse translating ${originalPath}`);

    try {
      const currentOriginal: string = await this.readOriginalFile(originalPath);
      // Baseline = the exact content readFile last rendered into this buffer (or what the previous
      // save left it as), taken from renderedContent — never from the cache, which a language switch
      // or the file-watcher may have cleared. An empty/mismatched baseline makes the 3-way merge
      // dump the translated text into the original.
      const previousTranslated: string = this.renderedContent.get(originalPath) ?? '';

      const originalCode: string = await this.coreBridge.applyTranslatedEdits(
        currentOriginal, previousTranslated, translatedContent, fileExtension, sourceLanguage
      );

      const originalUri: vscode.Uri = vscode.Uri.file(originalPath);
      this.markSelfWrite(originalPath);
      const originalDoc: vscode.TextDocument = await vscode.workspace.openTextDocument(originalUri);
      const fullRange: vscode.Range = new vscode.Range(
        new vscode.Position(0, 0),
        originalDoc.lineAt(originalDoc.lineCount - 1).range.end
      );
      const writeEdit: vscode.WorkspaceEdit = new vscode.WorkspaceEdit();
      writeEdit.replace(originalUri, fullRange, originalCode);
      await vscode.workspace.applyEdit(writeEdit);
      await originalDoc.save();

      // The buffer now holds `translatedContent` in `sourceLanguage`. Record it as the new baseline
      // and as the cache entry for that language; the buffer is left exactly as the user typed it (no
      // re-render). Other languages' cached translations are stale now that the original changed, so
      // drop them — a later switch re-translates from the updated source. displayLanguages is NOT
      // touched: a save does not change the language the buffer is in.
      for (const key of [...this.cache.keys()]) {
        if (key.startsWith(`${originalPath}::`)) {
          this.cache.delete(key);
        }
      }
      this.cache.set(`${originalPath}::${sourceLanguage}`, translatedContent);
      this.renderedContent.set(originalPath, translatedContent);

      this.outputChannel.appendLine(`TranslatedContentProvider: saved original code to ${originalPath}`);
      vscode.window.showInformationMessage(vscode.l10n.t('Babel TCC: File saved successfully.'));
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`TranslatedContentProvider: reverse translation failed - ${message}`);
      vscode.window.showErrorMessage(
        vscode.l10n.t('Babel TCC: Failed to reverse translate. Original file was NOT overwritten.')
      );
    }
  }

  public readDirectory(): [string, vscode.FileType][] {
    return [];
  }

  public createDirectory(): void {}

  public delete(): void {}

  public rename(): void {}

  /**
   * Resolves the content for a translated virtual document.
   * Returns cached content if available, otherwise reads the original file, translates it, and caches the result.
   */
  public async provideContent(uri: vscode.Uri): Promise<string> {
    const originalPath: string = uri.path;
    const cacheKey: string = this.buildCacheKey(uri);

    const cachedContent: string | undefined = this.cache.get(cacheKey);
    if (cachedContent !== undefined) {
      return cachedContent;
    }

    if (!this.languageDetector.isSupported(originalPath)) {
      this.outputChannel.appendLine(`TranslatedContentProvider: unsupported file ${originalPath}`);
      return await this.readOriginalFile(originalPath);
    }

    const originalContent: string = await this.readOriginalFile(originalPath);

    if (!this.configService.isEnabled()) {
      return originalContent;
    }

    const fileExtension: string = this.languageDetector.getFileExtension(originalPath);
    const targetLanguage: string = this.getTargetLanguage(uri);

    const translated: string = await this.translateContent(originalContent, fileExtension, targetLanguage);
    this.cache.set(cacheKey, translated);
    if (this.onTranslationComplete) {
      this.onTranslationComplete(targetLanguage);
    }
    return translated;
  }

  /**
   * Translates source code into the specified natural language using the Core engine.
   * Falls back to returning the original source code if translation fails.
   */
  public async translateContent(
    sourceCode: string,
    fileExtension: string,
    targetLanguage: string
  ): Promise<string> {
    try {
      const result: string = await this.coreBridge.translateToNaturalLanguage(
        sourceCode, fileExtension, targetLanguage
      );
      return result;
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`TranslatedContentProvider: translation failed - ${message}`);
      vscode.window.showWarningMessage(
        vscode.l10n.t('Babel TCC: Translation failed. Showing original code. Check Output panel for details.')
      );
      return sourceCode;
    }
  }

  /**
   * Invalidates every cached translation for an original file path (all languages and schemes) and
   * fires change events for any open translated view of that path. Used when the original file
   * changes on disk so the visible translation refreshes regardless of which language it is in.
   */
  public invalidatePath(originalPath: string): void {
    for (const key of [...this.cache.keys()]) {
      if (key.startsWith(`${originalPath}::`)) {
        this.cache.delete(key);
      }
    }

    const now: number = Date.now();
    const events: vscode.FileChangeEvent[] = [];
    for (const doc of vscode.workspace.textDocuments) {
      if (isTranslatedScheme(doc.uri.scheme) && doc.uri.path === originalPath) {
        this.mtimeMap.set(doc.uri.toString(), now);
        events.push({ type: vscode.FileChangeType.Changed, uri: doc.uri });
      }
    }
    if (events.length > 0) {
      this.changeEmitter.fire(events);
    }
  }

  /** Clears the entire translation cache, forcing all documents to be re-translated on next access. */
  public invalidateAll(): void {
    this.cache.clear();
    this.mtimeMap.clear();
  }

  /**
   * Records that WE just wrote the given original path, so the file-watcher can ignore the resulting
   * change event(s) for a short window (SELF_WRITE_SUPPRESS_MS). Called from doWriteFile.
   */
  public markSelfWrite(originalPath: string): void {
    this.recentWrites.set(originalPath, Date.now());
  }

  /** Returns true if the given original path was written by us within the suppression window. */
  public isRecentSelfWrite(originalPath: string): boolean {
    const writtenAt: number | undefined = this.recentWrites.get(originalPath);
    return writtenAt !== undefined && Date.now() - writtenAt < SELF_WRITE_SUPPRESS_MS;
  }

  /** Resolves the configured target natural language for a file's programming language. */
  public getTargetLanguage(uri: vscode.Uri): string {
    const programmingLanguage: string = this.languageDetector.detectLanguage(uri.path) || '';
    return this.configService.getLanguageForProgrammingLanguage(programmingLanguage);
  }

  /**
   * The language the open view is currently displaying for this file, falling back to the configured
   * language when no view has been rendered yet. Used by reverse translation so a save uses the
   * language the content is actually in, not the (possibly just-changed) configured one.
   */
  public displayLanguageFor(uri: vscode.Uri): string {
    return this.displayLanguages.get(uri.path) ?? this.getTargetLanguage(uri);
  }

  /**
   * Builds a cache key combining the file path and the current target language for that file.
   */
  public buildCacheKey(uri: vscode.Uri): string {
    return `${uri.path}::${this.getTargetLanguage(uri)}`;
  }

  /**
   * Reads the original source file from disk, respecting the workspace encoding setting.
   */
  public async readOriginalFile(filePath: string): Promise<string> {
    const uri: vscode.Uri = vscode.Uri.file(filePath);
    const doc: vscode.TextDocument = await vscode.workspace.openTextDocument(uri);
    return doc.getText();
  }

  /** Disposes of resources by clearing the cache and releasing the change event emitter. */
  public dispose(): void {
    this.cache.clear();
    this.displayLanguages.clear();
    this.renderedContent.clear();
    this.recentWrites.clear();
    this.changeEmitter.dispose();
  }
}
