# Tarefa 028 - CompletionProvider e HoverProvider

## Fase
3 - VS Code Extension

## Objetivo
Implementar autocomplete e hover com informacoes de traducao.

## Escopo
- Implementar CompletionProvider em src/providers/
  - Autocomplete de keywords traduzidas
  - Sugestoes de identificadores comuns traduzidos
  - Integracao com CoreBridge para sugestoes contextuais
- Implementar HoverProvider em src/providers/
  - Ao passar mouse sobre keyword traduzida: mostrar keyword original
  - Ao passar sobre identificador: mostrar nome original + tipo
- Testes
