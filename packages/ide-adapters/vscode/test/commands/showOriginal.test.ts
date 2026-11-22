import { describe, it, expect } from 'vitest';
import { Uri } from '../__mocks__/vscode';
import { resolveEditorForShowOriginal } from '../../src/commands/showOriginal';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../../src/providers/translatedContentProvider';

function editorOn(uri: Uri, viewColumn: number): any {
  return { document: { uri }, viewColumn };
}

describe('resolveEditorForShowOriginal', () => {
  const translated = editorOn(Uri.parse(`${TRANSLATED_SCHEME}:/ex/Program.cs`), 2);
  const readonly = editorOn(Uri.parse(`${READONLY_SCHEME}:/ex/Calc.cs`), 3);
  const original = editorOn(Uri.file('/ex/Other.cs'), 1);

  it('should use the active editor when there is one', () => {
    expect(resolveEditorForShowOriginal(original, [translated])).toBe(original);
  });

  it('should fall back to the only translated view on screen when no editor is active', () => {
    expect(resolveEditorForShowOriginal(undefined, [original, translated])).toBe(translated);
  });

  it('should also find a read-only translated view', () => {
    expect(resolveEditorForShowOriginal(undefined, [readonly])).toBe(readonly);
  });

  it('should not guess between two translated views', () => {
    expect(resolveEditorForShowOriginal(undefined, [translated, readonly])).toBeUndefined();
  });

  it('should find nothing when no translated view is on screen', () => {
    expect(resolveEditorForShowOriginal(undefined, [original])).toBeUndefined();
  });
});
