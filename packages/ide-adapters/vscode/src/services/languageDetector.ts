import * as path from 'path';
import { buildExtensionMap } from '../config/languages';

/** Detects programming languages by file extension. */
export class LanguageDetector {
  /** Map of file extensions to their programming language names (e.g., ".cs" -> "CSharp"). */
  public supportedExtensions: Record<string, string>;

  constructor() {
    this.supportedExtensions = buildExtensionMap();
  }

  /**
   * Detects the programming language of a file based on its extension.
   * @param filePath - The file path to inspect.
   * @returns The language name (e.g., "CSharp") if supported, or `undefined` if not recognized.
   */
  public detectLanguage(filePath: string): string | undefined {
    const extension: string = path.extname(filePath).toLowerCase();
    const language: string | undefined = this.supportedExtensions[extension];
    return language;
  }

  /**
   * Checks whether the given file's programming language is supported for translation.
   * @param filePath - The file path to check.
   * @returns `true` if the file extension maps to a supported language, `false` otherwise.
   */
  public isSupported(filePath: string): boolean {
    const language: string | undefined = this.detectLanguage(filePath);
    return language !== undefined;
  }

  /**
   * Extracts the lowercase file extension from a file path.
   * @param filePath - The file path to extract the extension from.
   * @returns The file extension including the leading dot (e.g., ".cs").
   */
  public getFileExtension(filePath: string): string {
    return path.extname(filePath).toLowerCase();
  }

  /**
   * Returns all file extensions currently supported for translation.
   * @returns An array of extension strings (e.g., [".cs", ".py"]).
   */
  public getSupportedExtensions(): string[] {
    return Object.keys(this.supportedExtensions);
  }
}
