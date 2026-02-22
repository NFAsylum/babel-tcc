# Adicionar Novo IDE

## Indice

- [Arquitetura Core <-> IDE](#arquitetura-core---ide)
- [Protocolo JSON stdin/stdout](#protocolo-json-stdinstdout)
- [Metodos disponiveis](#metodos-disponiveis)
- [Criar novo IDE adapter](#criar-novo-ide-adapter)

## Arquitetura Core <-> IDE

O Core Engine e independente do IDE. A comunicacao acontece via processo .NET que recebe requests JSON:

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

## Metodos disponiveis

| Metodo | Params | Descricao |
|--------|--------|-----------|
| `TranslateToNaturalLanguage` | sourceCode, fileExtension, targetLanguage | Traduz codigo para idioma natural |
| `TranslateFromNaturalLanguage` | translatedCode, fileExtension, sourceLanguage | Traduz de volta para linguagem de programacao |
| `ValidateSyntax` | sourceCode, fileExtension | Valida sintaxe do codigo |
| `GetSupportedLanguages` | (nenhum) | Retorna lista de idiomas suportados |

## Criar novo IDE adapter

Para criar um adapter para outro IDE (IntelliJ, Sublime Text, Neovim, etc.):

1. **Spawnar o processo Core:**
   ```
   dotnet /path/to/MultiLingualCode.Core.Host.dll --method ... --params ...
   ```

2. **Parsear a resposta JSON do stdout**

3. **Implementar as features basicas:**
   - Abrir view com codigo traduzido
   - Interceptar save para traduzir de volta
   - Toggle on/off
   - Seletor de idioma

4. **Opcional:**
   - Autocomplete com keywords traduzidas
   - Hover com keyword original
   - Syntax highlighting

A referencia completa e a implementacao VS Code em `packages/ide-adapters/vscode/`.
