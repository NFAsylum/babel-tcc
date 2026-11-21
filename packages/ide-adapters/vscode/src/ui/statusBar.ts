import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { COMMANDS } from '../config/constants';

const STATUS_BAR_PRIORITY = 100;

/**
 * Name of the item in the status bar's own context menu, where the user can hide or show it. It is
 * the product name, identical in every locale, like the command category.
 */
const STATUS_BAR_NAME = 'Babel TCC';

/** Role announced by screen readers: the item opens the language picker when activated. */
const STATUS_BAR_ROLE = 'button';

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
    this.statusBarItem.name = STATUS_BAR_NAME;
    this.update();
    this.statusBarItem.show();

    this.configSubscription = this.configService.onDidChangeConfiguration((): void => {
      this.update();
    });

    this.editorSubscription = vscode.window.onDidChangeActiveTextEditor((): void => {
      this.update();
    });
  }

  /**
   * Updates the status bar item text, tooltip and accessible label based on the current translation
   * enabled state and language. The `$(globe)` codicon is not read by screen readers, so the text
   * alone would be announced as just "PT-BR"; the accessible label says what the item is, its state
   * and what activating it does.
   */
  public update(): void {
    const enabled: boolean = this.configService.isEnabled();

    const editor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
    let filePath: string = '';
    if (editor) {
      filePath = editor.document.uri.path;
    }
    let programmingLanguage: string = '';
    if (filePath) {
      programmingLanguage = this.languageDetector.detectLanguage(filePath) || '';
    }
    let language: string = this.configService.getLanguage().toUpperCase();
    if (programmingLanguage) {
      language = this.configService.getLanguageForProgrammingLanguage(programmingLanguage).toUpperCase();
    }

    if (enabled) {
      this.statusBarItem.text = `$(globe) ${language}`;
      this.statusBarItem.tooltip = vscode.l10n.t('Babel TCC: Translation active ({0}). Click to change language.', language);
      this.statusBarItem.accessibilityInformation = {
        label: vscode.l10n.t('Babel TCC: translation active, language {0}. Opens the language picker.', language),
        role: STATUS_BAR_ROLE,
      };
    } else {
      this.statusBarItem.text = '$(globe) OFF';
      this.statusBarItem.tooltip = vscode.l10n.t('Babel TCC: Translation disabled. Click to change language.');
      this.statusBarItem.accessibilityInformation = {
        label: vscode.l10n.t('Babel TCC: translation disabled. Opens the language picker.'),
        role: STATUS_BAR_ROLE,
      };
    }
  }

  /** Disposes of the configuration change subscription and the status bar item. */
  public dispose(): void {
    this.configSubscription.dispose();
    this.editorSubscription.dispose();
    this.statusBarItem.dispose();
  }
}
