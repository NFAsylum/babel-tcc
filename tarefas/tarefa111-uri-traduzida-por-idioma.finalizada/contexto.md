# Contexto - Tarefa 111

## Dependencias
Nenhuma.

## Bloqueia
Nenhuma.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts
- packages/ide-adapters/vscode/src/providers/autoTranslateManager.ts
- packages/ide-adapters/vscode/src/extension.ts (file-watcher + comandos open*)
- packages/ide-adapters/vscode/test/__mocks__/vscode.ts (Uri com query)
- packages/ide-adapters/vscode/test/providers/translatedContentProvider.test.ts
- packages/ide-adapters/vscode/test/providers/autoTranslateManager.test.ts
- docs/decisoes-tecnicas.md (DT-011)

## Notas
- O changeEmitter/mtime continuam necessarios para o caso "original mudou no disco" (invalidatePath
  refresca a visao ABERTA - funciona porque a aba esta visivel). O bug era so o close+reopen da mesma
  URI na troca de idioma, que a URI por idioma elimina.
- `Uri.file(uri.path)` continua mapeando para o arquivo original: a query nao afeta `.path`, entao a
  traducao reversa (salvar) segue acertando o original.
- Reportado pelo uso: trocar idioma as vezes nao re-traduzia, trocava o arquivo em foco e deixava a
  aba traduzida anterior aberta.
