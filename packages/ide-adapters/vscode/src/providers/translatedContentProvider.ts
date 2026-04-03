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

/** Provides a virtual filesystem for translated documents, supporting read and write operations. */
export class TranslatedContentProvider implements vscode.FileSystemProvider {
  public coreBridge: CoreBridge;
  public languageDetector: LanguageDetector;
  public configService: ConfigurationService;
  public outputChannel: vscode.OutputChannel;
  public cache: Map<string, string> = new Map<string, string>();
  public writingPaths: Set<string> = new Set<string>();
  public refreshingPaths: Set<string> = new Set<string>();
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
    return vscode.workspace.fs.stat(originalUri);
  }

  public async readFile(uri: vscode.Uri): Promise<Uint8Array> {
    const content: string = await this.provideContent(uri);
    const encoder: TextEncoder = new TextEncoder();
    return encoder.encode(content);
  }

  public async writeFile(uri: vscode.Uri, content: Uint8Array): Promise<void> {
    const originalPath: string = uri.path;

    if (this.refreshingPaths.has(originalPath)) {
      return;
    }

    const translatedContent: string = Buffer.from(content).toString('utf-8');
    const fileExtension: string = this.languageDetector.getFileExtension(originalPath);
    const sourceLanguage: string = this.configService.getLanguage();

    this.outputChannel.appendLine(`TranslatedContentProvider: reverse translating ${originalPath}`);

    try {
      const originalCode: string = await this.coreBridge.translateFromNaturalLanguage(
        translatedContent, fileExtension, sourceLanguage
      );

      const originalUri: vscode.Uri = vscode.Uri.file(originalPath);
      const encoder: TextEncoder = new TextEncoder();
      this.writingPaths.add(originalPath);
      await vscode.workspace.fs.writeFile(originalUri, encoder.encode(originalCode));
      setTimeout((): void => { this.writingPaths.delete(originalPath); }, 500);

      const targetLanguage: string = this.configService.getLanguage();
      const updatedOriginal: string = await this.readOriginalFile(originalPath);
      const freshTranslation: string = await this.translateContent(
        updatedOriginal, fileExtension, targetLanguage
      );

      const cacheKey: string = this.buildCacheKey(originalPath);
      this.cache.set(cacheKey, freshTranslation);

      setTimeout(async (): Promise<void> => {
        try {
          const doc: vscode.TextDocument | undefined = vscode.workspace.textDocuments.find(
            (d: vscode.TextDocument): boolean => d.uri.toString() === uri.toString()
          );
          if (doc && doc.getText() !== freshTranslation) {
            const edit: vscode.WorkspaceEdit = new vscode.WorkspaceEdit();
            const lastLine: vscode.TextLine = doc.lineAt(doc.lineCount - 1);
            edit.replace(
              uri,
              new vscode.Range(new vscode.Position(0, 0), lastLine.range.end),
              freshTranslation
            );
            this.refreshingPaths.add(originalPath);
            try {
              await vscode.workspace.applyEdit(edit);
            } finally {
              this.refreshingPaths.delete(originalPath);
            }
          }
        } catch (err: unknown) {
          this.outputChannel.appendLine('TranslatedContentProvider: failed to refresh translated view');
        }
      }, 100);

      this.outputChannel.appendLine(`TranslatedContentProvider: saved original code to ${originalPath}`);
      vscode.window.showInformationMessage('Babel TCC: File saved successfully.');
    } catch (error: unknown) {
      const message: string = error instanceof Error ? error.message : String(error);
      this.outputChannel.appendLine(`TranslatedContentProvider: reverse translation failed - ${message}`);
      vscode.window.showErrorMessage(
        'Babel TCC: Failed to reverse translate. Original file was NOT overwritten.'
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
    const cacheKey: string = this.buildCacheKey(originalPath);

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
    const targetLanguage: string = this.configService.getLanguage();

    const translated: string = await this.translateContent(originalContent, fileExtension, targetLanguage);
    this.cache.set(cacheKey, translated);
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
        'Babel TCC: Translation failed. Showing original code. Check Output panel for details.'
      );
      return sourceCode;
    }
  }

  /**
   * Removes the cached translation for a specific URI and fires a change event to refresh the document.
   */
  public invalidateCache(uri: vscode.Uri): void {
    const cacheKey: string = this.buildCacheKey(uri.path);
    this.cache.delete(cacheKey);

    const otherScheme: string = uri.scheme === TRANSLATED_SCHEME ? READONLY_SCHEME : TRANSLATED_SCHEME;
    const otherUri: vscode.Uri = vscode.Uri.parse(`${otherScheme}:${uri.path}`);

    this.changeEmitter.fire([
      { type: vscode.FileChangeType.Changed, uri: uri },
      { type: vscode.FileChangeType.Changed, uri: otherUri }
    ]);
  }

  /** Clears the entire translation cache, forcing all documents to be re-translated on next access. */
  public invalidateAll(): void {
    this.cache.clear();
  }

  /**
   * Builds a cache key combining the file path and the current target language.
   */
  public buildCacheKey(filePath: string): string {
    const language: string = this.configService.getLanguage();
    return `${filePath}::${language}`;
  }

  /**
   * Reads the original source file from disk as a UTF-8 string.
   */
  public async readOriginalFile(filePath: string): Promise<string> {
    const uri: vscode.Uri = vscode.Uri.file(filePath);
    const content: Uint8Array = await vscode.workspace.fs.readFile(uri);
    return Buffer.from(content).toString('utf-8');
  }

  /** Disposes of resources by clearing the cache and releasing the change event emitter. */
  public dispose(): void {
    this.cache.clear();
    this.changeEmitter.dispose();
  }
}
