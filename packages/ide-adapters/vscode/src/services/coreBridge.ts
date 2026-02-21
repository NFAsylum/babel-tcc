import * as vscode from 'vscode';
import * as path from 'path';
import { spawn } from 'child_process';

const DEFAULT_TIMEOUT_MS = 10000;

export interface CoreResponse {
  success: boolean;
  result: string;
  error: string;
}

export interface ValidationResult {
  isValid: boolean;
  diagnostics: DiagnosticEntry[];
}

export interface DiagnosticEntry {
  severity: string;
  message: string;
  line: number;
  column: number;
}

export class CoreBridge {
  public coreDllPath: string;
  public translationsPath: string;
  public projectPath: string;
  public outputChannel: vscode.OutputChannel;
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
    this.translationsPath = path.join(
      context.extensionPath,
      'translations'
    );
    this.projectPath = '';
    this.outputChannel = outputChannel;
    this.timeoutMs = timeoutMs;

    const workspaceFolders: readonly vscode.WorkspaceFolder[] | undefined =
      vscode.workspace.workspaceFolders;
    if (workspaceFolders && workspaceFolders.length > 0) {
      this.projectPath = workspaceFolders[0].uri.fsPath;
    }
  }

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

  public async getSupportedLanguages(): Promise<string[]> {
    const response: CoreResponse = await this.invokeCore('GetSupportedLanguages', {});
    const parsed: string[] = JSON.parse(response.result) as string[];
    return parsed;
  }

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

      coreProcess.on('close', (code: number | null): void => {
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

  public dispose(): void {
    this.outputChannel.appendLine('CoreBridge: disposed');
  }
}
