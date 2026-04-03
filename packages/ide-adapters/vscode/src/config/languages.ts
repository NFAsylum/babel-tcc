/**
 * Central registry of programming languages supported by the extension.
 * All language-specific configuration should be derived from this single source.
 * When adding a new language, add an entry here and update package.json manually
 * (activationEvents, languages, grammars cannot be generated at runtime).
 */
export interface LanguageConfig {
  /** Internal language name matching the Core adapter (e.g., 'CSharp', 'Python'). */
  name: string;
  /** File extensions including the leading dot (e.g., '.cs', '.py'). */
  extensions: string[];
  /** VS Code language identifier for syntax highlighting (e.g., 'csharp', 'python'). */
  vscodeLangId: string;
}

export const SUPPORTED_LANGUAGES: LanguageConfig[] = [
  { name: 'CSharp', extensions: ['.cs'], vscodeLangId: 'csharp' },
  { name: 'Python', extensions: ['.py'], vscodeLangId: 'python' },
];

/** Builds extension-to-language-name map from the central registry. */
export function buildExtensionMap(): Record<string, string> {
  const map: Record<string, string> = {};
  for (const lang of SUPPORTED_LANGUAGES) {
    for (const ext of lang.extensions) {
      map[ext] = lang.name;
    }
  }
  return map;
}

/** Builds language-name-to-vscode-lang-id map from the central registry. */
export function buildVSCodeLanguageMap(): Record<string, string> {
  const map: Record<string, string> = {};
  for (const lang of SUPPORTED_LANGUAGES) {
    map[lang.name] = lang.vscodeLangId;
  }
  return map;
}

/** Builds a glob pattern matching all supported file extensions. */
export function buildFileWatcherPattern(): string {
  const exts: string[] = SUPPORTED_LANGUAGES.flatMap((lang: LanguageConfig): string[] =>
    lang.extensions.map((ext: string): string => ext.substring(1))
  );
  return '**/*.{' + exts.join(',') + '}';
}
