import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { COMMANDS, CONFIG_KEYS } from '../../src/config/constants';
import { SUPPORTED_LANGUAGES, LanguageConfig } from '../../src/config/languages';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

/**
 * Contract tests between package.json, the 7 package.nls*.json files and the commands declared in
 * constants.ts. The manifest is data, and data without validation drifts: this file is to the UI
 * layer what scripts/validate.py is to the translation tables.
 *
 * The interfaces below describe this test's view of package.json. Their optional fields mirror the
 * manifest schema, where `when`, `mac` or `icon` may be absent; the `...Of` helpers normalize each
 * one to a non-optional value right at that boundary, so no optional value flows into the asserts.
 */

interface ManifestCommand {
  command: string;
  title: string;
  shortTitle?: string;
  category?: string;
  icon?: string;
}

interface ManifestMenuItem {
  command: string;
  when?: string;
  group?: string;
}

interface ManifestConfigProperty {
  type: string;
  description: string;
  order?: number;
  enum?: string[];
  enumItemLabels?: string[];
  enumDescriptions?: string[];
  properties?: Record<string, ManifestConfigProperty>;
}

interface ManifestKeybinding {
  command: string;
  key: string;
  mac?: string;
  when?: string;
}

interface PackageManifest {
  contributes: {
    commands: ManifestCommand[];
    menus: Record<string, ManifestMenuItem[]>;
    keybindings: ManifestKeybinding[];
    configuration: {
      properties: Record<string, ManifestConfigProperty>;
    };
  };
}

const EXTENSION_ROOT: string = path.join(__dirname, '../..');

const NLS_FILENAMES: string[] = [
  'package.nls.json',
  'package.nls.pt-br.json',
  'package.nls.es.json',
  'package.nls.fr.json',
  'package.nls.de.json',
  'package.nls.it.json',
  'package.nls.ja.json',
];

/**
 * Keys that the Brazilian ABNT2 layout reaches through AltGr (¹ ² ³ £ ¢ ¬ § ª º / ? ° ₢). On
 * Windows AltGr arrives at applications as Ctrl+Alt, so a `ctrl+alt+<key>` binding on one of these
 * fires while the user is just typing a character.
 *
 * The check below rejects these keys in every chord segment, not only in the prefix. Strictly, the
 * risk lives in the prefix: the second key of a chord is pressed with no modifier, so AltGr never
 * produces it. Rejecting it everywhere costs nothing (no planned binding needs one of these keys)
 * and keeps the rule a single sentence for whoever adds the next keybinding.
 */
const ABNT2_ALTGR_KEYS: string[] = ['1', '2', '3', '4', '5', '6', '=', '[', ']', 'q', 'w', 'e', 'c', '/'];

const KEYBINDING_MODIFIERS: string[] = ['ctrl', 'cmd', 'alt', 'shift', 'meta', 'win'];

/**
 * Chord prefix of every keybinding. `ctrl+alt+b` was the first choice and is taken by the built-in
 * "Toggle Secondary Side Bar": an extension chord on a built-in prefix disables the built-in
 * shortcut for everyone who installs the extension. `u` is free in VS Code's default keymap and
 * outside the ABNT2 AltGr keys.
 */
const CHORD_PREFIX: string = 'ctrl+alt+u ';
const CHORD_PREFIX_MAC: string = 'cmd+alt+u ';

/**
 * `when` fragments that the editor menus and keybindings must use. They are built from the language
 * registry and from the URI scheme constants, so adding a language or renaming a scheme without
 * updating the manifest breaks here. Per-editor facts use the built-in `resourceExtname` and
 * `resourceScheme` keys because VS Code evaluates them per editor group; a key published by the
 * extension with `setContext` is global and would make every group show the buttons of the active
 * editor (DT-012). The enabled flag is read straight from the setting through the built-in
 * `config.` prefix, so the extension publishes no context key at all. The extension regex is
 * case-insensitive like `LanguageDetector`, which lowercases before lookup.
 */
const SUPPORTED_EXTENSIONS: string[] = SUPPORTED_LANGUAGES.flatMap(
  (language: LanguageConfig): string[] => language.extensions.map((extension: string): string => extension.substring(1))
);
const SUPPORTED_FILE_CLAUSE: string = `resourceExtname =~ /^\\.(${SUPPORTED_EXTENSIONS.join('|')})$/i`;
const NOT_TRANSLATED_CLAUSE: string =
  `resourceScheme != '${TRANSLATED_SCHEME}' && resourceScheme != '${READONLY_SCHEME}'`;
const TRANSLATED_CLAUSE: string = `resourceScheme =~ /^(${TRANSLATED_SCHEME}|${READONLY_SCHEME})$/`;
const ENABLED_CLAUSE: string = `config.${CONFIG_KEYS.ENABLED}`;

const OPEN_TRANSLATED_COMMANDS: string[] = [COMMANDS.OPEN_TRANSLATED_EDITABLE, COMMANDS.OPEN_TRANSLATED_READONLY];

/**
 * Where the natural-language folders can be found, most authoritative first: the repository named
 * by BABEL_TCC_TRANSLATIONS_PATH (what CI and scripts/copy-translations.js use), the copy that the
 * build places inside the extension, then the sibling translations repository of a developer checkout.
 */
function listTranslationSources(): string[] {
  const sources: string[] = [];
  const configuredPath: string = process.env.BABEL_TCC_TRANSLATIONS_PATH || '';
  if (configuredPath !== '') {
    sources.push(path.join(configuredPath, 'natural-languages'));
  }
  sources.push(path.join(EXTENSION_ROOT, 'translations', 'natural-languages'));
  sources.push(path.join(EXTENSION_ROOT, '..', '..', '..', '..', 'babel-tcc-translations', 'natural-languages'));
  return sources;
}

/** Lists the locales that ship with the extension, or nothing when no source is reachable. */
function findShippedLocales(): string[] {
  for (const source of listTranslationSources()) {
    if (fs.existsSync(source)) {
      return fs
        .readdirSync(source, { withFileTypes: true })
        .filter((entry: fs.Dirent): boolean => entry.isDirectory())
        .map((entry: fs.Dirent): string => entry.name);
    }
  }
  return [];
}

const packageJson: PackageManifest = JSON.parse(
  fs.readFileSync(path.join(EXTENSION_ROOT, 'package.json'), 'utf-8')
);

const manifestCommands: ManifestCommand[] = packageJson.contributes.commands;
const manifestMenus: Record<string, ManifestMenuItem[]> = packageJson.contributes.menus;
const manifestKeybindings: ManifestKeybinding[] = packageJson.contributes.keybindings;
const configurationProperties: Record<string, ManifestConfigProperty> =
  packageJson.contributes.configuration.properties;
const declaredCommandIds: string[] = manifestCommands.map((c: ManifestCommand): string => c.command);
const editorMenuItems: ManifestMenuItem[] = [...manifestMenus['editor/title'], ...manifestMenus['editor/context']];

function readNlsFile(filename: string): Record<string, string> {
  return JSON.parse(fs.readFileSync(path.join(EXTENSION_ROOT, filename), 'utf-8'));
}

/** Normalizes an optional `when` clause to a string; an absent clause becomes empty. */
function whenOf(entry: { when?: string }): string {
  if (typeof entry.when === 'string') {
    return entry.when;
  }
  return '';
}

/** Normalizes an optional mac keybinding to a string; an absent one becomes empty. */
function macOf(keybinding: ManifestKeybinding): string {
  if (typeof keybinding.mac === 'string') {
    return keybinding.mac;
  }
  return '';
}

/** Normalizes an optional string list of the manifest to an array; an absent list becomes empty. */
function listOf(values: string[] | undefined): string[] {
  if (Array.isArray(values)) {
    return values;
  }
  return [];
}

/** Collects every `%key%` placeholder used anywhere in the manifest, at any nesting depth. */
function collectNlsPlaceholders(node: unknown, found: Set<string>): Set<string> {
  if (typeof node === 'string') {
    const match: RegExpMatchArray | null = node.match(/^%(.+)%$/);
    if (match) {
      found.add(match[1]);
    }
    return found;
  }
  if (Array.isArray(node)) {
    for (const child of node) {
      collectNlsPlaceholders(child, found);
    }
    return found;
  }
  if (node !== null && typeof node === 'object') {
    for (const child of Object.values(node as Record<string, unknown>)) {
      collectNlsPlaceholders(child, found);
    }
  }
  return found;
}

/** Splits a keybinding into its chord segments (`ctrl+alt+u t` -> `ctrl+alt+u`, `t`). */
function splitChord(keybinding: string): string[] {
  return keybinding.split(' ').filter((segment: string): boolean => segment !== '');
}

/** Returns the key of a chord segment without its modifiers (`ctrl+alt+u` -> `u`). */
function baseKeyOf(segment: string): string {
  const parts: string[] = segment.split('+');
  return parts[parts.length - 1];
}

describe('manifest contract', () => {
  describe('commands', () => {
    it('should declare every command listed in constants.ts', () => {
      for (const commandId of Object.values(COMMANDS)) {
        expect(declaredCommandIds).toContain(commandId);
      }
    });

    it('should not declare a command that constants.ts does not know about', () => {
      const knownCommandIds: string[] = Object.values(COMMANDS);
      for (const commandId of declaredCommandIds) {
        expect(knownCommandIds).toContain(commandId);
      }
    });

    it('should give every command a category instead of a prefixed title', () => {
      for (const command of manifestCommands) {
        expect(command.category).toBe('%extension.category%');
      }
    });

    it('should not repeat the category inside any translated command title', () => {
      for (const filename of NLS_FILENAMES) {
        const translations: Record<string, string> = readNlsFile(filename);
        const category: string = translations['extension.category'];
        const prefixedTitles: string[] = Object.entries(translations)
          .filter(([key]: [string, string]): boolean => key.startsWith('command.'))
          .filter(([, value]: [string, string]): boolean => value.startsWith(category))
          .map(([key]: [string, string]): string => key);
        expect({ filename, prefixedTitles }).toEqual({ filename, prefixedTitles: [] });
      }
    });
  });

  describe('nls placeholders', () => {
    const usedPlaceholders: Set<string> = collectNlsPlaceholders(packageJson, new Set<string>());

    it('should use at least the command and configuration placeholders', () => {
      expect(usedPlaceholders.has('extension.category')).toBe(true);
      expect(usedPlaceholders.has('command.toggle.title')).toBe(true);
    });

    it('should resolve every placeholder in all 7 nls files', () => {
      for (const filename of NLS_FILENAMES) {
        const translatedKeys: string[] = Object.keys(readNlsFile(filename));
        const unresolved: string[] = [...usedPlaceholders].filter(
          (placeholder: string): boolean => !translatedKeys.includes(placeholder)
        );
        expect({ filename, unresolved }).toEqual({ filename, unresolved: [] });
      }
    });

    it('should not keep an orphan key in any nls file', () => {
      for (const filename of NLS_FILENAMES) {
        const translations: Record<string, string> = readNlsFile(filename);
        for (const key of Object.keys(translations)) {
          expect(usedPlaceholders.has(key)).toBe(true);
        }
      }
    });

    it('should translate every key in every locale, with the base file as the reference', () => {
      const baseKeys: string[] = Object.keys(readNlsFile('package.nls.json')).sort();
      for (const filename of NLS_FILENAMES) {
        expect(Object.keys(readNlsFile(filename)).sort()).toEqual(baseKeys);
      }
    });
  });

  describe('menus', () => {
    it('should only reference declared commands', () => {
      for (const menuItems of Object.values(manifestMenus)) {
        for (const menuItem of menuItems) {
          expect(declaredCommandIds).toContain(menuItem.command);
        }
      }
    });

    it('should give every editor menu item a non-empty when clause', () => {
      for (const menuItem of editorMenuItems) {
        expect({ command: menuItem.command, when: whenOf(menuItem) }).not.toEqual({
          command: menuItem.command,
          when: '',
        });
      }
    });

    it('should give every editor/title command a codicon', () => {
      for (const menuItem of manifestMenus['editor/title']) {
        const matchingCommands: ManifestCommand[] = manifestCommands.filter(
          (c: ManifestCommand): boolean => c.command === menuItem.command
        );
        expect(matchingCommands.length).toBe(1);
        expect(matchingCommands[0].icon).toMatch(/^\$\([a-z0-9-]+\)$/);
      }
    });

    it('should offer the translated view only for a supported original file, while enabled', () => {
      const openItems: ManifestMenuItem[] = editorMenuItems.filter(
        (menuItem: ManifestMenuItem): boolean => OPEN_TRANSLATED_COMMANDS.includes(menuItem.command)
      );
      expect(openItems.length).toBeGreaterThan(0);
      for (const menuItem of openItems) {
        const when: string = whenOf(menuItem);
        expect(when).toContain(ENABLED_CLAUSE);
        expect(when).toContain(SUPPORTED_FILE_CLAUSE);
        expect(when).toContain(NOT_TRANSLATED_CLAUSE);
      }
    });

    it('should offer the original only from a translated view', () => {
      const originalItems: ManifestMenuItem[] = editorMenuItems.filter(
        (menuItem: ManifestMenuItem): boolean => menuItem.command === COMMANDS.SHOW_ORIGINAL
      );
      expect(originalItems.length).toBeGreaterThan(0);
      for (const menuItem of originalItems) {
        expect(whenOf(menuItem)).toContain(TRANSLATED_CLAUSE);
      }
    });

    // The two clauses are complementary on resourceScheme (one excludes both translated schemes,
    // the other requires one of them), so at most one of the two buttons matches a given editor.
    it('should never show two competing editor/title buttons at once', () => {
      const titleCommands: string[] = manifestMenus['editor/title'].map(
        (menuItem: ManifestMenuItem): string => menuItem.command
      );
      expect(titleCommands.sort()).toEqual([COMMANDS.OPEN_TRANSLATED_EDITABLE, COMMANDS.SHOW_ORIGINAL].sort());
    });

    it('should not hide any command from the Command Palette', () => {
      expect(Object.keys(manifestMenus)).not.toContain('commandPalette');
    });
  });

  describe('context keys', () => {
    const whenClauses: string[] = [
      ...Object.values(manifestMenus).flat().map(whenOf),
      ...manifestKeybindings.map(whenOf),
    ];
    const declaredSettings: string[] = Object.keys(configurationProperties);

    it('should not depend on a context key published by the extension', () => {
      const extensionKeys: string[] = whenClauses.flatMap(
        (when: string): string[] => when.match(/babelTcc\.[A-Za-z]+/g) || []
      );
      expect(extensionKeys).toEqual([]);
    });

    it('should only read settings that the extension declares', () => {
      const readSettings: string[] = whenClauses.flatMap((when: string): string[] =>
        (when.match(/config\.[A-Za-z0-9.-]+/g) || []).map((key: string): string => key.substring('config.'.length))
      );
      expect(readSettings.length).toBeGreaterThan(0);
      for (const setting of readSettings) {
        expect(declaredSettings).toContain(setting);
      }
    });
  });

  describe('keybindings', () => {
    it('should only reference declared commands', () => {
      for (const keybinding of manifestKeybindings) {
        expect(declaredCommandIds).toContain(keybinding.command);
      }
    });

    it('should never bind a bare key without a modifier', () => {
      for (const keybinding of manifestKeybindings) {
        const firstSegment: string = splitChord(keybinding.key)[0];
        const hasModifier: boolean = KEYBINDING_MODIFIERS.some(
          (modifier: string): boolean => firstSegment.startsWith(`${modifier}+`)
        );
        expect({ command: keybinding.command, hasModifier }).toEqual({
          command: keybinding.command,
          hasModifier: true,
        });
      }
    });

    it('should declare a mac variant for every binding', () => {
      for (const keybinding of manifestKeybindings) {
        expect({ command: keybinding.command, mac: macOf(keybinding) }).not.toEqual({
          command: keybinding.command,
          mac: '',
        });
      }
    });

    it('should use the same chord prefix on every binding', () => {
      for (const keybinding of manifestKeybindings) {
        expect(keybinding.key.startsWith(CHORD_PREFIX)).toBe(true);
        expect(macOf(keybinding).startsWith(CHORD_PREFIX_MAC)).toBe(true);
      }
    });

    it('should never use an ABNT2 AltGr key anywhere in a chord', () => {
      for (const keybinding of manifestKeybindings) {
        const altGrKeys: string[] = splitChord(keybinding.key)
          .map(baseKeyOf)
          .filter((baseKey: string): boolean => ABNT2_ALTGR_KEYS.includes(baseKey));
        expect({ command: keybinding.command, altGrKeys }).toEqual({ command: keybinding.command, altGrKeys: [] });
      }
    });

    it('should not bind the same chord to two commands', () => {
      const chords: string[] = manifestKeybindings.map((k: ManifestKeybinding): string => k.key);
      expect(new Set(chords).size).toBe(chords.length);
    });

    it('should use the same visibility clauses as the editor menus', () => {
      for (const keybinding of manifestKeybindings) {
        const when: string = whenOf(keybinding);
        if (OPEN_TRANSLATED_COMMANDS.includes(keybinding.command)) {
          expect(when).toContain(SUPPORTED_FILE_CLAUSE);
          expect(when).toContain(NOT_TRANSLATED_CLAUSE);
        }
        if (keybinding.command === COMMANDS.SHOW_ORIGINAL) {
          expect(when).toContain(TRANSLATED_CLAUSE);
        }
      }
    });
  });

  describe('configuration', () => {
    const languageProperty: ManifestConfigProperty = configurationProperties['babel-tcc.language'];
    const offeredLocales: string[] = listOf(languageProperty.enum);

    it('should offer the target language as a list instead of free text', () => {
      expect(offeredLocales.length).toBeGreaterThan(0);
    });

    // The golden rule of the translation repo, extended to the UI layer: add a locale and forget the
    // manifest, and this breaks. It does not skip: CI checks out the translations repository and
    // points BABEL_TCC_TRANSLATIONS_PATH at it, and a developer checkout has it as a sibling folder.
    it('should offer exactly the locales that ship with the extension', () => {
      const shippedLocales: string[] = findShippedLocales();
      expect(
        shippedLocales.length,
        'translations not found: set BABEL_TCC_TRANSLATIONS_PATH to the babel-tcc-translations repository'
      ).toBeGreaterThan(0);
      // Compared as sets: the manifest order is editorial (the two Portuguese variants first, for
      // the target audience) and must stay free to change.
      expect([...offeredLocales].sort()).toEqual([...shippedLocales].sort());
    });

    it('should label and describe every locale it offers', () => {
      expect(listOf(languageProperty.enumItemLabels).length).toBe(offeredLocales.length);
      expect(listOf(languageProperty.enumDescriptions).length).toBe(offeredLocales.length);
    });

    it('should never leave a locale label empty', () => {
      const emptyLabels: string[] = listOf(languageProperty.enumItemLabels).filter(
        (label: string): boolean => label.trim() === ''
      );
      expect(emptyLabels).toEqual([]);
    });

    it('should offer the same locales in every per-language override', () => {
      const overrideProperties: Record<string, ManifestConfigProperty> =
        configurationProperties['babel-tcc.languageOverrides'].properties || {};
      expect(Object.keys(overrideProperties).length).toBeGreaterThan(0);
      for (const [programmingLanguage, override] of Object.entries(overrideProperties)) {
        expect({ programmingLanguage, locales: override.enum }).toEqual({
          programmingLanguage,
          locales: languageProperty.enum,
        });
        expect({ programmingLanguage, labels: override.enumItemLabels }).toEqual({
          programmingLanguage,
          labels: languageProperty.enumItemLabels,
        });
      }
    });

    it('should order every setting explicitly', () => {
      const unordered: string[] = Object.entries(configurationProperties)
        .filter(([, property]: [string, ManifestConfigProperty]): boolean =>
          typeof property.order !== 'number'
        )
        .map(([name]: [string, ManifestConfigProperty]): string => name);
      expect(unordered).toEqual([]);
    });

    it('should not give two settings the same position', () => {
      const positions: unknown[] = Object.values(configurationProperties).map(
        (property: ManifestConfigProperty): unknown => property.order
      );
      expect(new Set(positions).size).toBe(positions.length);
    });
  });
});
