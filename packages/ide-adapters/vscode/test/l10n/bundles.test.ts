import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Contract test between the `vscode.l10n.t()` calls in src/ and the runtime translation bundles.
 * A string added to the code without an entry in every bundle silently falls back to English for
 * that locale, which is a regression against INV-04 rather than a detail to fix later.
 *
 * The extraction is static, so it imposes the same convention the official vscode-l10n-dev tool
 * imposes: the literal passed to `l10n.t()` stays on a single line, in single quotes, and is never
 * assembled by concatenation or by a template literal.
 */

const EXTENSION_ROOT: string = path.join(__dirname, '../..');
const SOURCE_DIR: string = path.join(EXTENSION_ROOT, 'src');
const BUNDLE_DIR: string = path.join(EXTENSION_ROOT, 'l10n');

const BUNDLE_FILENAMES: string[] = [
  'bundle.l10n.pt-br.json',
  'bundle.l10n.es.json',
  'bundle.l10n.fr.json',
  'bundle.l10n.de.json',
  'bundle.l10n.it.json',
  'bundle.l10n.ja.json',
];

const L10N_CALL_PATTERN: RegExp = /l10n\.t\(\s*'((?:[^'\\]|\\.)*)'/g;

/** Lists every TypeScript file under a directory, recursively. */
function listTypeScriptFiles(directory: string): string[] {
  const found: string[] = [];
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const entryPath: string = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      found.push(...listTypeScriptFiles(entryPath));
    } else if (entry.name.endsWith('.ts')) {
      found.push(entryPath);
    }
  }
  return found;
}

/** Extracts the literal message of every `l10n.t()` call found in the sources. */
function collectTranslatedMessages(): string[] {
  const messages: Set<string> = new Set<string>();
  for (const filePath of listTypeScriptFiles(SOURCE_DIR)) {
    const sourceText: string = fs.readFileSync(filePath, 'utf-8');
    let match: RegExpExecArray | null = L10N_CALL_PATTERN.exec(sourceText);
    while (match !== null) {
      messages.add(match[1]);
      match = L10N_CALL_PATTERN.exec(sourceText);
    }
  }
  return [...messages];
}

function readBundle(filename: string): Record<string, string> {
  return JSON.parse(fs.readFileSync(path.join(BUNDLE_DIR, filename), 'utf-8'));
}

describe('l10n bundle contract', () => {
  const translatedMessages: string[] = collectTranslatedMessages();

  it('should find the l10n calls of the extension in the sources', () => {
    expect(translatedMessages.length).toBeGreaterThan(0);
    expect(translatedMessages).toContain('Babel TCC: No active editor.');
  });

  it('should translate every message in all 6 bundles (INV-04)', () => {
    for (const filename of BUNDLE_FILENAMES) {
      const bundleKeys: string[] = Object.keys(readBundle(filename));
      const missing: string[] = translatedMessages.filter(
        (message: string): boolean => !bundleKeys.includes(message)
      );
      expect({ filename, missing }).toEqual({ filename, missing: [] });
    }
  });

  it('should not keep an orphan entry in any bundle', () => {
    for (const filename of BUNDLE_FILENAMES) {
      const orphans: string[] = Object.keys(readBundle(filename)).filter(
        (key: string): boolean => !translatedMessages.includes(key)
      );
      expect({ filename, orphans }).toEqual({ filename, orphans: [] });
    }
  });

  it('should never leave a translation empty', () => {
    for (const filename of BUNDLE_FILENAMES) {
      const emptyKeys: string[] = Object.entries(readBundle(filename))
        .filter(([, value]: [string, string]): boolean => value.trim() === '')
        .map(([key]: [string, string]): string => key);
      expect({ filename, emptyKeys }).toEqual({ filename, emptyKeys: [] });
    }
  });

  it('should keep the same placeholders in every translation', () => {
    const placeholderPattern: RegExp = /\{\d+\}/g;
    for (const filename of BUNDLE_FILENAMES) {
      for (const [message, translation] of Object.entries(readBundle(filename))) {
        const sourcePlaceholders: string[] = (message.match(placeholderPattern) ?? []).sort();
        const translatedPlaceholders: string[] = (translation.match(placeholderPattern) ?? []).sort();
        expect({ filename, message, translatedPlaceholders }).toEqual({
          filename,
          message,
          translatedPlaceholders: sourcePlaceholders,
        });
      }
    }
  });
});
