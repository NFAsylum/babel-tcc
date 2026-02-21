# Tarefa 023 - TranslatedContentProvider

## Fase
3 - VS Code Extension

## Objetivo
Implementar provider que mostra codigo traduzido no editor VS Code.

## Escopo
- Implementar TranslatedContentProvider em src/providers/
  - Implementar TextDocumentContentProvider para esquema multilingual://
  - Ler arquivo original do disco
  - Chamar CoreBridge.translateToNaturalLanguage()
  - Retornar codigo traduzido
- Cache de traducoes (invalidar ao editar)
- Evento onDidChange para refresh
- Fallback: se traducao falha, mostrar original
