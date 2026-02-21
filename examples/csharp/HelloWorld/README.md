# HelloWorld

Exemplo basico demonstrando o uso de anotacoes `// tradu:` para traduzir um programa simples.

## Conceitos demonstrados

- Traducao de identificadores simples (`greeting` -> `saudacao`)
- Traducao de parametros de metodo (`args` -> `argumentos`)
- Traducao de strings literais
- Keywords traduzidas (`if`/`else` -> `se`/`senao`, `for` -> `para`)

## Como testar

```bash
dotnet run --project packages/core/MultiLingualCode.Core.Host -- \
  --method TranslateToNaturalLanguage \
  --params '{"sourceCode":"<conteudo de Program.cs>","fileExtension":".cs","targetLanguage":"pt-br"}' \
  --translations packages/core/MultiLingualCode.Core.Tests/TestData/translations
```
