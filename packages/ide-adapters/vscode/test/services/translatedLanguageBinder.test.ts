import { describe, it, expect, beforeEach, vi } from 'vitest';
import { languages } from '../__mocks__/vscode';
import { bindTranslatedLanguage } from '../../src/services/translatedLanguageBinder';

describe('bindTranslatedLanguage', () => {
  let outputChannel: { appendLine: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    vi.clearAllMocks();
    outputChannel = { appendLine: vi.fn() };
  });

  it('should call setTextDocumentLanguage with mlc-{vscodeLangId} for supported file', async () => {
    const doc = { languageId: 'csharp' } as any;
    const detector = { detectLanguage: vi.fn().mockReturnValue('CSharp') } as any;

    await bindTranslatedLanguage(doc, '/test/Program.cs', detector, outputChannel as any);

    expect(languages.setTextDocumentLanguage).toHaveBeenCalledWith(doc, 'mlc-csharp');
  });

  it('should derive different mlc-* IDs for different languages', async () => {
    const detector = {
      detectLanguage: vi.fn()
        .mockReturnValueOnce('Python')
        .mockReturnValueOnce('VisuAlg')
        .mockReturnValueOnce('PortugolStudio'),
    } as any;

    await bindTranslatedLanguage({ languageId: 'python' } as any, '/a.py', detector, outputChannel as any);
    await bindTranslatedLanguage({ languageId: 'visualg' } as any, '/a.alg', detector, outputChannel as any);
    await bindTranslatedLanguage({ languageId: 'portugol-studio' } as any, '/a.por', detector, outputChannel as any);

    expect(languages.setTextDocumentLanguage).toHaveBeenNthCalledWith(1, expect.anything(), 'mlc-python');
    expect(languages.setTextDocumentLanguage).toHaveBeenNthCalledWith(2, expect.anything(), 'mlc-visualg');
    expect(languages.setTextDocumentLanguage).toHaveBeenNthCalledWith(3, expect.anything(), 'mlc-portugol-studio');
  });

  it('should skip when language is not detected', async () => {
    const doc = { languageId: 'plaintext' } as any;
    const detector = { detectLanguage: vi.fn().mockReturnValue(undefined) } as any;

    await bindTranslatedLanguage(doc, '/test/unknown.xyz', detector, outputChannel as any);

    expect(languages.setTextDocumentLanguage).not.toHaveBeenCalled();
  });

  it('should skip when document already has the target mlc-* languageId', async () => {
    const doc = { languageId: 'mlc-csharp' } as any;
    const detector = { detectLanguage: vi.fn().mockReturnValue('CSharp') } as any;

    await bindTranslatedLanguage(doc, '/test/Program.cs', detector, outputChannel as any);

    expect(languages.setTextDocumentLanguage).not.toHaveBeenCalled();
  });

  it('should swallow exceptions from setTextDocumentLanguage and log to output channel', async () => {
    const doc = { languageId: 'csharp' } as any;
    const detector = { detectLanguage: vi.fn().mockReturnValue('CSharp') } as any;
    vi.mocked(languages.setTextDocumentLanguage).mockRejectedValueOnce(new Error('doc closed'));

    await bindTranslatedLanguage(doc, '/test/Program.cs', detector, outputChannel as any);

    expect(outputChannel.appendLine).toHaveBeenCalledWith(expect.stringContaining('failed for /test/Program.cs'));
  });

  it('should swallow exceptions from detectLanguage itself', async () => {
    const doc = { languageId: 'csharp' } as any;
    const detector = {
      detectLanguage: vi.fn().mockImplementation(() => { throw new Error('boom'); }),
    } as any;

    await bindTranslatedLanguage(doc, '/test/Program.cs', detector, outputChannel as any);

    expect(outputChannel.appendLine).toHaveBeenCalledWith(expect.stringContaining('failed for /test/Program.cs'));
  });
});
