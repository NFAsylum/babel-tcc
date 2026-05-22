import * as vscode from 'vscode';
import { getMlcLanguageId } from '../config/languages';
import { LanguageDetector } from './languageDetector';

/**
 * Assigns the babel-tcc-exclusive mlc-* language ID to a translated document so its
 * tmLanguage and language-configuration come from this extension instead of being
 * inherited from whichever extension wins the original file extension association
 * (e.g. .alg could be claimed by other Portugol extensions).
 *
 * No-op when the file extension is not recognized or when setTextDocumentLanguage fails
 * (e.g. document already closed). Errors are written to outputChannel so the user can
 * inspect issues without the translation flow itself failing.
 */
export async function bindTranslatedLanguage(
  doc: vscode.TextDocument,
  originalPath: string,
  languageDetector: LanguageDetector,
  outputChannel: vscode.OutputChannel
): Promise<void> {
  try {
    const programmingLanguage: string | undefined = languageDetector.detectLanguage(originalPath);
    if (programmingLanguage === undefined) {
      return;
    }

    const mlcLangId: string = getMlcLanguageId(programmingLanguage);
    if (mlcLangId.length === 0) {
      return;
    }

    if (doc.languageId === mlcLangId) {
      return;
    }

    await vscode.languages.setTextDocumentLanguage(doc, mlcLangId);
  } catch (error: unknown) {
    const message: string = error instanceof Error ? error.message : String(error);
    outputChannel.appendLine(`bindTranslatedLanguage: failed for ${originalPath} - ${message}`);
  }
}
