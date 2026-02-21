import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';

const STATUS_BAR_PRIORITY = 100;

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

  public dispose(): void {
    this.configSubscription.dispose();
    this.statusBarItem.dispose();
  }
}
