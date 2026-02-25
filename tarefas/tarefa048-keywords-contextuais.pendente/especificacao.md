# Tarefa 048 - Adicionar Keywords Contextuais C#

## Prioridade: ALTA

## Problema

O `CSharpKeywordMap` atualmente contém apenas 77 keywords reservadas do C#, mas ignora
keywords contextuais extremamente comuns em codigo C# moderno:

- `var` - usado em praticamente todo codigo C# contemporaneo
- `async` / `await` - padrao para codigo assincrono
- `yield` - usado em iterators
- `record` - tipo moderno do C#
- `partial` - classes e metodos parciais
- `where` - constraints de generics e LINQ
- `dynamic` - tipagem dinamica
- `nameof` - operador de nome
- `init` - property initializers
- `required` - campos obrigatorios (C# 11)
- `global` - using global

Sem essas keywords, codigo como `var x = await GetDataAsync()` nao traduz `var` nem `await`.

## Solucao

1. Adicionar keywords contextuais ao `CSharpKeywordMap.TextToId`
   - Preencher o gap do ID 74
   - Continuar a numeracao a partir de 78

2. Atualizar `RoslynWrapper.IsKeywordKind` para reconhecer os `SyntaxKind` contextuais:
   - `SyntaxKind.VarKeyword` (nota: Roslyn trata var como contextual)
   - `SyntaxKind.AsyncKeyword`
   - `SyntaxKind.AwaitKeyword`
   - `SyntaxKind.YieldKeyword`
   - `SyntaxKind.PartialKeyword`
   - etc.

3. Verificar se `CSharpAdapter.Parse` reconhece tokens contextuais
   - Roslyn pode classificar `var` como identifier em certos contextos
   - Pode ser necessario logica adicional para detectar keywords contextuais

4. Atualizar tabela de traducao PT-BR com as novas keywords

5. Atualizar testes

## Arquivos afetados

- `packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpKeywordMap.cs`
- `packages/core/MultiLingualCode.Core/Utilities/RoslynWrapper.cs`
- `packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs`
- `packages/core/MultiLingualCode.Core.Tests/` (varios)
- `babel-tcc-translations` repo (tabela PT-BR)

## Dependencias

- Nenhuma (pode ser feita independentemente)
- Complementa tarefa 047 (traducao reversa)
