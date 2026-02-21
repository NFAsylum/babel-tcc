import * as vscode from 'vscode';
import { TRANSLATED_SCHEME } from './translatedContentProvider';

const KEYWORD_REVERSE_MAP: Record<string, string> = {
  'usando': 'using',
  'espaconome': 'namespace',
  'classe': 'class',
  'estrutura': 'struct',
  'enumeracao': 'enum',
  'interface': 'interface',
  'publico': 'public',
  'protegido': 'protected',
  'estatico': 'static',
  'constante': 'const',
  'somenteLeitura': 'readonly',
  'vazio': 'void',
  'inteiro': 'int',
  'texto': 'string',
  'booleano': 'bool',
  'decimal': 'decimal',
  'flutuante': 'float',
  'duplo': 'double',
  'longo': 'long',
  'curto': 'short',
  'byte': 'byte',
  'caractere': 'char',
  'objeto': 'object',
  'se': 'if',
  'senao': 'else',
  'para': 'for',
  'paracada': 'foreach',
  'enquanto': 'while',
  'faca': 'do',
  'escolha': 'switch',
  'caso': 'case',
  'padrao': 'default',
  'retornar': 'return',
  'quebrar': 'break',
  'continuar': 'continue',
  'novo': 'new',
  'nulo': 'null',
  'verdadeiro': 'true',
  'falso': 'false',
  'este': 'this',
  'base': 'base',
  'lancamento': 'throw',
  'tentar': 'try',
  'capturar': 'catch',
  'finalmente': 'finally',
  'em': 'in',
  'de': 'out',
  'referencia': 'ref',
  'virtual': 'virtual',
  'sobrescrever': 'override',
  'abstrato': 'abstract',
  'selado': 'sealed',
  'async': 'async',
  'aguardar': 'await',
  'tipode': 'typeof',
  'tamanhode': 'sizeof',
  'como': 'as',
  'e': 'is'
};

export class HoverProvider implements vscode.HoverProvider {
  public provideHover(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.Hover | undefined {
    if (document.uri.scheme !== TRANSLATED_SCHEME) {
      return undefined;
    }

    const wordRange: vscode.Range | undefined = document.getWordRangeAtPosition(position);
    if (!wordRange) {
      return undefined;
    }

    const word: string = document.getText(wordRange);
    const originalKeyword: string | undefined = KEYWORD_REVERSE_MAP[word.toLowerCase()];

    if (originalKeyword) {
      const markdown: vscode.MarkdownString = new vscode.MarkdownString();
      markdown.appendCodeblock(`${originalKeyword}`, 'csharp');
      markdown.appendMarkdown(`\n\nC# keyword: \`${originalKeyword}\``);
      return new vscode.Hover(markdown, wordRange);
    }

    return undefined;
  }
}
