import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { COMMANDS, CONFIG_KEYS, WALKTHROUGH_ID } from '../../src/config/constants';
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

interface ManifestWalkthroughStep {
  id: string;
  title: string;
  description: string;
  media: { markdown?: string; image?: unknown; altText?: string };
  when?: string;
  completionEvents?: string[];
}

interface ManifestWalkthrough {
  id: string;
  title: string;
  description: string;
  steps: ManifestWalkthroughStep[];
}

interface PackageManifest {
  contributes: {
    commands: ManifestCommand[];
    menus: Record<string, ManifestMenuItem[]>;
    keybindings: ManifestKeybinding[];
    walkthroughs: ManifestWalkthrough[];
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

/** A markdown link: the button label and where it goes. Both are empty when there is no link. */
interface DescriptionLink {
  label: string;
  target: string;
}

/**
 * Reads the link that closes a walkthrough step description. VS Code draws a link that sits alone on
 * the last line as a button, which is what makes the step doable from the keyboard (INV-09).
 */
function trailingLinkOf(description: string): DescriptionLink {
  const match: RegExpMatchArray | null = description.match(/\n\[([^\]]+)\]\(([^)]+)\)$/);
  if (match) {
    return { label: match[1], target: match[2] };
  }
  return { label: '', target: '' };
}

/** Returns the command a `command:` link runs, without the `toSide:` hint; '' for other links. */
function commandOfLink(target: string): string {
  const match: RegExpMatchArray | null = target.match(/^command:(?:toSide:)?(.+)$/);
  if (match) {
    return match[1];
  }
  return '';
}

/** Turns a `%key%` placeholder into its key. */
function placeholderKeyOf(value: string): string {
  return value.replace(/^%(.+)%$/, '$1');
}

/** Normalizes the optional completion events of a step to an array. */
function completionEventsOf(step: ManifestWalkthroughStep): string[] {
  return listOf(step.completionEvents);
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

  describe('walkthrough', () => {
    const walkthroughs: ManifestWalkthrough[] = packageJson.contributes.walkthroughs || [];
    const steps: ManifestWalkthroughStep[] = walkthroughs.flatMap(
      (walkthrough: ManifestWalkthrough): ManifestWalkthroughStep[] => walkthrough.steps
    );
    const baseTranslations: Record<string, string> = readNlsFile('package.nls.json');

    /** The link that closes a step, read from the given nls file. */
    function stepLinkIn(translations: Record<string, string>, step: ManifestWalkthroughStep): DescriptionLink {
      return trailingLinkOf(translations[placeholderKeyOf(step.description)] || '');
    }

    it('should declare one walkthrough, with the id that "Open Introduction" opens', () => {
      expect(walkthroughs.map((walkthrough: ManifestWalkthrough): string => walkthrough.id)).toEqual([WALKTHROUGH_ID]);
      expect(steps.length).toBeGreaterThan(0);
    });

    // Markdown media is read by screen readers, follows the theme and needs no light/dark pair
    // (INV-08); a media file left out of the package would show an empty step (INV-16).
    it('should give every step markdown media that exists in the extension folder', () => {
      for (const step of steps) {
        const markdownPath: string = step.media.markdown || '';
        expect({ step: step.id, image: step.media.image }).toEqual({ step: step.id, image: undefined });
        expect({ step: step.id, exists: markdownPath !== '' && fs.existsSync(path.join(EXTENSION_ROOT, markdownPath)) }).toEqual({
          step: step.id,
          exists: true,
        });
      }
    });

    it('should end every step description with a link, in all 7 nls files', () => {
      for (const filename of NLS_FILENAMES) {
        const translations: Record<string, string> = readNlsFile(filename);
        for (const step of steps) {
          const link: DescriptionLink = stepLinkIn(translations, step);
          expect({ filename, step: step.id, hasLabel: link.label.trim() !== '' }).toEqual({ filename, step: step.id, hasLabel: true });
          // Only the label is translated: the button must do the same thing in every language.
          expect({ filename, step: step.id, target: link.target }).toEqual({
            filename,
            step: step.id,
            target: stepLinkIn(baseTranslations, step).target,
          });
        }
      }
    });

    it('should point every command link at a declared command', () => {
      for (const step of steps) {
        const target: string = stepLinkIn(baseTranslations, step).target;
        if (target.startsWith('command:')) {
          expect(declaredCommandIds).toContain(commandOfLink(target));
        }
      }
    });

    // From the walkthrough no text editor is active; `toSide:` makes VS Code open the result next to
    // the walkthrough and focus that group, so a command that needs the active editor still has one.
    it('should open editors beside the walkthrough', () => {
      const editorCommands: string[] = [COMMANDS.OPEN_EXAMPLE, COMMANDS.SHOW_ORIGINAL];
      for (const step of steps) {
        const target: string = stepLinkIn(baseTranslations, step).target;
        if (editorCommands.includes(commandOfLink(target))) {
          expect(target).toBe(`command:toSide:${commandOfLink(target)}`);
        }
      }
    });

    // "Toggle" turns translation off as readily as on: a button for it is only safe in a step that is
    // shown while translation is off, and that step must come first, before anything that needs it.
    it('should only offer the toggle in a first step that shows while translation is off', () => {
      const toggleSteps: ManifestWalkthroughStep[] = steps.filter(
        (step: ManifestWalkthroughStep): boolean =>
          commandOfLink(stepLinkIn(baseTranslations, step).target) === COMMANDS.TOGGLE
      );
      expect(toggleSteps.map((step: ManifestWalkthroughStep): string => step.id)).toEqual([steps[0].id]);
      expect(whenOf(toggleSteps[0])).toBe(`!${ENABLED_CLAUSE}`);
    });

    it('should show every other step regardless of the settings', () => {
      for (const step of steps.slice(1)) {
        expect({ step: step.id, when: whenOf(step) }).toEqual({ step: step.id, when: '' });
      }
    });

    it('should complete each step when its button is used', () => {
      for (const step of steps) {
        const target: string = stepLinkIn(baseTranslations, step).target;
        let expectedEvent: string = `onLink:${target}`;
        if (target.startsWith('command:')) {
          expectedEvent = `onCommand:${commandOfLink(target)}`;
        }
        expect({ step: step.id, events: completionEventsOf(step) }).toEqual({
          step: step.id,
          events: expect.arrayContaining([expectedEvent]),
        });
      }
    });

    it('should only complete steps on commands and settings that exist', () => {
      const declaredSettings: string[] = Object.keys(configurationProperties);
      for (const step of steps) {
        for (const event of completionEventsOf(step)) {
          const [kind, ...rest] = event.split(':');
          const subject: string = rest.join(':');
          if (kind === 'onCommand') {
            expect(declaredCommandIds).toContain(subject);
          } else if (kind === 'onSettingChanged') {
            expect(declaredSettings).toContain(subject);
          } else {
            expect({ step: step.id, kind }).toEqual({ step: step.id, kind: 'onLink' });
          }
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
