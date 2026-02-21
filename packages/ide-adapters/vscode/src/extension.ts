import * as vscode from 'vscode';

const OUTPUT_CHANNEL_NAME = 'Babel TCC';

let outputChannel: vscode.OutputChannel;

export function activate(context: vscode.ExtensionContext): void {
  outputChannel = vscode.window.createOutputChannel(OUTPUT_CHANNEL_NAME);
  outputChannel.appendLine('Babel TCC extension activated.');

  const toggleCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.toggle',
    (): void => {
      const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration('babel-tcc');
      const currentEnabled: boolean = config.get<boolean>('enabled', true);
      config.update('enabled', !currentEnabled, vscode.ConfigurationTarget.Global);
      const status: string = !currentEnabled ? 'enabled' : 'disabled';
      outputChannel.appendLine(`Translation ${status}.`);
      vscode.window.showInformationMessage(`Babel TCC: Translation ${status}.`);
    }
  );

  const selectLanguageCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.selectLanguage',
    async (): Promise<void> => {
      const languages: string[] = ['pt-br'];
      const selected: string | undefined = await vscode.window.showQuickPick(languages, {
        placeHolder: 'Select target language for translation'
      });
      if (selected) {
        const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration('babel-tcc');
        config.update('language', selected, vscode.ConfigurationTarget.Global);
        outputChannel.appendLine(`Language set to: ${selected}`);
        vscode.window.showInformationMessage(`Babel TCC: Language set to ${selected}.`);
      }
    }
  );

  const openTranslatedCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.openTranslated',
    (): void => {
      outputChannel.appendLine('Open translated view requested.');
      vscode.window.showInformationMessage('Babel TCC: Translated view not yet implemented.');
    }
  );

  const showOriginalCommand: vscode.Disposable = vscode.commands.registerCommand(
    'babel-tcc.showOriginal',
    (): void => {
      outputChannel.appendLine('Show original code requested.');
      vscode.window.showInformationMessage('Babel TCC: Show original not yet implemented.');
    }
  );

  context.subscriptions.push(outputChannel);
  context.subscriptions.push(toggleCommand);
  context.subscriptions.push(selectLanguageCommand);
  context.subscriptions.push(openTranslatedCommand);
  context.subscriptions.push(showOriginalCommand);

  outputChannel.appendLine('All commands registered successfully.');
}

export function deactivate(): void {
  if (outputChannel) {
    outputChannel.appendLine('Babel TCC extension deactivated.');
  }
}
