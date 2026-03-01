# Tarefa 051 - Investigacao: Traducao reversa e linhas adicionadas manualmente

## Prioridade: BAIXA (investigacao concluida, bug nao confirmado no Core)

## Descricao original

Ao editar um ficheiro traduzido (modo editavel do FileSystemProvider) e adicionar
uma linha nova em portugues (ex: `publico estatico texto teste = "oi";`), a traducao
reversa supostamente nao convertia a linha adicionada manualmente.

## Investigacao

Testes de debug com Roslyn mostraram que `DescendantTokens()` classifica correctamente
TODAS as palavras portuguesas como `IdentifierToken` — tanto em ficheiros sem linhas
novas quanto com linhas adicionadas — desde que o codigo tenha estrutura completa
(using, namespace, class).

O `ReverseSubstituteKeywords()` funciona correctamente para ambos os casos.
Testes unitarios adicionados confirmam que a traducao reversa converte todas as
keywords, incluindo linhas adicionadas manualmente.

## Causa provavel do problema original

O bug reportado ocorreu durante uma sessao onde tambem havia problemas com tabelas
de traducao em falta (directoria `translations/` inexistente). E possivel que o
comportamento observado tenha sido causado por esse problema e nao pelo Core.

## Resultado

- Bug no Core NAO confirmado
- Testes unitarios adicionados para `ReverseSubstituteKeywords` cobrindo o cenario
- Nenhuma alteracao necessaria no `CSharpAdapter.cs`

## Arquivos alterados

- `packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpAdapterTests.cs` (testes adicionados)
