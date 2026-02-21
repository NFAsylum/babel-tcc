import * as vscode from 'vscode';
import { TRANSLATED_SCHEME } from './translatedContentProvider';

const TRANSLATED_KEYWORDS: Record<string, string> = {
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

export class CompletionProvider implements vscode.CompletionItemProvider {
  public provideCompletionItems(
    document: vscode.TextDocument,
    position: vscode.Position
  ): vscode.CompletionItem[] {
    if (document.uri.scheme !== TRANSLATED_SCHEME) {
      return [];
    }

    const wordRange: vscode.Range | undefined = document.getWordRangeAtPosition(position);
    if (!wordRange) {
      return [];
    }

    const currentWord: string = document.getText(wordRange).toLowerCase();
    const items: vscode.CompletionItem[] = [];

    for (const [translated, original] of Object.entries(TRANSLATED_KEYWORDS)) {
      if (translated.startsWith(currentWord)) {
        const item: vscode.CompletionItem = new vscode.CompletionItem(
          translated,
          vscode.CompletionItemKind.Keyword
        );
        item.detail = `C# keyword: ${original}`;
        item.insertText = translated;
        items.push(item);
      }
    }

    return items;
  }
}
