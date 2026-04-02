import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import * as fs from 'fs';
import { window, __clearConfigValues, __setConfigValue, workspace } from '../__mocks__/vscode';

vi.mock('fs');

import { KeywordMapService } from '../../src/providers/keywordMap';
import { ConfigurationService } from '../../src/services/configurationService';
import { LanguageDetector } from '../../src/services/languageDetector';

const KEYWORDS_BASE = JSON.stringify({
  keywords: { public: 1, class: 2, void: 3 },
});

const TRANSLATION_PT_BR = JSON.stringify({
  translations: { '1': 'publico', '2': 'classe', '3': 'vazio' },
});

describe('KeywordMapService', () => {
  let configService: ConfigurationService;
  let languageDetector: LanguageDetector;
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };
  let service: KeywordMapService;

  beforeEach(() => {
    vi.clearAllMocks();
    __clearConfigValues();

    __setConfigValue('babel-tcc.enabled', true);
    __setConfigValue('babel-tcc.language', 'pt-br');

    configService = new ConfigurationService();
    languageDetector = new LanguageDetector();
    outputChannel = { appendLine: vi.fn() };

    vi.mocked(fs.existsSync).mockReturnValue(true);
    vi.mocked(fs.readFileSync).mockImplementation((filePath: fs.PathOrFileDescriptor) => {
      const p = String(filePath);
      if (p.includes('keywords-base.json')) return KEYWORDS_BASE;
      if (p.includes('csharp.json')) return TRANSLATION_PT_BR;
      return '';
    });

    service = new KeywordMapService(
      '/translations',
      configService,
      languageDetector,
      outputChannel as any
    );
  });

  afterEach(() => {
    service.dispose();
  });

  describe('getMap', () => {
    it('should return inverted map (translated → original) for valid files', () => {
      const map = service.getMap('file.cs');
      expect(map).toEqual({
        publico: 'public',
        classe: 'class',
        vazio: 'void',
      });
    });

    it('should return empty map for unsupported language', () => {
      const map = service.getMap('file.py');
      expect(map).toEqual({});
    });

    it('should return empty map when keywords-base.json is missing', () => {
      vi.mocked(fs.existsSync).mockReturnValue(false);
      const map = service.getMap('file.cs');
      expect(map).toEqual({});
    });

    it('should return empty map and show warning on parse error', () => {
      vi.mocked(fs.readFileSync).mockReturnValue('invalid json{{{');
      const map = service.getMap('file.cs');
      expect(map).toEqual({});
      expect(window.showWarningMessage).toHaveBeenCalled();
    });

    it('should cache result for same language', () => {
      service.getMap('file.cs');
      service.getMap('file.cs');
      expect(fs.readFileSync).toHaveBeenCalledTimes(2); // base + translation only once
    });

    it('should reload when language changes via config event', () => {
      service.getMap('file.cs');
      const readCount = vi.mocked(fs.readFileSync).mock.calls.length;

      __setConfigValue('babel-tcc.language', 'es-es');
      workspace.__fireConfigChange('babel-tcc');

      service.getMap('file.cs');
      expect(vi.mocked(fs.readFileSync).mock.calls.length).toBeGreaterThan(readCount);
    });
  });

  describe('dispose', () => {
    it('should dispose config subscription without error', () => {
      expect(() => service.dispose()).not.toThrow();
    });
  });
});
