# Contexto - Tarefa 089

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/src/providers/keywordMap.ts

## Notas
- Atualmente o KeywordMapService ja e instanciado uma unica vez no extension.ts
- O problema real e a invalidacao agressiva do cache em onDidChangeConfiguration
- Arquivos de traducao nao mudam durante uma sessao do VS Code — cache pode ser permanente
