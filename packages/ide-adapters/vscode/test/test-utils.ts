import { vi } from 'vitest';
import { Uri, Position, Range } from './__mocks__/vscode';

// eslint-disable-next-line @typescript-eslint/explicit-function-return-type
export function makeDocument(scheme: string, word: string | null, filePath: string = '/test/file.cs') {
  return {
    uri: Uri.parse(`${scheme}:${filePath}`),
    getWordRangeAtPosition: vi.fn(() =>
      word ? new Range(new Position(0, 0), new Position(0, word.length)) : undefined
    ),
    getText: vi.fn(() => word ?? ''),
  };
}

export function makeContext(extensionPath: string = '/ext'): { extensionPath: string; subscriptions: { dispose(): void }[] } {
  return {
    extensionPath,
    subscriptions: [] as { dispose(): void }[],
  };
}

export function makeOutputChannel(): { appendLine: ReturnType<typeof vi.fn>; show: ReturnType<typeof vi.fn>; dispose: ReturnType<typeof vi.fn> } {
  return { appendLine: vi.fn(), show: vi.fn(), dispose: vi.fn() };
}
