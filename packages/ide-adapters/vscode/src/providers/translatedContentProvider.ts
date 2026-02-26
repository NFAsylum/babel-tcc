import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';

/** The URI scheme used for translated document views. */
export const TRANSLATED_SCHEME = 'babel-tcc-translated';

/** Provides virtual document content by translating source files via the Core engine. */
export class TranslatedContentProvider implements vscode.TextDocumentContentProvider {
  public coreBridge: CoreBridge;
  public languageDetector: LanguageDetector;
  public configService: ConfigurationService;
  public outputChannel: vscode.OutputChannel;
  public cache: Map<string, string> = new Map<string, string>();
  public changeEmitter: vscode.EventEmitter<vscode.Uri> = new vscode.EventEmitter<vscode.Uri>();
  public onDidChange: vscode.Event<vscode.Uri> = this.changeEmitter.event;

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

  /**
   * Resolves the content for a translated virtual document.
   * Returns cached content if available, otherwise reads the original file, translates it, and caches the result.
   * @param uri - The URI of the translated virtual document.
   * @returns The translated file content, or the original content if translation is disabled or unsupported.
   */
  public async provideTextDocumentContent(uri: vscode.Uri): Promise<string> {
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
   * @param sourceCode - The original source code to translate.
   * @param fileExtension - The file extension (e.g. '.cs') to determine the programming language.
   * @param targetLanguage - The target natural language code (e.g. 'pt-BR').
   * @returns The translated source code, or the original on failure.
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
      return sourceCode;
    }
  }

  /**
   * Removes the cached translation for a specific URI and fires a change event to refresh the document.
   * @param uri - The URI of the translated document to invalidate.
   */
  public invalidateCache(uri: vscode.Uri): void {
    const cacheKey: string = this.buildCacheKey(uri.path);
    this.cache.delete(cacheKey);
    this.changeEmitter.fire(uri);
  }

  /** Clears the entire translation cache, forcing all documents to be re-translated on next access. */
  public invalidateAll(): void {
    this.cache.clear();
  }

  /**
   * Builds a cache key combining the file path and the current target language.
   * @param filePath - The path of the original source file.
   * @returns A composite key in the format `filePath::language`.
   */
  public buildCacheKey(filePath: string): string {
    const language: string = this.configService.getLanguage();
    return `${filePath}::${language}`;
  }

  /**
   * Reads the original source file from disk as a UTF-8 string.
   * @param filePath - The absolute path of the file to read.
   * @returns The file content as a string.
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
