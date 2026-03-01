# Tarefa 051 - Bug: Traducao reversa falha em linhas adicionadas manualmente

## Prioridade: ALTA

## Descricao

Ao editar um ficheiro traduzido (modo editavel do FileSystemProvider) e adicionar
uma linha nova em portugues (ex: `publico estatico texto teste = "oi";`), a traducao
reversa converte correctamente todas as linhas originais mas **nao converte a linha
adicionada manualmente**. O resultado e codigo C# invalido com uma linha em pt-br.

## Causa raiz

`CSharpAdapter.ReverseSubstituteKeywords()` usa Roslyn para parsear o codigo traduzido
antes de identificar quais tokens sao keywords traduzidas:

```csharp
// CSharpAdapter.cs:171-196
SyntaxTree tree = RoslynWrapper.ParseSourceCode(translatedCode);
SyntaxNode root = RoslynWrapper.GetRoot(tree);

foreach (SyntaxToken token in root.DescendantTokens())
{
    if (!token.IsKind(SyntaxKind.IdentifierToken))
        continue;

    int keywordId = lookupTranslatedKeyword(token.Text);
    // ...
}
```

O Roslyn nao reconhece keywords em portugues como keywords C#, classificando-as como
`IdentifierToken`. Para o codigo gerado pelo Core isto funciona porque a estrutura
sintatica e preservada. Porem, uma linha adicionada manualmente pode gerar erros de
parsing no Roslyn, fazendo com que os tokens dessa linha nao sejam emitidos como
`IdentifierToken` e a substituicao reversa falhe.

## Reproducao

1. Abrir um ficheiro .cs com a extensao Babel TCC
2. Usar "Traduzir Codigo (Editavel)"
3. Adicionar uma linha nova no ficheiro traduzido (ex: `publico estatico texto teste = "oi";`)
4. Salvar (Ctrl+S)
5. Verificar o ficheiro .cs original — a linha nova permanece em portugues

## Possivel solucao

Combinar a abordagem baseada em Roslyn com um fallback de substituicao textual
(regex com word boundaries) para tokens que o Roslyn nao conseguiu classificar.
Ou fazer uma primeira passagem textual de keywords ANTES do parsing Roslyn.

## Arquivos afetados

- `packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs` (ReverseSubstituteKeywords)
- Testes correspondentes
