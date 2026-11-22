import * as vscode from 'vscode';
import { isTranslatedScheme } from '../providers/translatedContentProvider';

/**
 * Picks the editor "Show Original Code" acts on. Normally that is the active text editor. When there
 * is none, as when the command runs from a button of the welcome walkthrough, it falls back to the
 * translated view on screen, but only when exactly one is visible: with two, there is no telling
 * which one the user meant.
 * @param activeEditor - `window.activeTextEditor`.
 * @param visibleEditors - `window.visibleTextEditors`.
 * @returns The editor to act on, or undefined when there is none to choose.
 */
export function resolveEditorForShowOriginal(
  activeEditor: vscode.TextEditor | undefined,
  visibleEditors: readonly vscode.TextEditor[]
): vscode.TextEditor | undefined {
  if (activeEditor) {
    return activeEditor;
  }
  const visibleTranslated: vscode.TextEditor[] = visibleEditors.filter(
    (editor: vscode.TextEditor): boolean => isTranslatedScheme(editor.document.uri.scheme)
  );
  if (visibleTranslated.length === 1) {
    return visibleTranslated[0];
  }
  return undefined;
}
