import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { COMMANDS } from '../../src/config/constants';

/**
 * Contract tests between package.json, the 7 package.nls*.json files and the commands declared in
 * constants.ts. The manifest is data, and data without validation drifts: this file is to the UI
 * layer what scripts/validate.py is to the translation tables.
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

interface ManifestKeybinding {
  command: string;
  key: string;
  mac?: string;
  when?: string;
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
 * Keys that the ABNT2 layout reaches through AltGr. On Windows AltGr arrives at applications as
 * Ctrl+Alt, so a `ctrl+alt+<key>` binding on one of these fires while the user is just typing a
 * character. Documented in guia-ux-acessibilidade-vscode_1.md, INV-14.
 *
 * The check below rejects these keys in every chord segment, not only in the prefix. Strictly, the
 * risk lives in the prefix: the second key of a chord is pressed with no modifier, so AltGr never
 * produces it. Rejecting it everywhere costs nothing (no planned binding needs one of these keys)
 * and keeps the rule a single sentence for whoever adds the next keybinding.
 */
const ABNT2_ALTGR_KEYS: string[] = ['q', 'w', 'e', 'c', '1', '2', '3', '/'];

const KEYBINDING_MODIFIERS: string[] = ['ctrl', 'cmd', 'alt', 'shift', 'meta', 'win'];

const packageJson: Record<string, any> = JSON.parse(
  fs.readFileSync(path.join(EXTENSION_ROOT, 'package.json'), 'utf-8')
);

const manifestCommands: ManifestCommand[] = packageJson['contributes']['commands'];
const manifestMenus: Record<string, ManifestMenuItem[]> = packageJson['contributes']['menus'];
const manifestKeybindings: ManifestKeybinding[] = packageJson['contributes']['keybindings'];
const declaredCommandIds: string[] = manifestCommands.map((c: ManifestCommand): string => c.command);

function readNlsFile(filename: string): Record<string, string> {
  return JSON.parse(fs.readFileSync(path.join(EXTENSION_ROOT, filename), 'utf-8'));
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

/** Splits a keybinding into its chord segments (`ctrl+alt+b t` -> `ctrl+alt+b`, `t`). */
function splitChord(keybinding: string): string[] {
  return keybinding.split(' ').filter((segment: string): boolean => segment !== '');
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

    it('should give every command a category instead of a prefixed title (INV-03)', () => {
      for (const command of manifestCommands) {
        expect(command.category).toBe('%extension.category%');
      }
    });

    it('should not repeat the category inside any translated command title (INV-03)', () => {
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

    it('should give every editor menu item a non-empty when clause (INV-11)', () => {
      const editorMenus: ManifestMenuItem[] = [
        ...manifestMenus['editor/title'],
        ...manifestMenus['editor/context'],
      ];
      for (const menuItem of editorMenus) {
        expect(typeof menuItem.when).toBe('string');
        expect(menuItem.when).not.toBe('');
      }
    });

    it('should give every editor/title command a codicon (INV-05)', () => {
      for (const menuItem of manifestMenus['editor/title']) {
        const command: ManifestCommand | undefined = manifestCommands.find(
          (c: ManifestCommand): boolean => c.command === menuItem.command
        );
        expect(command).toBeDefined();
        expect(command?.icon).toMatch(/^\$\([a-z0-9-]+\)$/);
      }
    });

    it('should never show two competing editor/title buttons at once', () => {
      const titleItems: ManifestMenuItem[] = manifestMenus['editor/title'];
      const showsTranslatedView: ManifestMenuItem[] = titleItems.filter(
        (menuItem: ManifestMenuItem): boolean => menuItem.when!.includes('!babelTcc.translatedView')
      );
      const showsOriginal: ManifestMenuItem[] = titleItems.filter(
        (menuItem: ManifestMenuItem): boolean => !menuItem.when!.includes('!babelTcc.translatedView')
      );
      expect(showsTranslatedView.length).toBe(1);
      expect(showsOriginal.length).toBe(1);
      expect(showsOriginal[0].when).toContain('babelTcc.translatedView');
    });

    it('should not hide any command from the Command Palette (INV-02)', () => {
      const paletteEntries: ManifestMenuItem[] = manifestMenus['commandPalette'] ?? [];
      expect(paletteEntries).toEqual([]);
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
        expect(typeof keybinding.mac).toBe('string');
        expect(keybinding.mac).not.toBe('');
      }
    });

    it('should use the same chord prefix on every binding', () => {
      for (const keybinding of manifestKeybindings) {
        expect(keybinding.key.startsWith('ctrl+alt+b ')).toBe(true);
        expect(keybinding.mac?.startsWith('cmd+alt+b ')).toBe(true);
      }
    });

    it('should never use an ABNT2 AltGr key anywhere in a chord (INV-14)', () => {
      for (const keybinding of manifestKeybindings) {
        for (const segment of splitChord(keybinding.key)) {
          const baseKey: string = segment.split('+').pop() ?? '';
          expect({ command: keybinding.command, baseKey }).toEqual({
            command: keybinding.command,
            baseKey: ABNT2_ALTGR_KEYS.includes(baseKey) ? 'an ABNT2 AltGr key' : baseKey,
          });
        }
      }
    });

    it('should not bind the same chord to two commands', () => {
      const chords: string[] = manifestKeybindings.map((k: ManifestKeybinding): string => k.key);
      expect(new Set(chords).size).toBe(chords.length);
    });
  });
});
