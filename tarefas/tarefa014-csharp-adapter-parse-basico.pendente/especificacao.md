# Tarefa 014 - CSharpAdapter: Parse basico com Roslyn

## Fase
2 - Language Adapters

## Objetivo
Implementar o parsing basico de codigo C# usando Roslyn, convertendo para nossa AST.

## Escopo
- Adicionar pacote NuGet Microsoft.CodeAnalysis.CSharp
- Implementar CSharpAdapter : ILanguageAdapter em Core/LanguageAdapters/
- Implementar Parse(string sourceCode):
  - Usar CSharpSyntaxTree.ParseText() para obter AST Roslyn
  - Converter SyntaxNode Roslyn -> nossa hierarquia ASTNode
  - Suportar: keywords (if, else, for, while, class, namespace, using, etc.)
  - Suportar: declaracoes de variaveis, metodos, classes
  - Preservar posicoes (linha/coluna) nos nodes
- Implementar GetKeywordMap(): carregar de keywords-base.json
- Implementar ExtractIdentifiers(): usar Roslyn para encontrar todos IdentifierNameSyntax
- Testes com codigo C# simples (HelloWorld, declaracoes basicas)
