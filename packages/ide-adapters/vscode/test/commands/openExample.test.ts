import { describe, it, expect, beforeEach, vi } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import { Uri, window, workspace } from '../__mocks__/vscode';
import {
  buildExampleCatalog,
  buildExampleItems,
  resolveExampleCopy,
  openExample,
  ExampleEntry,
} from '../../src/commands/openExample';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';
import { LanguageDetector } from '../../src/services/languageDetector';

/** The examples folder of the repository, which scripts/copy-examples.js copies into the package. */
const REPOSITORY_EXAMPLES: string = path.join(__dirname, '..', '..', '..', '..', '..', 'examples');

function makeContext(): { extensionUri: Uri; globalStorageUri: Uri; extension: { packageJSON: { version: string } } } {
  return {
    extensionUri: Uri.file('/ext'),
    globalStorageUri: Uri.file('/storage'),
    extension: { packageJSON: { version: '1.1.0' } },
  };
}

describe('openExample', () => {
  let configService: { isEnabled: ReturnType<typeof vi.fn>; isReadonly: ReturnType<typeof vi.fn> };
  let outputChannel: { appendLine: ReturnType<typeof vi.fn>; show: ReturnType<typeof vi.fn> };
  const openedDocument = { uri: 'opened' };

  beforeEach(() => {
    vi.clearAllMocks();
    configService = { isEnabled: vi.fn(() => true), isReadonly: vi.fn(() => false) };
    outputChannel = { appendLine: vi.fn(), show: vi.fn() };
    // No copy yet: stat rejects like the real API does for a missing file.
    vi.mocked(workspace.fs.stat).mockRejectedValue(new Error('FileNotFound'));
    vi.mocked(workspace.fs.createDirectory).mockResolvedValue(undefined);
    vi.mocked(workspace.fs.copy).mockResolvedValue(undefined);
    vi.mocked(workspace.openTextDocument).mockResolvedValue(openedDocument as any);
    vi.mocked(window.showTextDocument).mockResolvedValue({} as any);
  });

  /** Makes the picker answer with the first example of the catalog. */
  function pickFirstExample(): ExampleEntry {
    const firstItem = buildExampleItems(buildExampleCatalog())[0];
    vi.mocked(window.showQuickPick).mockResolvedValue(firstItem as any);
    return firstItem.entry;
  }

  describe('catalog', () => {
    it('should point every example at a file that exists in the repository', () => {
      for (const entry of buildExampleCatalog()) {
        const examplePath: string = path.join(REPOSITORY_EXAMPLES, ...entry.relativePath.split('/'));
        expect({ example: entry.relativePath, exists: fs.existsSync(examplePath) }).toEqual({
          example: entry.relativePath,
          exists: true,
        });
      }
    });

    it('should only offer files of a supported language', () => {
      const languageDetector: LanguageDetector = new LanguageDetector();
      for (const entry of buildExampleCatalog()) {
        expect({ example: entry.relativePath, supported: languageDetector.isSupported(entry.relativePath) }).toEqual({
          example: entry.relativePath,
          supported: true,
        });
      }
    });

    it('should cover every supported file extension at least once', () => {
      const extensions: Set<string> = new Set(buildExampleCatalog().map((entry: ExampleEntry): string => path.extname(entry.relativePath)));
      expect([...extensions].sort()).toEqual(['.alg', '.cs', '.js', '.por', '.py']);
    });

    it('should not repeat an example', () => {
      const paths: string[] = buildExampleCatalog().map((entry: ExampleEntry): string => entry.relativePath);
      expect(new Set(paths).size).toBe(paths.length);
    });
  });

  describe('picker', () => {
    it('should offer one item per example, with label, language and detail in words', () => {
      const catalog: ExampleEntry[] = buildExampleCatalog();
      const items = buildExampleItems(catalog);

      expect(items.length).toBe(catalog.length);
      for (const item of items) {
        expect(item.label).not.toBe('');
        expect(item.label).not.toContain('$(');
        expect(item.description).toBe(item.entry.language);
        expect(item.detail).toBe(item.entry.detail);
      }
    });

    it('should give the picker a title and a placeholder', async () => {
      vi.mocked(window.showQuickPick).mockResolvedValue(undefined);

      await openExample(makeContext() as any, configService as any, outputChannel as any);

      const options = vi.mocked(window.showQuickPick).mock.calls[0][1] as { title: string; placeHolder: string };
      // A missing option is undefined, not '': check for actual text.
      expect(typeof options.title).toBe('string');
      expect(options.title.length).toBeGreaterThan(0);
      expect(typeof options.placeHolder).toBe('string');
      expect(options.placeHolder.length).toBeGreaterThan(0);
    });
  });

  it('should do nothing when the picker is cancelled', async () => {
    vi.mocked(window.showQuickPick).mockResolvedValue(undefined);

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    expect(workspace.fs.copy).not.toHaveBeenCalled();
    expect(workspace.openTextDocument).not.toHaveBeenCalled();
  });

  it('should copy the example to the global storage, under the extension version', async () => {
    const entry: ExampleEntry = pickFirstExample();

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    const [source, target, options] = vi.mocked(workspace.fs.copy).mock.calls[0] as [Uri, Uri, { overwrite: boolean }];
    expect(source.path).toBe(`/ext/examples/${entry.relativePath}`);
    expect(target.path).toBe(`/storage/examples/1.1.0/${entry.relativePath}`);
    expect(options.overwrite).toBe(false);
    const createdFolder = vi.mocked(workspace.fs.createDirectory).mock.calls[0][0] as Uri;
    expect(createdFolder.path).toBe(path.posix.dirname(`/storage/examples/1.1.0/${entry.relativePath}`));
  });

  it('should keep an existing copy, so the edits made to the example survive', async () => {
    pickFirstExample();
    vi.mocked(workspace.fs.stat).mockResolvedValue({ type: 1 } as any);

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    expect(workspace.fs.copy).not.toHaveBeenCalled();
    expect(workspace.openTextDocument).toHaveBeenCalledTimes(1);
  });

  it('should open the editable translated view of the copy, as a permanent tab', async () => {
    const entry: ExampleEntry = pickFirstExample();

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    const openedUri = vi.mocked(workspace.openTextDocument).mock.calls[0][0] as Uri;
    expect(openedUri.scheme).toBe(TRANSLATED_SCHEME);
    expect(openedUri.path).toBe(`/storage/examples/1.1.0/${entry.relativePath}`);
    expect(window.showTextDocument).toHaveBeenCalledWith(openedDocument, { preview: false });
  });

  it('should open the read-only translated view when that is the setting', async () => {
    pickFirstExample();
    configService.isReadonly.mockReturnValue(true);

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    const openedUri = vi.mocked(workspace.openTextDocument).mock.calls[0][0] as Uri;
    expect(openedUri.scheme).toBe(READONLY_SCHEME);
  });

  it('should open the original file when translation is off', async () => {
    pickFirstExample();
    configService.isEnabled.mockReturnValue(false);

    await openExample(makeContext() as any, configService as any, outputChannel as any);

    const openedUri = vi.mocked(workspace.openTextDocument).mock.calls[0][0] as Uri;
    expect(openedUri.scheme).toBe('file');
  });

  describe('when the copy fails', () => {
    beforeEach(() => {
      pickFirstExample();
      vi.mocked(workspace.fs.copy).mockRejectedValue(new Error('disk full'));
    });

    it('should explain the failure with a button to the log, instead of throwing', async () => {
      vi.mocked(window.showErrorMessage).mockResolvedValue(undefined as any);

      await expect(openExample(makeContext() as any, configService as any, outputChannel as any)).resolves.toBeUndefined();

      const [message, button] = vi.mocked(window.showErrorMessage).mock.calls[0] as unknown as [string, string];
      expect(message).toContain('disk full');
      expect(button).toBe('Open Log');
      expect(workspace.openTextDocument).not.toHaveBeenCalled();
    });

    it('should open the log when the user picks the button', async () => {
      vi.mocked(window.showErrorMessage).mockResolvedValue('Open Log' as any);

      await openExample(makeContext() as any, configService as any, outputChannel as any);

      expect(outputChannel.show).toHaveBeenCalledTimes(1);
    });

    it('should not open the log when the message is dismissed', async () => {
      vi.mocked(window.showErrorMessage).mockResolvedValue(undefined as any);

      await openExample(makeContext() as any, configService as any, outputChannel as any);

      expect(outputChannel.show).not.toHaveBeenCalled();
    });
  });

  it('should place each example copy in its own path under the version folder', () => {
    const copy: Uri = resolveExampleCopy(Uri.file('/storage') as any, '2.0.0', 'csharp/HelloWorld/Program.cs') as any;
    expect(copy.path).toBe('/storage/examples/2.0.0/csharp/HelloWorld/Program.cs');
  });

  describe('when VS Code hands out the global storage under its own scheme', () => {
    function makeUserDataContext(): ReturnType<typeof makeContext> {
      return { ...makeContext(), globalStorageUri: Uri.parse('vscode-userdata:/storage') };
    }

    it('should still place the copy under a file URI', () => {
      const copy: Uri = resolveExampleCopy(Uri.parse('vscode-userdata:/storage') as any, '1.1.0', 'visualg/media_notas.alg') as any;
      expect(copy.scheme).toBe('file');
      expect(copy.path).toBe('/storage/examples/1.1.0/visualg/media_notas.alg');
    });

    it('should open the original as a file, so turning translation on can swap it for the translated view', async () => {
      pickFirstExample();
      configService.isEnabled.mockReturnValue(false);

      await openExample(makeUserDataContext() as any, configService as any, outputChannel as any);

      const openedUri = vi.mocked(workspace.openTextDocument).mock.calls[0][0] as Uri;
      expect(openedUri.scheme).toBe('file');
    });
  });
});
