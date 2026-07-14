import * as vscode from 'vscode';
import { COMMANDS, TRANSLATION_MARKER_PREFIX } from '../config/constants';
import { BabelServicesClient } from '../services/babelServicesClient';

/** Regex matching a C-family identifier. */
const IDENTIFIER_RE = /[A-Za-z_][A-Za-z0-9_]*/;

/** Lines of surrounding code sent as disambiguation context. */
const CONTEXT_WINDOW_LINES = 8;

/**
 * Offers a "Suggest translation" code action (Ctrl+.) on the identifier under the cursor. The
 * action is suppressed on lines that already carry a `// tradu` marker, so a translated line is
 * not re-offered. Selecting it runs {@link COMMANDS.SUGGEST_TRANSLATION}.
 */
export class IdentifierTranslateProvider implements vscode.CodeActionProvider {
  public static readonly providedKinds: vscode.CodeActionKind[] = [vscode.CodeActionKind.RefactorRewrite];

  public provideCodeActions(
    document: vscode.TextDocument,
    range: vscode.Range | vscode.Selection
  ): vscode.CodeAction[] {
    const position: vscode.Position = range.start;
    const lineText: string = document.lineAt(position.line).text;
    if (lineText.includes(TRANSLATION_MARKER_PREFIX)) {
      return []; // already translated
    }

    const wordRange: vscode.Range | undefined = document.getWordRangeAtPosition(position, IDENTIFIER_RE);
    if (!wordRange) {
      return [];
    }
    const identifier: string = document.getText(wordRange);
    if (!identifier) {
      return [];
    }

    const action: vscode.CodeAction = new vscode.CodeAction(
      vscode.l10n.t('Babel: Suggest translation for "{0}"', identifier),
      vscode.CodeActionKind.RefactorRewrite
    );
    action.command = {
      command: COMMANDS.SUGGEST_TRANSLATION,
      title: vscode.l10n.t('Babel: Suggest translation'),
      arguments: [document.uri, wordRange, identifier],
    };
    return [action];
  }
}

/** Extracts a window of surrounding lines as disambiguation context for the LLM. */
export function extractContext(document: vscode.TextDocument, line: number): string {
  const start: number = Math.max(0, line - CONTEXT_WINDOW_LINES);
  const end: number = Math.min(document.lineCount - 1, line + CONTEXT_WINDOW_LINES);
  const lines: string[] = [];
  for (let i = start; i <= end; i++) {
    lines.push(document.lineAt(i).text);
  }
  return lines.join('\n');
}

/** Builds the marker comment appended to a translated line, e.g. ` // tradu[pt-br]:Calculadora`. */
export function buildMarker(targetLanguage: string, translation: string): string {
  return ` ${TRANSLATION_MARKER_PREFIX}[${targetLanguage}]:${translation}`;
}

/**
 * Command handler for {@link COMMANDS.SUGGEST_TRANSLATION}. Resolves a translation (hosted or
 * local via {@link BabelServicesClient}), shows it in a QuickPick, and on acceptance appends a
 * `// tradu[<lang>]:<translation>` marker to the identifier's line. Fully graceful: a `null`
 * result (backend + local both unavailable) shows an informational message, never an error dialog.
 */
export async function suggestTranslation(
  client: BabelServicesClient,
  targetLanguage: string,
  uri: vscode.Uri,
  wordRange: vscode.Range,
  identifier: string
): Promise<void> {
  const document: vscode.TextDocument = await vscode.workspace.openTextDocument(uri);
  const context: string = extractContext(document, wordRange.start.line);

  const suggestion: string | null = await vscode.window.withProgress(
    { location: vscode.ProgressLocation.Notification, title: vscode.l10n.t('Babel: Translating "{0}"…', identifier) },
    (): Promise<string | null> => client.translateIdentifier(identifier, context, targetLanguage)
  );

  if (suggestion === null) {
    vscode.window.showInformationMessage(
      vscode.l10n.t('Babel: No translation available (backend and local model unreachable).')
    );
    return;
  }

  const picked: string | undefined = await vscode.window.showQuickPick(
    [suggestion],
    { placeHolder: vscode.l10n.t('Translation for "{0}" ({1}) — press Enter to insert', identifier, targetLanguage) }
  );
  if (picked === undefined) {
    return; // user dismissed
  }

  const edit: vscode.WorkspaceEdit = new vscode.WorkspaceEdit();
  const line: vscode.TextLine = document.lineAt(wordRange.start.line);
  edit.insert(uri, line.range.end, buildMarker(targetLanguage, picked));
  await vscode.workspace.applyEdit(edit);
}
