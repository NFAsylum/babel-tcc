import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';

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
  private languageDetector: LanguageDetector;
  private outputChannel: vscode.OutputChannel;
  private cachedLanguage: string = '';
  private cachedProgrammingLanguage: string = '';
  private cachedMap: Record<string, string> = {};
  private configSubscription: { dispose(): void };

  constructor(
    translationsPath: string,
    configService: ConfigurationService,
    languageDetector: LanguageDetector,
    outputChannel: vscode.OutputChannel
  ) {
    this.translationsPath = translationsPath;
    this.configService = configService;
    this.languageDetector = languageDetector;
    this.outputChannel = outputChannel;
    this.configSubscription = configService.onDidChangeConfiguration(() => {
      this.invalidate();
    });
  }

  /**
   * Returns a map of translated keywords to their original equivalents for the current language
   * and the programming language detected from the given file path.
   * The map is cached and reloaded automatically when the language setting changes.
   * @param filePath - The file path used to detect the programming language.
   */
  public getMap(filePath: string): Record<string, string> {
    const programmingLanguage: string | undefined = this.languageDetector.detectLanguage(filePath);
    if (!programmingLanguage) {
      return {};
    }

    const language: string = this.configService.getLanguage();
    const progLangKey: string = programmingLanguage.toLowerCase();

    if (language !== this.cachedLanguage || progLangKey !== this.cachedProgrammingLanguage) {
      this.load(language, progLangKey);
    }
    return this.cachedMap;
  }

  private invalidate(): void {
    this.cachedLanguage = '';
    this.cachedProgrammingLanguage = '';
  }

  private load(language: string, programmingLanguage: string): void {
    const map: Record<string, string> = {};

    const basePath: string = path.join(
      this.translationsPath, 'programming-languages', programmingLanguage, 'keywords-base.json'
    );
    const translationPath: string = path.join(
      this.translationsPath, 'natural-languages', language, `${programmingLanguage}.json`
    );

    if (!fs.existsSync(basePath) || !fs.existsSync(translationPath)) {
      this.cachedMap = map;
      this.cachedLanguage = language;
      this.cachedProgrammingLanguage = programmingLanguage;
      return;
    }

    try {
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
    } catch (err: unknown) {
      const message: string = err instanceof Error ? err.message : String(err);
      this.outputChannel.appendLine(`KeywordMapService: failed to load translations - ${message}`);
      vscode.window.showWarningMessage(
        `Babel TCC: Failed to load keyword translations for ${programmingLanguage}/${language}. Completion and hover may not work.`
      );
      this.cachedMap = map;
      return;
    }

    this.cachedMap = map;
    this.cachedLanguage = language;
    this.cachedProgrammingLanguage = programmingLanguage;
  }

  /** Disposes of the configuration change subscription. */
  public dispose(): void {
    this.configSubscription.dispose();
  }
}
