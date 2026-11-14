import * as vscode from 'vscode';
import { WALKTHROUGH_ID } from '../config/constants';

/** Built-in command that opens a walkthrough on the "Welcome" page. */
export const OPEN_WALKTHROUGH_BUILTIN = 'workbench.action.openWalkthrough';

/**
 * Builds the reference VS Code expects for a walkthrough: `<extension id>#<walkthrough id>`. The
 * extension id comes from the running extension instead of a literal, so a change of publisher or
 * name cannot leave the command pointing at a walkthrough that no longer exists.
 * @param extensionId - The id of the running extension (`publisher.name`).
 * @returns The walkthrough reference.
 */
export function buildWalkthroughReference(extensionId: string): string {
  return `${extensionId}#${WALKTHROUGH_ID}`;
}

/**
 * Runs "Open Introduction": opens the welcome walkthrough again. VS Code shows a walkthrough on its
 * own only right after installation; once dismissed, this command is the way back to it.
 * @param context - The extension context, source of the extension id.
 */
export async function openWalkthrough(context: vscode.ExtensionContext): Promise<void> {
  await vscode.commands.executeCommand(
    OPEN_WALKTHROUGH_BUILTIN,
    buildWalkthroughReference(context.extension.id),
    false
  );
}
