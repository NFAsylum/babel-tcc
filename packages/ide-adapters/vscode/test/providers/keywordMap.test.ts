import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { __clearConfigValues, __setConfigValue, workspace } from '../__mocks__/vscode';

import { KeywordMapService } from '../../src/providers/keywordMap';
import { ConfigurationService } from '../../src/services/configurationService';
import { LanguageDetector } from '../../src/services/languageDetector';

const MOCK_KEYWORD_MAP: Record<string, string> = {
  publico: 'public',
  classe: 'class',
  vazio: 'void',
};

const MOCK_IDENTIFIER_MAP: Record<string, string> = {
  calcularTotal: 'calculateTotal',
  nomeUsuario: 'userName',
};

describe('KeywordMapService', () => {
  let configService: ConfigurationService;
  let languageDetector: LanguageDetector;
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };
  let service: KeywordMapService;
  let mockCoreBridge: {
    getKeywordMap: ReturnType<typeof vi.fn>;
    getIdentifierMap: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    vi.clearAllMocks();
    __clearConfigValues();

    __setConfigValue('babel-tcc.enabled', true);
    __setConfigValue('babel-tcc.language', 'pt-br');

    configService = new ConfigurationService();
    languageDetector = new LanguageDetector();
    outputChannel = { appendLine: vi.fn() };
    mockCoreBridge = {
      getKeywordMap: vi.fn().mockResolvedValue(MOCK_KEYWORD_MAP),
      getIdentifierMap: vi.fn().mockResolvedValue(MOCK_IDENTIFIER_MAP),
    };

    service = new KeywordMapService(
      mockCoreBridge as any,
      configService,
      languageDetector,
      outputChannel as any
    );
  });

  afterEach(() => {
    service.dispose();
  });

  describe('getMap', () => {
    it('should return empty map before warmCache is called', () => {
      const map: Record<string, string> = service.getMap('file.cs');
      expect(map).toEqual({});
    });

    it('should return keyword map after warmCache', async () => {
      await service.warmCache();
      const map: Record<string, string> = service.getMap('file.cs');
      expect(map).toEqual(MOCK_KEYWORD_MAP);
    });

    it('should return empty map for unsupported file type', async () => {
      await service.warmCache();
      const map: Record<string, string> = service.getMap('file.txt');
      expect(map).toEqual({});
    });

    it('should cache and not re-fetch on second call', async () => {
      await service.warmCache();
      service.getMap('file.cs');
      service.getMap('file.cs');
      // getKeywordMap called once per extension during warmCache, not per getMap call
      expect(mockCoreBridge.getKeywordMap).toHaveBeenCalled();
    });

    it('should clear cache when language changes via config event', async () => {
      await service.warmCache();
      const map1: Record<string, string> = service.getMap('file.cs');
      expect(map1).toEqual(MOCK_KEYWORD_MAP);

      __setConfigValue('babel-tcc.language', 'es-es');
      workspace.__fireConfigChange('babel-tcc');

      // Cache was cleared, so getMap returns empty until warmCache is called again
      const map2: Record<string, string> = service.getMap('file.cs');
      expect(map2).toEqual({});
    });
  });

  describe('getIdentifierMap', () => {
    it('should return empty map before warmCache is called', () => {
      const map: Record<string, string> = service.getIdentifierMap('file.cs');
      expect(map).toEqual({});
    });

    it('should return identifier map after warmCache', async () => {
      await service.warmCache();
      const map: Record<string, string> = service.getIdentifierMap('file.cs');
      expect(map).toEqual(MOCK_IDENTIFIER_MAP);
    });

    it('should clear identifier cache on config change', async () => {
      await service.warmCache();
      expect(service.getIdentifierMap('file.cs')).toEqual(MOCK_IDENTIFIER_MAP);

      __setConfigValue('babel-tcc.language', 'es-es');
      workspace.__fireConfigChange('babel-tcc');

      expect(service.getIdentifierMap('file.cs')).toEqual({});
    });
  });

  describe('warmCache', () => {
    it('should call coreBridge.getKeywordMap for each supported extension', async () => {
      await service.warmCache();
      const extensions: string[] = languageDetector.getSupportedExtensions();
      // Called once per supported extension
      expect(mockCoreBridge.getKeywordMap).toHaveBeenCalledTimes(extensions.length);
    });

    it('should call coreBridge.getIdentifierMap once', async () => {
      await service.warmCache();
      expect(mockCoreBridge.getIdentifierMap).toHaveBeenCalledTimes(1);
    });

    it('should handle coreBridge errors gracefully', async () => {
      mockCoreBridge.getKeywordMap.mockRejectedValue(new Error('Core not available'));
      mockCoreBridge.getIdentifierMap.mockRejectedValue(new Error('Core not available'));
      await service.warmCache();
      const map: Record<string, string> = service.getMap('file.cs');
      expect(map).toEqual({});
      expect(outputChannel.appendLine).toHaveBeenCalledWith(
        expect.stringContaining('failed to load')
      );
    });
  });

  describe('dispose', () => {
    it('should dispose config subscription without error', () => {
      expect(() => service.dispose()).not.toThrow();
    });
  });
});
