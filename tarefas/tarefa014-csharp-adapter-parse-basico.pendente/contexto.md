# Contexto - Tarefa 014

## Dependencias
- Tarefa 006 (ILanguageAdapter)
- Tarefa 007 (modelos AST)
- Tarefa 005 (keywords-base.json)

## Bloqueia
- Tarefa 015 (Generate depende de Parse)
- Tarefa 016 (Validacao)
- Tarefa 017 (deteccao tradu)
- Tarefa 018 (recursos avancados)

## Arquivos relevantes
- docs/plano-geral.txt linhas 670-778 (codigo CSharpAdapter)
- docs/decisoes-tecnicas.md (DT-001 Roslyn)

## Notas
Pacote: Microsoft.CodeAnalysis.CSharp (Roslyn).
O Parse converte de Roslyn SyntaxNode para nossa ASTNode customizada.
Comecar com construtos simples e expandir na Tarefa 018.
