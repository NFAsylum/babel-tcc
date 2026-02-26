import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';

const STATUS_BAR_PRIORITY = 100;

/** Displays the current translation status and language in the VS Code status bar. */
export class StatusBar implements vscode.Disposable {
  public statusBarItem: vscode.StatusBarItem;
  public configService: ConfigurationService;
  public configSubscription: vscode.Disposable;

  constructor(configService: ConfigurationService) {
    this.configService = configService;
    this.statusBarItem = vscode.window.createStatusBarItem(
      vscode.StatusBarAlignment.Right,
      STATUS_BAR_PRIORITY
    );
    this.statusBarItem.command = 'babel-tcc.selectLanguage';
    this.update();
    this.statusBarItem.show();

    this.configSubscription = this.configService.onDidChangeConfiguration((): void => {
      this.update();
    });
  }

  /** Updates the status bar item text and tooltip based on the current translation enabled state and language. */
  public update(): void {
    const enabled: boolean = this.configService.isEnabled();
    const language: string = this.configService.getLanguage().toUpperCase();

    if (enabled) {
      this.statusBarItem.text = `$(globe) ${language}`;
      this.statusBarItem.tooltip = `Babel TCC: Translation active (${language}). Click to change language.`;
    } else {
      this.statusBarItem.text = '$(globe) OFF';
      this.statusBarItem.tooltip = 'Babel TCC: Translation disabled. Click to change language.';
    }
  }

  /** Disposes of the configuration change subscription and the status bar item. */
  public dispose(): void {
    this.configSubscription.dispose();
    this.statusBarItem.dispose();
  }
}
