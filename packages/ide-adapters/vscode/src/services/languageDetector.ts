import * as path from 'path';

const SUPPORTED_EXTENSIONS: Record<string, string> = {
  '.cs': 'CSharp'
};

export class LanguageDetector {
  public supportedExtensions: Record<string, string>;

  constructor() {
    this.supportedExtensions = { ...SUPPORTED_EXTENSIONS };
  }

  public detectLanguage(filePath: string): string | undefined {
    const extension: string = path.extname(filePath).toLowerCase();
    const language: string | undefined = this.supportedExtensions[extension];
    return language;
  }

  public isSupported(filePath: string): boolean {
    const language: string | undefined = this.detectLanguage(filePath);
    return language !== undefined;
  }

  public getFileExtension(filePath: string): string {
    return path.extname(filePath).toLowerCase();
  }

  public getSupportedExtensions(): string[] {
    return Object.keys(this.supportedExtensions);
  }
}
