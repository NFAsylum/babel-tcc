import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { workspace, ConfigurationTarget } from '../__mocks__/vscode';
import { ConfigurationService } from '../../src/services/configurationService';

describe('ConfigurationService', () => {
  let service: ConfigurationService;
  let configChangeCallback: (event: { affectsConfiguration: (s: string) => boolean }) => void;
  let updateMock: ReturnType<typeof vi.fn>;
  const configStore: Record<string, unknown> = {};

  beforeEach(() => {
    vi.clearAllMocks();

    // Clear store
    for (const key of Object.keys(configStore)) delete configStore[key];

    updateMock = vi.fn();
    vi.mocked(workspace.getConfiguration).mockImplementation((_section?: string) => ({
      get: vi.fn((key: string, defaultValue?: unknown) => {
        return key in configStore ? configStore[key] : defaultValue;
      }),
      update: updateMock,
    }) as any);

    vi.mocked(workspace.onDidChangeConfiguration).mockImplementation((cb: any) => {
      configChangeCallback = cb;
      return { dispose: vi.fn() };
    });

    service = new ConfigurationService();
  });

  afterEach(() => {
    service.dispose();
  });

  describe('isEnabled', () => {
    it('should return true by default', () => {
      expect(service.isEnabled()).toBe(true);
    });

    it('should return false when configured as false', () => {
      configStore['enabled'] = false;
      expect(service.isEnabled()).toBe(false);
    });
  });

  describe('getLanguage', () => {
    it('should return pt-br by default', () => {
      expect(service.getLanguage()).toBe('pt-br');
    });

    it('should return configured value', () => {
      configStore['language'] = 'es-es';
      expect(service.getLanguage()).toBe('es-es');
    });
  });

  describe('isReadonly', () => {
    it('should return false by default', () => {
      expect(service.isReadonly()).toBe(false);
    });

    it('should return true when configured', () => {
      configStore['readonly'] = true;
      expect(service.isReadonly()).toBe(true);
    });
  });

  describe('setEnabled', () => {
    it('should call config.update with correct params', async () => {
      await service.setEnabled(false);
      expect(updateMock).toHaveBeenCalledWith('enabled', false, ConfigurationTarget.Global);
    });
  });

  describe('setLanguage', () => {
    it('should call config.update with correct params', async () => {
      await service.setLanguage('fr-fr');
      expect(updateMock).toHaveBeenCalledWith('language', 'fr-fr', ConfigurationTarget.Global);
    });
  });

  describe('setReadonly', () => {
    it('should call config.update with correct params', async () => {
      await service.setReadonly(true);
      expect(updateMock).toHaveBeenCalledWith('readonly', true, ConfigurationTarget.Global);
    });
  });

  describe('getLanguageForProgrammingLanguage', () => {
    it('should return global language when no override is set', () => {
      configStore['language'] = 'pt-br';
      expect(service.getLanguageForProgrammingLanguage('CSharp')).toBe('pt-br');
    });

    it('should return override when set for programming language', () => {
      configStore['language'] = 'pt-br';
      configStore['languageOverrides'] = { CSharp: 'es-es' };
      expect(service.getLanguageForProgrammingLanguage('CSharp')).toBe('es-es');
    });

    it('should return global language for programming language without override', () => {
      configStore['language'] = 'pt-br';
      configStore['languageOverrides'] = { CSharp: 'es-es' };
      expect(service.getLanguageForProgrammingLanguage('Python')).toBe('pt-br');
    });

    it('should support different overrides for different programming languages', () => {
      configStore['language'] = 'pt-br';
      configStore['languageOverrides'] = { CSharp: 'es-es', Python: 'fr-fr' };
      expect(service.getLanguageForProgrammingLanguage('CSharp')).toBe('es-es');
      expect(service.getLanguageForProgrammingLanguage('Python')).toBe('fr-fr');
    });

    it('should match keys case-insensitively', () => {
      configStore['languageOverrides'] = { csharp: 'es-es' };
      expect(service.getLanguageForProgrammingLanguage('CSharp')).toBe('es-es');
    });
  });

  describe('setLanguageOverride', () => {
    it('should call config.update with override added', async () => {
      configStore['languageOverrides'] = {};
      await service.setLanguageOverride('CSharp', 'es-es');
      expect(updateMock).toHaveBeenCalledWith(
        'languageOverrides',
        { CSharp: 'es-es' },
        ConfigurationTarget.Global
      );
    });

    it('should remove override when language is undefined', async () => {
      configStore['languageOverrides'] = { CSharp: 'es-es', Python: 'fr-fr' };
      await service.setLanguageOverride('CSharp', undefined);
      expect(updateMock).toHaveBeenCalledWith(
        'languageOverrides',
        { Python: 'fr-fr' },
        ConfigurationTarget.Global
      );
    });
  });

  describe('onDidChangeConfiguration', () => {
    it('should fire event when babel-tcc config changes', () => {
      const listener = vi.fn();
      service.onDidChangeConfiguration(listener);
      configChangeCallback({ affectsConfiguration: (s: string) => s === 'babel-tcc' });
      expect(listener).toHaveBeenCalledOnce();
    });

    it('should not fire event for unrelated config changes', () => {
      const listener = vi.fn();
      service.onDidChangeConfiguration(listener);
      configChangeCallback({ affectsConfiguration: (s: string) => s === 'editor.fontSize' });
      expect(listener).not.toHaveBeenCalled();
    });
  });

  describe('dispose', () => {
    it('should dispose without error', () => {
      expect(() => service.dispose()).not.toThrow();
    });
  });
});
