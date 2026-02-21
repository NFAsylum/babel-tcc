import * as vscode from 'vscode';
import { CoreBridge } from '../services/coreBridge';
import { LanguageDetector } from '../services/languageDetector';
import { ConfigurationService } from '../services/configurationService';

export const TRANSLATED_SCHEME = 'babel-tcc-translated';

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

  public invalidateCache(uri: vscode.Uri): void {
    const cacheKey: string = this.buildCacheKey(uri.path);
    this.cache.delete(cacheKey);
    this.changeEmitter.fire(uri);
  }

  public invalidateAll(): void {
    this.cache.clear();
  }

  public buildCacheKey(filePath: string): string {
    const language: string = this.configService.getLanguage();
    return `${filePath}::${language}`;
  }

  public async readOriginalFile(filePath: string): Promise<string> {
    const uri: vscode.Uri = vscode.Uri.file(filePath);
    const content: Uint8Array = await vscode.workspace.fs.readFile(uri);
    return Buffer.from(content).toString('utf-8');
  }

  public dispose(): void {
    this.cache.clear();
    this.changeEmitter.dispose();
  }
}
