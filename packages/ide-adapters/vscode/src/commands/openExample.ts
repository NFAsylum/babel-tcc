import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { TRANSLATED_SCHEME, READONLY_SCHEME } from '../providers/translatedContentProvider';
import { describeError } from '../services/errorMessage';

/**
 * Folder that holds the examples, both inside the extension (filled at packaging time by
 * scripts/copy-examples.js) and inside the extension's global storage (where the user's copy lives).
 */
export const EXAMPLES_FOLDER = 'examples';

/** An example the user can open: a file under the examples folder, described for the picker. */
export interface ExampleEntry {
  /** Path of the file relative to the examples folder, with forward slashes. */
  relativePath: string;
  /** Programming language, shown next to the label. A product name, identical in every locale. */
  language: string;
  /** Short, translated name of the example. */
  label: string;
  /** Translated sentence saying what the example demonstrates. */
  detail: string;
}

/** Picker item that carries the example it stands for. */
export interface ExampleQuickPickItem extends vscode.QuickPickItem {
  entry: ExampleEntry;
}

/**
 * The examples offered by "Open Example", in teaching order: the smallest program first, then one
 * example per supported language. A typed catalog instead of a listing of the folder, so the labels
 * can be translated and the order stays deliberate.
 * @returns The catalog, with labels and details in the current UI language.
 */
export function buildExampleCatalog(): ExampleEntry[] {
  return [
    {
      relativePath: 'csharp/HelloWorld/Program.cs',
      language: 'C#',
      label: vscode.l10n.t('Hello, World'),
      detail: vscode.l10n.t('The first program: prints a greeting. Class, method and variable names are translated with // tradu.'),
    },
    {
      relativePath: 'csharp/Calculator/Calculator.cs',
      language: 'C#',
      label: vscode.l10n.t('Calculator'),
      detail: vscode.l10n.t('A class with the four operations; methods and parameters translated with // tradu.'),
    },
    {
      relativePath: 'python/shapes.py',
      language: 'Python',
      label: vscode.l10n.t('Geometric shapes'),
      detail: vscode.l10n.t('Classes and inheritance computing area and perimeter; names translated with # tradu.'),
    },
    {
      relativePath: 'javascript/calculadora.js',
      language: 'JavaScript',
      label: vscode.l10n.t('Calculator'),
      detail: vscode.l10n.t('Functions, constants and loops; comments and strings stay untouched.'),
    },
    {
      relativePath: 'visualg/media_notas.alg',
      language: 'VisuAlg',
      label: vscode.l10n.t('Grade average'),
      detail: vscode.l10n.t('Input, conditions and loops, in the style of introductory logic classes.'),
    },
    {
      relativePath: 'portugol-studio/calculadora.por',
      language: 'Portugol Studio',
      label: vscode.l10n.t('Calculator'),
      detail: vscode.l10n.t('Functions, types and a choose-case menu.'),
    },
  ];
}

/**
 * Turns the catalog into picker items. The label is text only: a codicon would not be read by a
 * screen reader, so it would carry no information.
 * @param catalog - The examples to offer.
 * @returns One picker item per example.
 */
export function buildExampleItems(catalog: ExampleEntry[]): ExampleQuickPickItem[] {
  return catalog.map((entry: ExampleEntry): ExampleQuickPickItem => ({
    label: entry.label,
    description: entry.language,
    detail: entry.detail,
    entry,
  }));
}

/**
 * Where the user's copy of an example lives: the extension's global storage, under the extension
 * version. The copy is writable (the files inside the extension are not), it does not depend on any
 * folder being open, and a new version of the extension starts from fresh copies.
 *
 * The result is always a `file:` URI. VS Code may hand out the global storage under its own
 * `vscode-userdata:` scheme, and the rest of the extension recognizes an original file only by the
 * `file:` scheme: a copy opened under the other scheme was not swapped for its translated view when
 * translation was turned on.
 * @param globalStorageUri - The extension's global storage folder.
 * @param extensionVersion - The running extension version.
 * @param relativePath - The example path relative to the examples folder.
 * @returns The `file:` URI of the user's copy.
 */
export function resolveExampleCopy(
  globalStorageUri: vscode.Uri,
  extensionVersion: string,
  relativePath: string
): vscode.Uri {
  const storedCopy: vscode.Uri = vscode.Uri.joinPath(
    globalStorageUri,
    EXAMPLES_FOLDER,
    extensionVersion,
    ...relativePath.split('/')
  );
  return vscode.Uri.file(storedCopy.fsPath);
}

/**
 * Checks whether a file exists. `workspace.fs.stat` reports a missing file by rejecting, so the
 * rejection is caught here, at the file system boundary, and turned into a boolean.
 * @param fileUri - The file to look for.
 * @returns True when the file exists.
 */
export async function fileExists(fileUri: vscode.Uri): Promise<boolean> {
  try {
    await vscode.workspace.fs.stat(fileUri);
    return true;
  } catch {
    return false;
  }
}

/**
 * Makes sure the user's copy of an example exists. An existing copy is left as it is, so edits the
 * user made to the example survive opening it again.
 * @param sourceUri - The example shipped with the extension.
 * @param copyUri - Where the user's copy lives.
 */
export async function materializeExample(sourceUri: vscode.Uri, copyUri: vscode.Uri): Promise<void> {
  if (await fileExists(copyUri)) {
    return;
  }
  await vscode.workspace.fs.createDirectory(vscode.Uri.joinPath(copyUri, '..'));
  await vscode.workspace.fs.copy(sourceUri, copyUri, { overwrite: false });
}

/**
 * Decides which view of the copy to open: the translated view when translation is on (read-only or
 * editable, following the setting), and the original file when it is off.
 * @param copyUri - The user's copy of the example.
 * @param configService - Source of the enabled and read-only settings.
 * @returns The URI to open.
 */
export function buildExampleViewUri(copyUri: vscode.Uri, configService: ConfigurationService): vscode.Uri {
  if (!configService.isEnabled()) {
    return copyUri;
  }
  let scheme: string = TRANSLATED_SCHEME;
  if (configService.isReadonly()) {
    scheme = READONLY_SCHEME;
  }
  return vscode.Uri.parse(`${scheme}:${copyUri.path}`);
}

/**
 * Decides where the example opens, so it never covers the welcome walkthrough. A walkthrough button
 * with `toSide:` creates an empty group beside the page and activates it, but only when the page is
 * the only group; when there is already a group beside it, the page's group stays active.
 * - Active group empty, or showing a text editor: open there (the command palette case, and the
 *   empty group created for the walkthrough).
 * - Active group showing something else, such as the walkthrough page: open beside it.
 * @param activeGroup - The editor group active when the command started.
 * @returns The column to open the example in.
 */
export function chooseExampleColumn(activeGroup: vscode.TabGroup): vscode.ViewColumn {
  const activeTab: vscode.Tab | undefined = activeGroup.activeTab;
  if (!activeTab || activeTab.input instanceof vscode.TabInputText) {
    return activeGroup.viewColumn;
  }
  return vscode.ViewColumn.Beside;
}

/**
 * Runs "Open Example": lets the user pick an example, copies it to the extension's storage and opens
 * it. Cancelling the picker does nothing. A file system failure becomes a message with a button to
 * the log, never an exception escaping the command.
 * @param context - The extension context (extension folder, global storage and version).
 * @param configService - Source of the enabled and read-only settings.
 * @param outputChannel - The channel where the outcome is logged.
 */
export async function openExample(
  context: vscode.ExtensionContext,
  configService: ConfigurationService,
  outputChannel: vscode.OutputChannel
): Promise<void> {
  // Decided now, before the picker takes focus: closing the picker hands focus back to whatever had
  // it, which from a walkthrough button is the walkthrough page.
  const targetColumn: vscode.ViewColumn = chooseExampleColumn(vscode.window.tabGroups.activeTabGroup);
  const picked: ExampleQuickPickItem | undefined = await vscode.window.showQuickPick(
    buildExampleItems(buildExampleCatalog()),
    {
      title: vscode.l10n.t('Babel TCC: Open Example'),
      placeHolder: vscode.l10n.t('Choose an example to see it in your language'),
      matchOnDescription: true,
      matchOnDetail: true,
    }
  );
  if (!picked) {
    return;
  }

  const relativePath: string = picked.entry.relativePath;
  const extensionVersion: string = String(context.extension.packageJSON.version);
  const sourceUri: vscode.Uri = vscode.Uri.joinPath(context.extensionUri, EXAMPLES_FOLDER, ...relativePath.split('/'));
  const copyUri: vscode.Uri = resolveExampleCopy(context.globalStorageUri, extensionVersion, relativePath);

  // File system boundary: copying and opening can fail (disk full, missing file in a broken
  // package); the failure is reported to the user instead of escaping the command.
  try {
    await materializeExample(sourceUri, copyUri);
    const document: vscode.TextDocument = await vscode.workspace.openTextDocument(
      buildExampleViewUri(copyUri, configService)
    );
    await vscode.window.showTextDocument(document, { preview: false, viewColumn: targetColumn });
    outputChannel.appendLine(`Opened example ${relativePath} from ${copyUri.fsPath}`);
  } catch (error: unknown) {
    const message: string = describeError(error);
    outputChannel.appendLine(`Failed to open example ${relativePath}: ${message}`);
    const openLogLabel: string = vscode.l10n.t('Open Log');
    const selection: string | undefined = await vscode.window.showErrorMessage(
      vscode.l10n.t('Babel TCC: Could not open the example: {0}', message),
      openLogLabel
    );
    if (selection === openLogLabel) {
      outputChannel.show();
    }
  }
}
