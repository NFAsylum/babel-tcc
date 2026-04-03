import { describe, it, expect } from 'vitest';
import { SUPPORTED_LANGUAGES, buildExtensionMap, buildVSCodeLanguageMap, buildFileWatcherPattern } from '../../src/config/languages';
import * as fs from 'fs';
import * as path from 'path';

describe('languages config', () => {
  const packageJsonPath: string = path.join(__dirname, '../../package.json');
  const packageJson: Record<string, unknown> = JSON.parse(fs.readFileSync(packageJsonPath, 'utf-8'));

  describe('SUPPORTED_LANGUAGES', () => {
    it('should have at least CSharp and Python', () => {
      const names: string[] = SUPPORTED_LANGUAGES.map((l) => l.name);
      expect(names).toContain('CSharp');
      expect(names).toContain('Python');
    });
  });

  describe('buildExtensionMap', () => {
    it('should map .cs to CSharp and .py to Python', () => {
      const map: Record<string, string> = buildExtensionMap();
      expect(map['.cs']).toBe('CSharp');
      expect(map['.py']).toBe('Python');
    });
  });

  describe('buildVSCodeLanguageMap', () => {
    it('should map CSharp to csharp and Python to python', () => {
      const map: Record<string, string> = buildVSCodeLanguageMap();
      expect(map['CSharp']).toBe('csharp');
      expect(map['Python']).toBe('python');
    });
  });

  describe('buildFileWatcherPattern', () => {
    it('should include all extensions', () => {
      const pattern: string = buildFileWatcherPattern();
      expect(pattern).toContain('cs');
      expect(pattern).toContain('py');
      expect(pattern).toMatch(/^\*\*\/\*\.\{.*\}$/);
    });
  });

  describe('consistency with package.json', () => {
    it('should have activationEvent for each language', () => {
      const activationEvents: string[] = (packageJson as Record<string, string[]>)['activationEvents'] || [];
      for (const lang of SUPPORTED_LANGUAGES) {
        const expectedEvent: string = `onLanguage:${lang.vscodeLangId}`;
        expect(activationEvents).toContain(expectedEvent);
      }
    });

    it('should have mlc-language registered for each language', () => {
      const contributes: Record<string, unknown[]> = (packageJson as Record<string, Record<string, unknown[]>>)['contributes'];
      const languages: Record<string, string>[] = (contributes['languages'] || []) as Record<string, string>[];
      const registeredIds: string[] = languages.map((l) => l['id']);
      for (const lang of SUPPORTED_LANGUAGES) {
        expect(registeredIds).toContain(`mlc-${lang.vscodeLangId}`);
      }
    });

    it('should have grammar for each language', () => {
      const contributes: Record<string, unknown[]> = (packageJson as Record<string, Record<string, unknown[]>>)['contributes'];
      const grammars: Record<string, string>[] = (contributes['grammars'] || []) as Record<string, string>[];
      const grammarLanguages: string[] = grammars.map((g) => g['language']);
      for (const lang of SUPPORTED_LANGUAGES) {
        expect(grammarLanguages).toContain(`mlc-${lang.vscodeLangId}`);
      }
    });

    it('should have tmLanguage.json file for each language', () => {
      const syntaxesDir: string = path.join(__dirname, '../../syntaxes');
      for (const lang of SUPPORTED_LANGUAGES) {
        const grammarFile: string = path.join(syntaxesDir, `mlc-${lang.vscodeLangId}.tmLanguage.json`);
        expect(fs.existsSync(grammarFile)).toBe(true);
      }
    });
  });
});
