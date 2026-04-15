import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { CoreBridge } from '../services/coreBridge';

/**
 * Loads keyword translation maps from the Core via CoreBridge.
 * Maps are cached per (language, programmingLanguage) pair and invalidated on config change.
 * The getMap() method is synchronous — cache is pre-populated via warmCache().
 */
export class KeywordMapService {
  public coreBridge: CoreBridge;
  public configService: ConfigurationService;
  public languageDetector: LanguageDetector;
  public outputChannel: vscode.OutputChannel;
  public cache: Map<string, Record<string, string>> = new Map<string, Record<string, string>>();
  public categoryCache: Map<string, Record<string, string>> = new Map<string, Record<string, string>>();
  public identifierCache: Map<string, Record<string, string>> = new Map<string, Record<string, string>>();
  public configSubscription: { dispose(): void };

  constructor(
    coreBridge: CoreBridge,
    configService: ConfigurationService,
    languageDetector: LanguageDetector,
    outputChannel: vscode.OutputChannel
  ) {
    this.coreBridge = coreBridge;
    this.configService = configService;
    this.languageDetector = languageDetector;
    this.outputChannel = outputChannel;
    this.configSubscription = configService.onDidChangeConfiguration((): void => {
      this.cache.clear();
      this.categoryCache.clear();
      this.identifierCache.clear();
      this.warmCache();
    });
  }

  /**
   * Returns a map of translated keywords to their original equivalents.
   * Synchronous — returns from cache. Call warmCache() first to populate.
   * @param filePath - The file path used to detect the programming language.
   */
  public getMap(filePath: string): Record<string, string> {
    const programmingLanguage: string | undefined = this.languageDetector.detectLanguage(filePath);
    if (!programmingLanguage) {
      return {};
    }

    const language: string = this.configService.getLanguageForProgrammingLanguage(programmingLanguage);
    const cacheKey: string = `${language}::${programmingLanguage}`;

    const cached: Record<string, string> | undefined = this.cache.get(cacheKey);
    if (cached) {
      return cached;
    }

    return {};
  }

  /**
   * Returns a map of original keywords to their semantic categories.
   * Synchronous — returns from cache. Call warmCache() first to populate.
   * @param filePath - The file path used to detect the programming language.
   */
  public getCategories(filePath: string): Record<string, string> {
    const programmingLanguage: string | undefined = this.languageDetector.detectLanguage(filePath);
    if (!programmingLanguage) {
      return {};
    }

    const cacheKey: string = `categories::${programmingLanguage}`;
    const cached: Record<string, string> | undefined = this.categoryCache.get(cacheKey);
    if (cached) {
      return cached;
    }

    return {};
  }

  /**
   * Returns a map of translated identifiers to their original equivalents.
   * Synchronous — returns from cache. Call warmCache() first to populate.
   * @param filePath - Unused, kept for API consistency. Identifier maps are language-only.
   */
  public getIdentifierMap(_filePath: string): Record<string, string> {
    const language: string = this.configService.getLanguage();
    const cacheKey: string = `identifiers::${language}`;

    const cached: Record<string, string> | undefined = this.identifierCache.get(cacheKey);
    if (cached) {
      return cached;
    }

    return {};
  }

  /**
   * Pre-populates the cache by fetching keyword maps and identifier maps from the Core.
   * Called during extension activation and after language changes.
   */
  public async warmCache(): Promise<void> {
    const extensions: string[] = this.languageDetector.getSupportedExtensions();
    const language: string = this.configService.getLanguage();

    for (const ext of extensions) {
      const programmingLanguage: string | undefined = this.languageDetector.detectLanguage(`file${ext}`);
      if (!programmingLanguage) {
        continue;
      }

      const cacheKey: string = `${language}::${programmingLanguage}`;
      if (this.cache.has(cacheKey)) {
        continue;
      }

      try {
        const map: Record<string, string> = await this.coreBridge.getKeywordMap(ext, language);
        this.cache.set(cacheKey, map);
        this.outputChannel.appendLine(`KeywordMapService: loaded ${Object.keys(map).length} keywords for ${programmingLanguage}/${language}`);
      } catch (err: unknown) {
        const message: string = err instanceof Error ? err.message : String(err);
        this.outputChannel.appendLine(`KeywordMapService: failed to load keywords for ${programmingLanguage}/${language} - ${message}`);
        vscode.window.showWarningMessage('Babel TCC: Failed to load translations. Completion and highlighting may not work.');
      }

      const categoryCacheKey: string = `categories::${programmingLanguage}`;
      if (!this.categoryCache.has(categoryCacheKey)) {
        try {
          const categories: Record<string, string> = await this.coreBridge.getKeywordCategories(ext);
          this.categoryCache.set(categoryCacheKey, categories);
          this.outputChannel.appendLine(`KeywordMapService: loaded ${Object.keys(categories).length} categories for ${programmingLanguage}`);
        } catch (err: unknown) {
          const message: string = err instanceof Error ? err.message : String(err);
          this.outputChannel.appendLine(`KeywordMapService: failed to load categories for ${programmingLanguage} - ${message}`);
        }
      }
    }

    const identifierCacheKey: string = `identifiers::${language}`;
    if (!this.identifierCache.has(identifierCacheKey)) {
      try {
        const map: Record<string, string> = await this.coreBridge.getIdentifierMap(language);
        this.identifierCache.set(identifierCacheKey, map);
        this.outputChannel.appendLine(`KeywordMapService: loaded ${Object.keys(map).length} identifiers for ${language}`);
      } catch (err: unknown) {
        const message: string = err instanceof Error ? err.message : String(err);
        this.outputChannel.appendLine(`KeywordMapService: failed to load identifiers for ${language} - ${message}`);
        vscode.window.showWarningMessage('Babel TCC: Failed to load identifier translations. Highlighting may not work.');
      }
    }
  }

  /**
   * Re-fetches the identifier map from the Core, replacing stale cached data.
   * Called after translations populate new identifiers via ApplyTraduAnnotations.
   * @param language - The effective language used in the translation.
   */
  public async refreshIdentifierCache(language: string): Promise<void> {
    const identifierCacheKey: string = `identifiers::${language}`;
    try {
      const map: Record<string, string> = await this.coreBridge.getIdentifierMap(language);
      this.identifierCache.set(identifierCacheKey, map);
      this.outputChannel.appendLine(`KeywordMapService: refreshed ${Object.keys(map).length} identifiers for ${language}`);
    } catch (err: unknown) {
      const message: string = err instanceof Error ? err.message : String(err);
      this.outputChannel.appendLine(`KeywordMapService: failed to refresh identifiers for ${language} - ${message}`);
    }
  }

  /** Disposes of the configuration change subscription. */
  public dispose(): void {
    this.configSubscription.dispose();
  }
}
