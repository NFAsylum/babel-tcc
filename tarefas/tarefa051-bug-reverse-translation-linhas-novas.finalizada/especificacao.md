# Tarefa 051 - Bug: Traducao reversa falha em codigo complexo

## Prioridade: ALTA

## Descricao

Ao salvar um ficheiro traduzido (modo editavel), a traducao reversa falhava
em converter algumas keywords traduzidas de volta para C#. O resultado era
codigo C# invalido com keywords em portugues misturadas com codigo ingles.

## Causa raiz

`CSharpAdapter.ReverseSubstituteKeywords()` usava Roslyn para parsear o codigo
traduzido e iterar `DescendantTokens()` a procura de `IdentifierToken`. Porem,
o Roslyn nao consegue parsear codigo com keywords em portugues de forma fiavel.
O error recovery do Roslyn absorve tokens em `SkippedTokensTrivia`, tornando-os
invisiveis ao `DescendantTokens()`.

Tokens perdidos pelo Roslyn em ficheiros complexos: `classe`, `somenteleitura`,
`se`, `booleano`, e outros dependendo da estrutura do codigo.

## Solucao

Substituir a abordagem Roslyn por scan textual com word boundaries. O novo
metodo percorre o codigo caracter a caracter, identifica palavras e verifica
se sao keywords traduzidas via `lookupTranslatedKeyword`. Ignora conteudo
dentro de strings (regulares e verbatim), char literals, comentarios de linha
e comentarios de bloco para evitar falsos positivos.

## Arquivos alterados

- `packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs` (ReverseSubstituteKeywords reescrito)
- `packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpAdapterTests.cs` (9 testes adicionados)
