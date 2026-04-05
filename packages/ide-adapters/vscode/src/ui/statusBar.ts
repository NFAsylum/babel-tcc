import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { COMMANDS } from '../config/constants';

const STATUS_BAR_PRIORITY = 100;

/** Displays the current translation status and language in the VS Code status bar. */
export class StatusBar implements vscode.Disposable {
  public statusBarItem: vscode.StatusBarItem;
  public configService: ConfigurationService;
  public languageDetector: LanguageDetector;
  public configSubscription: vscode.Disposable;
  public editorSubscription: vscode.Disposable;

  constructor(configService: ConfigurationService, languageDetector: LanguageDetector) {
    this.configService = configService;
    this.languageDetector = languageDetector;
    this.statusBarItem = vscode.window.createStatusBarItem(
      vscode.StatusBarAlignment.Right,
      STATUS_BAR_PRIORITY
    );
    this.statusBarItem.command = COMMANDS.SELECT_LANGUAGE;
    this.update();
    this.statusBarItem.show();

    this.configSubscription = this.configService.onDidChangeConfiguration((): void => {
      this.update();
    });

    this.editorSubscription = vscode.window.onDidChangeActiveTextEditor((): void => {
      this.update();
    });
  }

  /** Updates the status bar item text and tooltip based on the current translation enabled state and language. */
  public update(): void {
    const enabled: boolean = this.configService.isEnabled();

    const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
    const filePath: string = editor ? editor.document.uri.path : '';
    const programmingLanguage: string = filePath ? (this.languageDetector.detectLanguage(filePath) || '') : '';
    const language: string = programmingLanguage
      ? this.configService.getLanguageForProgrammingLanguage(programmingLanguage).toUpperCase()
      : this.configService.getLanguage().toUpperCase();

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
    this.editorSubscription.dispose();
    this.statusBarItem.dispose();
  }
}
