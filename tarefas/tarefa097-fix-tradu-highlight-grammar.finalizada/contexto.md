# Contexto - Tarefa 097

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/syntaxes/mlc-csharp.tmLanguage.json
- packages/ide-adapters/vscode/syntaxes/mlc-python.tmLanguage.json

## Notas
- Bug existe desde a criacao das grammars — nunca funcionou
- PR #76 (tarefa 079) corrigiu o regex tradu mas nao a ordem dos patterns
- Identificado na review do PR #76 como "pre-existente" mas nao reportado
  como problema — deveria ter sido HIGH
- A grammar mlc-csharp teve keywords removidas no PR #96 (tarefa 086);
  agora tem apenas comments, strings, numbers, tradu-annotations
