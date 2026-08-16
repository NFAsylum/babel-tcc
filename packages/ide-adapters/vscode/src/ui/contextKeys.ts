import * as vscode from 'vscode';
import { ConfigurationService } from '../services/configurationService';
import { LanguageDetector } from '../services/languageDetector';
import { isTranslatedScheme, READONLY_SCHEME } from '../providers/translatedContentProvider';
import { CONTEXT_KEYS } from '../config/constants';

/** Built-in VS Code command that publishes a context key value to the `when` clause evaluator. */
const SET_CONTEXT_COMMAND = 'setContext';

/**
 * Publishes and keeps in sync the `babelTcc.*` context keys consumed by the `when` clauses of
 * menus and keybindings, so UI visibility is declared in the manifest instead of being decided
 * by a runtime check inside each command.
 */
export class ContextKeyManager implements vscode.Disposable {
  /** Source of the enabled flag and of the notification that a setting changed. */
  public configService: ConfigurationService;
  /** Decides whether the active file belongs to a supported programming language. */
  public languageDetector: LanguageDetector;
  /** Subscription to active editor changes. Replaced by the real one in `create`. */
  public editorSubscription: vscode.Disposable = new vscode.Disposable((): void => { });
  /** Subscription to configuration changes. Replaced by the real one in `create`. */
  public configSubscription: vscode.Disposable = new vscode.Disposable((): void => { });

  /**
   * Declares the manager properties and nothing else. Initialization logic lives in `create`,
   * as required by docs/padroes-codigo.md — the constructor exists only because
   * `strictPropertyInitialization` makes it structural in TypeScript.
   * @param configService - Service that exposes the enabled flag and configuration changes.
   * @param languageDetector - Detector used to decide whether a file path is supported.
   */
  constructor(configService: ConfigurationService, languageDetector: LanguageDetector) {
    this.configService = configService;
    this.languageDetector = languageDetector;
  }

  /**
   * Creates a manager that is already subscribed to the events that invalidate the keys and has
   * published their first values.
   * @param configService - Service that exposes the enabled flag and configuration changes.
   * @param languageDetector - Detector used to decide whether a file path is supported.
   * @returns A manager wired to the editor and configuration events.
   */
  public static create(
    configService: ConfigurationService,
    languageDetector: LanguageDetector
  ): ContextKeyManager {
    const manager: ContextKeyManager = new ContextKeyManager(configService, languageDetector);

    manager.editorSubscription = vscode.window.onDidChangeActiveTextEditor((): void => {
      manager.refresh();
    });
    manager.configSubscription = configService.onDidChangeConfiguration((): void => {
      manager.refresh();
    });
    manager.refresh();

    return manager;
  }

  /**
   * Recomputes and republishes every context key from the current editor and configuration.
   * `vscode.window.activeTextEditor` is nullable by definition of the VS Code API; that nullable
   * is consumed here and normalized to empty strings, so no nullable value flows any further.
   */
  public refresh(): void {
    let activeFilePath: string = '';
    let activeScheme: string = '';
    const activeEditor: vscode.TextEditor | undefined = vscode.window.activeTextEditor;
    if (activeEditor) {
      activeFilePath = activeEditor.document.uri.path;
      activeScheme = activeEditor.document.uri.scheme;
    }

    let isSupportedFile: boolean = false;
    if (activeFilePath !== '') {
      isSupportedFile = this.languageDetector.isSupported(activeFilePath);
    }

    this.publishKey(CONTEXT_KEYS.ENABLED, this.configService.isEnabled());
    this.publishKey(CONTEXT_KEYS.SUPPORTED_FILE, isSupportedFile);
    this.publishKey(CONTEXT_KEYS.TRANSLATED_VIEW, isTranslatedScheme(activeScheme));
    this.publishKey(CONTEXT_KEYS.READONLY_VIEW, activeScheme === READONLY_SCHEME);
  }

  /**
   * Publishes a single context key to VS Code.
   * The command is intentionally not awaited: `when` clauses are re-evaluated by VS Code when the
   * key lands, and blocking the editor event handler on it would add latency for no benefit.
   * @param contextKeyName - Name of the context key, from `CONTEXT_KEYS`.
   * @param contextKeyValue - Value to publish for that key.
   */
  public publishKey(contextKeyName: string, contextKeyValue: boolean): void {
    vscode.commands.executeCommand(SET_CONTEXT_COMMAND, contextKeyName, contextKeyValue);
  }

  /** Disposes of the editor and configuration subscriptions. */
  public dispose(): void {
    this.editorSubscription.dispose();
    this.configSubscription.dispose();
  }
}
