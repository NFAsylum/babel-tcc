import { vi } from 'vitest';

// --- Core classes ---

export class EventEmitter<T> {
  private listeners: Array<(e: T) => void> = [];

  event = (listener: (e: T) => void): { dispose: () => void } => {
    this.listeners.push(listener);
    return {
      dispose: () => {
        const idx = this.listeners.indexOf(listener);
        if (idx >= 0) this.listeners.splice(idx, 1);
      },
    };
  };

  fire(data: T): void {
    for (const l of this.listeners) l(data);
  }

  dispose(): void {
    this.listeners = [];
  }
}

export class Disposable {
  private callOnDispose?: () => void;
  constructor(callOnDispose?: () => void) {
    this.callOnDispose = callOnDispose;
  }
  static from(...disposables: Array<{ dispose(): void }>): Disposable {
    return new Disposable(() => disposables.forEach((d) => d.dispose()));
  }
  dispose(): void {
    this.callOnDispose?.();
  }
}

export class Uri {
  scheme: string;
  authority: string;
  path: string;
  query: string;
  fragment: string;
  fsPath: string;

  private constructor(scheme: string, path: string) {
    this.scheme = scheme;
    this.authority = '';
    this.path = path;
    this.query = '';
    this.fragment = '';
    this.fsPath = path;
  }

  static file(p: string): Uri {
    return new Uri('file', p);
  }

  static parse(value: string): Uri {
    const colonIdx = value.indexOf(':');
    if (colonIdx >= 0) {
      return new Uri(value.substring(0, colonIdx), value.substring(colonIdx + 1));
    }
    return new Uri('file', value);
  }

  toString(): string {
    return `${this.scheme}:${this.path}`;
  }

  with(change: { scheme?: string; path?: string }): Uri {
    const u = new Uri(change.scheme ?? this.scheme, change.path ?? this.path);
    return u;
  }
}

export class Position {
  constructor(
    public line: number,
    public character: number
  ) {}
}

export class Range {
  constructor(
    public start: Position,
    public end: Position
  ) {}
}

export class MarkdownString {
  value = '';

  appendCodeblock(code: string, language?: string): MarkdownString {
    this.value += `\`\`\`${language ?? ''}\n${code}\n\`\`\`\n`;
    return this;
  }

  appendMarkdown(value: string): MarkdownString {
    this.value += value;
    return this;
  }
}

export class CompletionItem {
  detail?: string;
  insertText?: string;
  constructor(
    public label: string,
    public kind?: number
  ) {}
}

export class Hover {
  constructor(
    public contents: MarkdownString | string,
    public range?: Range
  ) {}
}

export class WorkspaceEdit {
  replace = vi.fn();
}

export class TabInputText {
  constructor(public uri: Uri) {}
}

export class SemanticTokensLegend {
  constructor(
    public tokenTypes: string[],
    public tokenModifiers: string[] = []
  ) {}
}

export class SemanticTokensBuilder {
  private tokens: Array<{ line: number; char: number; length: number; type: number }> = [];
  constructor(public legend?: SemanticTokensLegend) {}
  push(line: number, char: number, length: number, tokenType: number): void {
    this.tokens.push({ line, char, length, type: tokenType });
  }
  build(): { data: Uint32Array } {
    const data: Uint32Array = new Uint32Array(this.tokens.length * 5);
    let prevLine = 0;
    let prevChar = 0;
    for (let i = 0; i < this.tokens.length; i++) {
      const token = this.tokens[i];
      const deltaLine = token.line - prevLine;
      const deltaChar = deltaLine === 0 ? token.char - prevChar : token.char;
      data[i * 5] = deltaLine;
      data[i * 5 + 1] = deltaChar;
      data[i * 5 + 2] = token.length;
      data[i * 5 + 3] = token.type;
      data[i * 5 + 4] = 0;
      prevLine = token.line;
      prevChar = token.char;
    }
    return { data };
  }
}

// --- Enums ---

export enum CompletionItemKind {
  Keyword = 14,
}

export enum StatusBarAlignment {
  Left = 1,
  Right = 2,
}

export enum ConfigurationTarget {
  Global = 1,
  Workspace = 2,
  WorkspaceFolder = 3,
}

export enum FileChangeType {
  Changed = 1,
  Created = 2,
  Deleted = 3,
}

export enum FileType {
  File = 1,
  Directory = 2,
  SymbolicLink = 64,
}

export enum ViewColumn {
  One = 1,
  Two = 2,
  Beside = -2,
}

// --- Namespace objects ---

const configValues: Record<string, unknown> = {};

export const workspace = {
  getConfiguration: vi.fn((_section?: string) => ({
    get: vi.fn((key: string, defaultValue?: unknown) => {
      const fullKey = `${_section}.${key}`;
      return fullKey in configValues ? configValues[fullKey] : defaultValue;
    }),
    update: vi.fn(),
  })),
  onDidChangeConfiguration: vi.fn((cb: (e: unknown) => void) => {
    workspace.__configChangeCallbacks.push(cb);
    return {
      dispose: vi.fn(() => {
        const idx = workspace.__configChangeCallbacks.indexOf(cb);
        if (idx >= 0) workspace.__configChangeCallbacks.splice(idx, 1);
      }),
    };
  }),
  __configChangeCallbacks: [] as Array<(e: unknown) => void>,
  __fireConfigChange: (affectedSection?: string): void => {
    const event = { affectsConfiguration: (s: string): boolean => s === (affectedSection ?? 'babel-tcc') };
    for (const cb of workspace.__configChangeCallbacks) cb(event);
  },
  workspaceFolders: undefined as unknown,
  registerFileSystemProvider: vi.fn(() => ({ dispose: vi.fn() })),
  openTextDocument: vi.fn(),
  createFileSystemWatcher: vi.fn(() => ({
    onDidChange: vi.fn(() => ({ dispose: vi.fn() })),
    onDidCreate: vi.fn(() => ({ dispose: vi.fn() })),
    onDidDelete: vi.fn(() => ({ dispose: vi.fn() })),
    dispose: vi.fn(),
  })),
  fs: {
    stat: vi.fn(),
    readFile: vi.fn(),
    writeFile: vi.fn(),
  },
  textDocuments: [] as unknown[],
  applyEdit: vi.fn(),
};

export const window = {
  createOutputChannel: vi.fn(() => ({
    appendLine: vi.fn(),
    show: vi.fn(),
    dispose: vi.fn(),
  })),
  createStatusBarItem: vi.fn(() => ({
    text: '',
    tooltip: '',
    command: '',
    show: vi.fn(),
    hide: vi.fn(),
    dispose: vi.fn(),
  })),
  showInformationMessage: vi.fn(),
  showWarningMessage: vi.fn(),
  showErrorMessage: vi.fn(),
  showQuickPick: vi.fn(),
  onDidChangeActiveTextEditor: vi.fn(() => ({ dispose: vi.fn() })),
  showTextDocument: vi.fn(),
  activeTextEditor: undefined as unknown,
  tabGroups: {
    all: [] as unknown[],
    close: vi.fn(),
  },
};

export const commands = {
  registerCommand: vi.fn(() => ({ dispose: vi.fn() })),
  executeCommand: vi.fn(),
};

export const languages = {
  registerCompletionItemProvider: vi.fn(() => ({ dispose: vi.fn() })),
  registerHoverProvider: vi.fn(() => ({ dispose: vi.fn() })),
  registerDocumentSemanticTokensProvider: vi.fn(() => ({ dispose: vi.fn() })),
};

// --- Helper to set config values in tests ---
export function __setConfigValue(key: string, value: unknown): void {
  configValues[key] = value;
}

export function __clearConfigValues(): void {
  for (const key of Object.keys(configValues)) {
    delete configValues[key];
  }
}
