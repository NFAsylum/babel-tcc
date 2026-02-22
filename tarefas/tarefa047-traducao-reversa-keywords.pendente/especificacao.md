# Tarefa 047 - Traducao reversa de keywords [CRITICA]

## Fase
BLOQUEANTE - Deve ser resolvida antes do deploy

## Objetivo
Corrigir o bug critico onde `TranslateFromNaturalLanguage` nao reverte keywords traduzidas para o C# original. Atualmente o codigo traduzido entra e sai identico, sem nenhuma reversao.

## Problema

Quando o usuario salva codigo traduzido (ex: `usando`, `classe`, `se`), o SaveHandler chama `TranslateFromNaturalLanguage` para reverter para C# (`using`, `class`, `if`). Porem a reversao nao funciona - o codigo sai identico ao que entrou.

### Causa raiz

1. `TranslateFromNaturalLanguage` parseia o codigo traduzido com Roslyn (`CSharpAdapter.Parse`)
2. Roslyn nao reconhece keywords traduzidas (`usando`, `classe`, `se`) como keywords C#
3. Roslyn classifica essas palavras como `IdentifierToken` (nao `KeywordToken`)
4. O `CSharpAdapter.Parse` cria `IdentifierNode` em vez de `KeywordNode` para elas
5. `TranslateAstReverse` procura `KeywordNode` para reverter keywords, mas nao encontra nenhum
6. O caso `IdentifierNode` em `TranslateAstReverse` so consulta `IdentifierMapper` (para `// tradu:`), nao o mapeamento de keywords
7. Resultado: nenhuma keyword e revertida

### Reproducao

```bash
dotnet run --project packages/core/MultiLingualCode.Core.Host -- \
  --method TranslateFromNaturalLanguage \
  --params '{"translatedCode":"usando System;\nespaconome HelloWorld\n{\n    classe Program\n    {\n    }\n}","fileExtension":".cs","sourceLanguage":"pt-br"}' \
  --translations packages/core/MultiLingualCode.Core.Tests/TestData/translations \
  --project .
```

Output atual: codigo portugues identico ao input (keywords nao revertidas)
Output esperado: `using System;\nnamespace HelloWorld\n{\n    class Program\n    {\n    }\n}`

## Solucao proposta

A traducao reversa NAO pode depender do Roslyn para classificar tokens traduzidos como keywords. Em vez disso, usar substituicao textual baseada no mapeamento reverso de keywords.

### Abordagem: substituicao textual pre-Roslyn

1. Antes de parsear com Roslyn, fazer substituicao textual das keywords traduzidas para keywords C# originais
2. Usar o mapeamento reverso da `LanguageTable` (traducao -> keyword original)
3. Substituir usando word boundaries para evitar substituicoes parciais (ex: nao substituir "classe" dentro de "subclasse")
4. Apos a substituicao textual, o codigo volta a ser C# valido
5. Parsear com Roslyn normalmente para reverter identificadores e literals

### Implementacao

Em `TranslationOrchestrator.TranslateFromNaturalLanguageAsync`:
1. Carregar tabela de traducao
2. Construir mapeamento reverso: `{traducao -> keyword_original}` (ex: `"usando" -> "using"`, `"classe" -> "class"`)
3. Ordenar por comprimento decrescente (evitar substituicoes parciais - "espaconome" antes de "nome")
4. Aplicar substituicoes no texto com regex de word boundary (`\b`)
5. Agora o texto e C# valido - parsear com Roslyn
6. Reverter identificadores e literals via AST normalmente

### Testes necessarios

- `TranslateFromNaturalLanguage_TranslatedKeywords_RevertsToCSharp`
- `TranslateFromNaturalLanguage_MixedKeywordsAndIdentifiers_RevertsAll`
- `RoundTrip_ForwardThenReverse_ProducesOriginalCode`
- `TranslateFromNaturalLanguage_PartialWordMatch_DoesNotReplace` (ex: "classement" nao deve virar "classment")
- `TranslateFromNaturalLanguage_KeywordsInStrings_NotReplaced`
- `TranslateFromNaturalLanguage_KeywordsInComments_NotReplaced`

## Impacto

Sem esta correcao:
- O SaveHandler salva codigo em portugues no disco, quebrando compilacao
- A feature "editar codigo traduzido" e inutilizavel
- O round-trip (principio fundamental do projeto) nao funciona

## Arquivos afetados

- `Core/Services/TranslationOrchestrator.cs` - logica principal da reversao
- `Core.Tests/Services/TranslationOrchestratorTests.cs` - novos testes
- `Core.Tests/Integration/CoreIntegrationTests.cs` - testes de round-trip
