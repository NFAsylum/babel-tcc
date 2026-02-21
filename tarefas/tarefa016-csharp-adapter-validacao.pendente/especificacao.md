# Tarefa 016 - CSharpAdapter: Validacao e diagnosticos

## Fase
2 - Language Adapters

## Objetivo
Implementar validacao de sintaxe e extracao de diagnosticos via Roslyn.

## Escopo
- Implementar ValidateSyntax(string sourceCode):
  - Usar Roslyn para detectar erros de sintaxe
  - Retornar ValidationResult com lista de erros
  - Incluir posicao (linha/coluna) dos erros
- Mapear DiagnosticSeverity do Roslyn para nosso modelo
- Testes com codigo valido e invalido
