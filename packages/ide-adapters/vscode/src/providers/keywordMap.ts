import * as path from 'path';
import * as fs from 'fs';
import { ConfigurationService } from '../services/configurationService';

interface KeywordsBase {
  keywords: Record<string, number>;
}

interface TranslationFile {
  translations: Record<string, string>;
}

/** Loads keyword translation maps dynamically from the translation tables for the currently selected language. */
export class KeywordMapService {
  private translationsPath: string;
  private configService: ConfigurationService;
  private cachedLanguage: string = '';
  private cachedMap: Record<string, string> = {};
  private configSubscription: { dispose(): void };

  constructor(translationsPath: string, configService: ConfigurationService) {
    this.translationsPath = translationsPath;
    this.configService = configService;
    this.configSubscription = configService.onDidChangeConfiguration(() => {
      this.invalidate();
    });
    this.load();
  }

  /**
   * Returns a map of translated keywords to their original C# equivalents for the current language.
   * The map is cached and reloaded automatically when the language setting changes.
   */
  public getMap(): Record<string, string> {
    const language: string = this.configService.getLanguage();
    if (language !== this.cachedLanguage) {
      this.load();
    }
    return this.cachedMap;
  }

  private invalidate(): void {
    this.cachedLanguage = '';
  }

  private load(): void {
    const language: string = this.configService.getLanguage();
    const map: Record<string, string> = {};

    const basePath: string = path.join(
      this.translationsPath, 'programming-languages', 'csharp', 'keywords-base.json'
    );
    const translationPath: string = path.join(
      this.translationsPath, 'natural-languages', language, 'csharp.json'
    );

    if (!fs.existsSync(basePath) || !fs.existsSync(translationPath)) {
      this.cachedMap = map;
      this.cachedLanguage = language;
      return;
    }

    const base: KeywordsBase = JSON.parse(fs.readFileSync(basePath, 'utf-8')) as KeywordsBase;
    const translation: TranslationFile = JSON.parse(
      fs.readFileSync(translationPath, 'utf-8')
    ) as TranslationFile;

    for (const [original, id] of Object.entries(base.keywords)) {
      const translated: string | undefined = translation.translations[String(id)];
      if (translated) {
        map[translated] = original;
      }
    }

    this.cachedMap = map;
    this.cachedLanguage = language;
  }

  /** Disposes of the configuration change subscription. */
  public dispose(): void {
    this.configSubscription.dispose();
  }
}
