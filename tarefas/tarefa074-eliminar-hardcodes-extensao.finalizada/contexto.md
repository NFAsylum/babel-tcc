# Contexto - Tarefa 074

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma (mas simplifica todas as futuras adicoes de linguagem)

## Arquivos afetados
- packages/ide-adapters/vscode/src/services/languageDetector.ts
- packages/ide-adapters/vscode/src/extension.ts
- packages/ide-adapters/vscode/src/providers/hoverProvider.ts
- Novo: packages/ide-adapters/vscode/src/config/languages.ts (registro central)
- packages/ide-adapters/vscode/test/ (teste de consistencia)

## Notas
- O package.json e lido estaticamente pelo VS Code — nao pode ser gerado em runtime
- O registro central e para codigo TypeScript, nao para package.json
- O CONTRIBUTING.md e adding-new-language.md devem ser atualizados para referenciar o registro central
