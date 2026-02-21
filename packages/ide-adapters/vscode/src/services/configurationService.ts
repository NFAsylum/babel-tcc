import * as vscode from 'vscode';

const CONFIG_SECTION = 'babel-tcc';
const KEY_ENABLED = 'enabled';
const KEY_LANGUAGE = 'language';

export class ConfigurationService implements vscode.Disposable {
  public configChangeSubscription: vscode.Disposable;
  public changeEmitter: vscode.EventEmitter<void> = new vscode.EventEmitter<void>();
  public onDidChangeConfiguration: vscode.Event<void> = this.changeEmitter.event;

  constructor() {
    this.configChangeSubscription = vscode.workspace.onDidChangeConfiguration(
      (event: vscode.ConfigurationChangeEvent): void => {
        if (event.affectsConfiguration(CONFIG_SECTION)) {
          this.changeEmitter.fire();
        }
      }
    );
  }

  public isEnabled(): boolean {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return config.get<boolean>(KEY_ENABLED, true);
  }

  public getLanguage(): string {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return config.get<string>(KEY_LANGUAGE, 'pt-br');
  }

  public async setEnabled(enabled: boolean): Promise<void> {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    await config.update(KEY_ENABLED, enabled, vscode.ConfigurationTarget.Global);
  }

  public async setLanguage(language: string): Promise<void> {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    await config.update(KEY_LANGUAGE, language, vscode.ConfigurationTarget.Global);
  }

  public dispose(): void {
    this.configChangeSubscription.dispose();
    this.changeEmitter.dispose();
  }
}
