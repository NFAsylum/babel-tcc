# Contexto - Tarefa 087

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json (configuracoes)
- packages/ide-adapters/vscode/src/services/configurationService.ts
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts
- packages/ide-adapters/vscode/src/providers/keywordMap.ts
- packages/ide-adapters/vscode/src/ui/statusBar.ts
- packages/ide-adapters/vscode/src/extension.ts (comando selectLanguage)

## Notas
- O Core Host nao precisa de mudancas — ja recebe idioma por request
- A mudanca e exclusivamente na extensao VS Code
- Retrocompativel: sem languageOverrides, comportamento identico ao atual
