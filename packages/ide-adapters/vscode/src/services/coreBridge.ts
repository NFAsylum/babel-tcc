import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { spawn } from 'child_process';

const DEFAULT_TIMEOUT_MS = 10000;

/** Response object returned by the .NET Core process. */
export interface CoreResponse {
  /** Whether the Core operation completed successfully. */
  success: boolean;
  /** The serialized result payload on success. */
  result: string;
  /** The error message when the operation fails. */
  error: string;
}

/** Result of a syntax validation check. */
export interface ValidationResult {
  /** Whether the source code is syntactically valid. */
  isValid: boolean;
  /** List of diagnostic entries describing any syntax issues found. */
  diagnostics: DiagnosticEntry[];
}

/** A single diagnostic entry from syntax validation. */
export interface DiagnosticEntry {
  /** Severity level of the diagnostic (e.g., "Error", "Warning"). */
  severity: string;
  /** Human-readable description of the issue. */
  message: string;
  /** 1-based line number where the issue occurs. */
  line: number;
  /** 1-based column number where the issue occurs. */
  column: number;
}

/** Bridge to the .NET Core translation engine, spawning a child process for each request. */
export class CoreBridge {
  /** Absolute path to the Core host DLL. */
  public coreDllPath: string;
  /** Absolute path to the translations resource directory. */
  public translationsPath: string;
  /** Absolute path to the current workspace project root. */
  public projectPath: string;
  /** Output channel used for logging Core bridge activity. */
  public outputChannel: vscode.OutputChannel;
  /** Maximum time in milliseconds to wait for a Core process response before timing out. */
  public timeoutMs: number;

  constructor(
    context: vscode.ExtensionContext,
    outputChannel: vscode.OutputChannel,
    timeoutMs: number = DEFAULT_TIMEOUT_MS
  ) {
    this.coreDllPath = path.join(
      context.extensionPath,
      'bin',
      'MultiLingualCode.Core.Host.dll'
    );
    this.projectPath = '';
    this.outputChannel = outputChannel;
    this.timeoutMs = timeoutMs;

    const workspaceFolders: readonly vscode.WorkspaceFolder[] | undefined =
      vscode.workspace.workspaceFolders;
    if (workspaceFolders && workspaceFolders.length > 0) {
      this.projectPath = workspaceFolders[0].uri.fsPath;
    }

    this.translationsPath = this.resolveTranslationsPath(context);
  }

  /**
   * Translates source code keywords and identifiers into a target natural language.
   * @param sourceCode - The original source code to translate.
   * @param fileExtension - The file extension indicating the programming language (e.g., ".cs").
   * @param targetLanguage - The target natural language code (e.g., "pt-br").
   * @returns The translated source code as a string.
   */
  public async translateToNaturalLanguage(
    sourceCode: string,
    fileExtension: string,
    targetLanguage: string
  ): Promise<string> {
    const params: Record<string, string> = {
      sourceCode,
      fileExtension,
      targetLanguage
    };
    const response: CoreResponse = await this.invokeCore('TranslateToNaturalLanguage', params);
    return response.result;
  }

  /**
   * Translates natural-language code back into valid programming-language source code.
   * @param translatedCode - The translated code to convert back.
   * @param fileExtension - The file extension indicating the programming language (e.g., ".cs").
   * @param sourceLanguage - The natural language code the input is written in (e.g., "pt-br").
   * @returns The original programming-language source code as a string.
   */
  public async translateFromNaturalLanguage(
    translatedCode: string,
    fileExtension: string,
    sourceLanguage: string
  ): Promise<string> {
    const params: Record<string, string> = {
      translatedCode,
      fileExtension,
      sourceLanguage
    };
    const response: CoreResponse = await this.invokeCore('TranslateFromNaturalLanguage', params);
    return response.result;
  }

  /**
   * Validates the syntax of the given source code.
   * @param sourceCode - The source code to validate.
   * @param fileExtension - The file extension indicating the programming language (e.g., ".cs").
   * @returns A validation result containing diagnostics if any issues are found.
   */
  public async validateSyntax(
    sourceCode: string,
    fileExtension: string
  ): Promise<ValidationResult> {
    const params: Record<string, string> = {
      sourceCode,
      fileExtension
    };
    const response: CoreResponse = await this.invokeCore('ValidateSyntax', params);
    const parsed: ValidationResult = JSON.parse(response.result) as ValidationResult;
    return parsed;
  }

  /**
   * Retrieves the list of natural languages supported by the Core translation engine.
   * @returns An array of language codes (e.g., ["pt-br", "es"]).
   */
  public async getSupportedLanguages(): Promise<string[]> {
    const response: CoreResponse = await this.invokeCore('GetSupportedLanguages', {});
    const parsed: string[] = JSON.parse(response.result) as string[];
    return parsed;
  }

  /**
   * Spawns a .NET Core child process to invoke the specified method with the given parameters.
   * @param method - The Core host method name to invoke (e.g., "TranslateToNaturalLanguage").
   * @param params - A key-value map of parameters to pass to the Core method.
   * @returns The parsed Core response containing success status, result, or error.
   */
  public invokeCore(method: string, params: Record<string, unknown>): Promise<CoreResponse> {
    return new Promise<CoreResponse>((resolve, reject): void => {
      const args: string[] = [
        this.coreDllPath,
        '--method', method,
        '--params', JSON.stringify(params)
      ];

      if (this.translationsPath) {
        args.push('--translations', this.translationsPath);
      }

      if (this.projectPath) {
        args.push('--project', this.projectPath);
      }

      this.outputChannel.appendLine(`CoreBridge: invoking ${method}`);

      const coreProcess = spawn('dotnet', args);

      let stdout = '';
      let stderr = '';

      const timeoutHandle: NodeJS.Timeout = setTimeout((): void => {
        coreProcess.kill();
        reject(new Error(`CoreBridge: timeout after ${this.timeoutMs}ms for method ${method}`));
      }, this.timeoutMs);

      coreProcess.stdout.on('data', (data: Buffer): void => {
        stdout += data.toString();
      });

      coreProcess.stderr.on('data', (data: Buffer): void => {
        stderr += data.toString();
      });

      coreProcess.on('close', (_code: number | null): void => {
        clearTimeout(timeoutHandle);

        if (stderr) {
          this.outputChannel.appendLine(`CoreBridge stderr: ${stderr}`);
        }

        if (stdout.length === 0) {
          reject(new Error(`CoreBridge: no output from Core process for method ${method}`));
          return;
        }

        const response: CoreResponse = JSON.parse(stdout) as CoreResponse;

        if (!response.success) {
          this.outputChannel.appendLine(`CoreBridge error: ${response.error}`);
          reject(new Error(`CoreBridge: ${response.error}`));
          return;
        }

        this.outputChannel.appendLine(`CoreBridge: ${method} completed successfully`);
        resolve(response);
      });

      coreProcess.on('error', (err: Error): void => {
        clearTimeout(timeoutHandle);
        this.outputChannel.appendLine(`CoreBridge: process error - ${err.message}`);
        reject(new Error(`CoreBridge: failed to start Core process - ${err.message}`));
      });
    });
  }

  /**
   * Resolves the translations path using a 3-level fallback:
   * 1. Setting babel-tcc.translationsPath
   * 2. Auto-detect babel-tcc-translations as sibling of workspace
   * 3. Embedded translations inside the extension
   */
  public resolveTranslationsPath(context: vscode.ExtensionContext): string {
    const config: vscode.WorkspaceConfiguration = vscode.workspace.getConfiguration('babel-tcc');
    const configuredPath: string = config.get<string>('translationsPath', '');

    if (configuredPath && fs.existsSync(configuredPath)) {
      this.outputChannel.appendLine(`CoreBridge: translations via setting: ${configuredPath}`);
      return configuredPath;
    }

    if (this.projectPath) {
      const workspaceParent: string = path.dirname(this.projectPath);
      const siblingPath: string = path.join(workspaceParent, 'babel-tcc-translations');

      if (fs.existsSync(siblingPath)) {
        this.outputChannel.appendLine(`CoreBridge: translations auto-detectadas: ${siblingPath}`);
        return siblingPath;
      }
    }

    const embeddedPath: string = path.join(context.extensionPath, 'translations');
    this.outputChannel.appendLine(`CoreBridge: translations embeddadas: ${embeddedPath}`);
    return embeddedPath;
  }

  /** Disposes of the CoreBridge, logging the disposal event. */
  public dispose(): void {
    this.outputChannel.appendLine('CoreBridge: disposed');
  }
}
