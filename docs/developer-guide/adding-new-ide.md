# Adicionar Novo IDE

## Índice

- [Arquitetura Core <-> IDE](#arquitetura-core---ide)
- [Protocolo JSON stdin/stdout](#protocolo-json-stdinstdout)
- [Métodos disponíveis](#métodos-disponíveis)
- [Criar novo IDE adapter](#criar-novo-ide-adapter)

## Arquitetura Core <-> IDE

O Core Engine é independente do IDE. A comunicação acontece via processo .NET que recebe requests JSON:

```
IDE Adapter (qualquer linguagem)
        |
        | spawn: dotnet MultiLingualCode.Core.Host.dll
        | args: --method, --params, --translations, --project
        |
        v
Core Host (C# / .NET 8)
        |
        v
Core Engine (TranslationOrchestrator)
```

## Protocolo JSON stdin/stdout

### Request (via argumentos CLI)

```bash
dotnet MultiLingualCode.Core.Host.dll \
  --method TranslateToNaturalLanguage \
  --params '{"sourceCode":"class Program { }","fileExtension":".cs","targetLanguage":"pt-br"}' \
  --translations /path/to/translations \
  --project /path/to/project
```

### Response (via stdout)

```json
{
  "success": true,
  "result": "classe Program { }",
  "error": ""
}
```

### Erro

```json
{
  "success": false,
  "result": "",
  "error": "Unsupported file extension: .xyz"
}
```

## Métodos disponíveis

| Método | Params | Descrição |
|--------|--------|-----------|
| `TranslateToNaturalLanguage` | sourceCode, fileExtension, targetLanguage | Traduz código para idioma natural |
| `ApplyTranslatedEdits` | originalCode, previousTranslatedCode, editedTranslatedCode, fileExtension, sourceLanguage | **Use este ao salvar.** Diff de 3 vias: linhas não alteradas são copiadas do original sem tradução reversa |
| `TranslateFromNaturalLanguage` | translatedCode, fileExtension, sourceLanguage | Tradução reversa simples. *Fallback* para chamadas sem contexto de diff — ver limitação abaixo |
| `ValidateSyntax` | sourceCode, fileExtension | Valida sintaxe do código |
| `GetSupportedLanguages` | (nenhum) | Retorna lista de idiomas suportados |

### Por que salvar com `ApplyTranslatedEdits`

`TranslateFromNaturalLanguage` recebe apenas o código traduzido, então não consegue distinguir uma
keyword traduzida de um identificador com o mesmo texto. Uma variável chamada `se` em pt-br volta
como `if`, corrompendo o arquivo (ver tarefa 085).

`ApplyTranslatedEdits` recebe as três versões — original em disco, tradução exibida e tradução
editada — e copia do original toda linha que não mudou, sem reinterpretá-la. Só o que o usuário
realmente editou passa por tradução reversa.

Guarde a tradução exibida ao renderizar a visão: ela é o `previousTranslatedCode`, e um baseline
errado faz o merge despejar texto traduzido dentro do original.

Use `TranslateFromNaturalLanguage` apenas quando não houver as três versões — por exemplo uma
conversão avulsa por linha de comando, aceitando a ambiguidade acima. Ela é estrutural: nasce de o
método receber uma única versão do código, e não de um defeito que possa ser corrigido.

## Criar novo IDE adapter

Para criar um adapter para outro IDE (IntelliJ, Sublime Text, Neovim, etc.):

1. **Spawnar o processo Core:**
   ```
   dotnet /path/to/MultiLingualCode.Core.Host.dll --method ... --params ...
   ```

2. **Parsear a resposta JSON do stdout**

3. **Implementar as features básicas:**
   - Abrir view com código traduzido, guardando o texto exibido como baseline
   - Interceptar save e chamar `ApplyTranslatedEdits` com as três versões
   - Toggle on/off
   - Seletor de idioma

4. **Opcional:**
   - Autocomplete com keywords traduzidas
   - Hover com keyword original
   - Syntax highlighting

A referência completa é a implementação VS Code em `packages/ide-adapters/vscode/`.
