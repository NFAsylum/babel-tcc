# Contexto - Tarefa 023

## Dependencias
- Tarefa 022 (CoreBridge)

## Bloqueia
- Tarefa 025 (EditInterceptor)
- Tarefa 026 (SaveHandler)

## Arquivos relevantes
- docs/plano-geral.txt linhas 1108-1161 (codigo TranslatedContentProvider)

## Notas
VS Code TextDocumentContentProvider registra um scheme customizado.
O provider e chamado quando vscode.workspace.openTextDocument(uri) usa scheme multilingual.
