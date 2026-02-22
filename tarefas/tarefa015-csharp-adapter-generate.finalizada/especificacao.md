# Tarefa 015 - CSharpAdapter: Generate (AST -> codigo)

## Fase
2 - Language Adapters

## Objetivo
Implementar a geracao de codigo C# a partir da AST customizada.

## Escopo
- Implementar Generate(ASTNode ast):
  - Percorrer AST e gerar texto C#
  - Preservar indentacao e formatacao original
  - Preservar comentarios
  - Preservar espacos em branco significativos
- Implementar round-trip: Parse -> Generate deve produzir codigo identico
- Testes de round-trip com multiplos exemplos
