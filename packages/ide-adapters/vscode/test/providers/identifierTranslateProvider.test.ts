import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  IdentifierTranslateProvider,
  extractContext,
  buildMarker,
  suggestTranslation,
} from '../../src/providers/identifierTranslateProvider';
import { Position, Range, Uri, window, workspace, WorkspaceEdit } from '../__mocks__/vscode';
import type { BabelServicesClient } from '../../src/services/babelServicesClient';

/** Minimal fake TextDocument for provider tests. */
function fakeDocument(lines: string[], wordRange?: Range): Record<string, unknown> {
  return {
    uri: Uri.file('/x/Sample.cs'),
    lineCount: lines.length,
    lineAt: (n: number) => ({
      text: lines[n],
      range: new Range(new Position(n, 0), new Position(n, lines[n].length)),
    }),
    getWordRangeAtPosition: (_pos: Position): Range | undefined => wordRange,
    getText: (r: Range): string => lines[r.start.line].slice(r.start.character, r.end.character),
  };
}

describe('IdentifierTranslateProvider', () => {
  const provider = new IdentifierTranslateProvider();

  it('offers a code action on an identifier', () => {
    const range = new Range(new Position(0, 13), new Position(0, 23));
    const doc = fakeDocument(['public class Calculator { }'], range);
    const actions = provider.provideCodeActions(
      doc as never,
      new Range(new Position(0, 13), new Position(0, 13)) as never
    );
    expect(actions).toHaveLength(1);
    expect(actions[0].command?.command).toBe('babel-tcc.suggestTranslation');
    expect(actions[0].command?.arguments?.[2]).toBe('Calculator');
  });

  it('does not offer on a line already marked with // tradu', () => {
    const range = new Range(new Position(0, 13), new Position(0, 23));
    const doc = fakeDocument(['public class Calculator { } // tradu[pt-br]:Calculadora'], range);
    const actions = provider.provideCodeActions(
      doc as never,
      new Range(new Position(0, 13), new Position(0, 13)) as never
    );
    expect(actions).toHaveLength(0);
  });

  it('does not offer when there is no identifier at the position', () => {
    const doc = fakeDocument(['    { }'], undefined);
    const actions = provider.provideCodeActions(
      doc as never,
      new Range(new Position(0, 0), new Position(0, 0)) as never
    );
    expect(actions).toHaveLength(0);
  });
});

describe('helpers', () => {
  it('buildMarker produces the tradu comment', () => {
    expect(buildMarker('pt-br', 'Calculadora')).toBe(' // tradu[pt-br]:Calculadora');
  });

  it('extractContext returns a window of lines around the target', () => {
    const doc = fakeDocument(['a', 'b', 'c', 'd', 'e']);
    expect(extractContext(doc as never, 2)).toBe('a\nb\nc\nd\ne');
  });
});

describe('suggestTranslation', () => {
  const uri = Uri.file('/x/Sample.cs');
  const wordRange = new Range(new Position(0, 13), new Position(0, 23));

  beforeEach(() => {
    vi.clearAllMocks();
    (workspace.openTextDocument as unknown as ReturnType<typeof vi.fn>).mockResolvedValue(
      fakeDocument(['public class Calculator { }'], wordRange)
    );
    (workspace.applyEdit as unknown as ReturnType<typeof vi.fn>).mockResolvedValue(true);
  });

  it('inserts a tradu marker when the user accepts the suggestion', async () => {
    const client = { translateIdentifier: vi.fn().mockResolvedValue('Calculadora') } as unknown as BabelServicesClient;
    (window.showQuickPick as unknown as ReturnType<typeof vi.fn>).mockResolvedValue('Calculadora');

    await suggestTranslation(client, 'pt-br', uri as never, wordRange as never, 'Calculator');

    expect(client.translateIdentifier).toHaveBeenCalledWith('Calculator', expect.any(String), 'pt-br');
    expect(workspace.applyEdit).toHaveBeenCalledTimes(1);
    // The WorkspaceEdit passed to applyEdit had its insert called with the tradu marker.
    const passedEdit = (workspace.applyEdit as unknown as ReturnType<typeof vi.fn>).mock.calls[0][0] as WorkspaceEdit;
    expect(passedEdit.insert).toHaveBeenCalledWith(uri, expect.anything(), ' // tradu[pt-br]:Calculadora');
  });

  it('shows an info message and does not edit when no translation is available', async () => {
    const client = { translateIdentifier: vi.fn().mockResolvedValue(null) } as unknown as BabelServicesClient;

    await suggestTranslation(client, 'pt-br', uri as never, wordRange as never, 'Calculator');

    expect(window.showInformationMessage).toHaveBeenCalled();
    expect(workspace.applyEdit).not.toHaveBeenCalled();
  });

  it('does not edit when the user dismisses the QuickPick', async () => {
    const client = { translateIdentifier: vi.fn().mockResolvedValue('Calculadora') } as unknown as BabelServicesClient;
    (window.showQuickPick as unknown as ReturnType<typeof vi.fn>).mockResolvedValue(undefined);

    await suggestTranslation(client, 'pt-br', uri as never, wordRange as never, 'Calculator');

    expect(workspace.applyEdit).not.toHaveBeenCalled();
  });
});
