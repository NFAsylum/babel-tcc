# Tarefa 091 - Preservar encoding original do arquivo no save

## Prioridade: LOW

## Problema
O `translatedContentProvider.ts` usa `TextEncoder` ao escrever o arquivo de volta no disco, o que forca UTF-8:

```typescript
const encoder: TextEncoder = new TextEncoder();
await vscode.workspace.fs.writeFile(originalUri, encoder.encode(originalCode));
```

Se o arquivo original era Latin-1, Shift-JIS, GB2312, ou qualquer encoding nao-UTF-8, o save sobrescreve com UTF-8. Os caracteres sao preservados (passam por JavaScript string -> UTF-8 -> Core -> UTF-8 -> disco), mas o encoding do arquivo muda. Projetos legados que dependem de encoding especifico podem quebrar.

## Solucao
Usar a API do VS Code para respeitar o encoding configurado:

1. Detectar o encoding do arquivo original antes de ler (via `files.encoding` na workspace settings ou auto-detection)
2. Ao escrever de volta, usar o mesmo encoding detectado
3. Alternativa: usar `vscode.workspace.openTextDocument` + `vscode.workspace.applyEdit` que respeitam o encoding configurado, em vez de `vscode.workspace.fs.writeFile` com `TextEncoder`

## Impacto
Baixo na pratica — a maioria do codigo moderno e UTF-8 e o VS Code salva em UTF-8 por padrao. Afeta projetos legados com encoding especifico.
