import * as vscode from 'vscode';

const CONFIG_SECTION = 'babel-tcc';
const KEY_ENABLED = 'enabled';
const KEY_LANGUAGE = 'language';
const KEY_READONLY = 'readonly';

/** Manages VS Code workspace configuration for the Babel TCC extension. */
export class ConfigurationService implements vscode.Disposable {
  /** Subscription that listens for workspace configuration changes. */
  public configChangeSubscription: vscode.Disposable;
  /** Event emitter that fires when Babel TCC configuration settings change. */
  public changeEmitter: vscode.EventEmitter<void> = new vscode.EventEmitter<void>();
  /** Event that fires when any Babel TCC configuration setting changes. */
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

  /**
   * Returns whether translation is currently enabled.
   * @returns `true` if translation is enabled, `false` otherwise. Defaults to `true`.
   */
  public isEnabled(): boolean {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return config.get<boolean>(KEY_ENABLED, true);
  }

  /**
   * Returns the currently configured target language for translation.
   * @returns The language code string (e.g., "pt-br"). Defaults to "pt-br".
   */
  public getLanguage(): string {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return config.get<string>(KEY_LANGUAGE, 'pt-br');
  }

  /**
   * Sets whether translation is enabled in the global configuration.
   * @param enabled - `true` to enable translation, `false` to disable it.
   */
  public async setEnabled(enabled: boolean): Promise<void> {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    await config.update(KEY_ENABLED, enabled, vscode.ConfigurationTarget.Global);
  }

  /**
   * Sets the target language for translation in the global configuration.
   * @param language - The language code to set (e.g., "pt-br").
   */
  public async setLanguage(language: string): Promise<void> {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    await config.update(KEY_LANGUAGE, language, vscode.ConfigurationTarget.Global);
  }

  /** Returns whether translated views should open in readonly mode. Defaults to false. */
  public isReadonly(): boolean {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    return config.get<boolean>(KEY_READONLY, false);
  }

  /** Sets whether translated views should open in readonly mode. */
  public async setReadonly(readonly: boolean): Promise<void> {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration(CONFIG_SECTION);
    await config.update(KEY_READONLY, readonly, vscode.ConfigurationTarget.Global);
  }

  /** Disposes of the configuration change subscription and the event emitter. */
  public dispose(): void {
    this.configChangeSubscription.dispose();
    this.changeEmitter.dispose();
  }
}
