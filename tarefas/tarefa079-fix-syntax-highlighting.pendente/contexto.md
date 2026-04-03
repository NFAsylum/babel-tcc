# Contexto - Tarefa 079

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/syntaxes/mlc-csharp.tmLanguage.json
- packages/ide-adapters/vscode/syntaxes/mlc-python.tmLanguage.json

## Notas
- TextMate grammars sao estaticas — nao podem mudar keywords por idioma em runtime
- A solucao definitiva para keywords dinamicas por idioma seria semantic tokens (VS Code API), mas e muito mais complexo
- Para esta tarefa, alinhar com pt-br-ascii e documentar a limitacao
