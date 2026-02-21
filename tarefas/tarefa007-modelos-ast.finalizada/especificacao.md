# Tarefa 007 - Modelos AST

## Fase
1 - Core Engine

## Objetivo
Criar a hierarquia de classes que representam a Abstract Syntax Tree do projeto.

## Escopo
- Implementar ASTNode (classe base abstrata) em Core/Models/AST/
  - Propriedades: StartPosition, EndPosition, StartLine, EndLine, Parent, Children
  - Metodos abstratos: ToCode(), Clone()
- Implementar KeywordNode (herda ASTNode)
  - KeywordId, OriginalKeyword
- Implementar IdentifierNode (herda ASTNode)
  - Name, IsTranslatable, TranslatedName
- Implementar LiteralNode (herda ASTNode)
  - Value, Type (String/Number/Boolean/Null), IsTranslatable
- Implementar ExpressionNode e StatementNode como containers
- Testes unitarios: criacao, clone, serializacao
