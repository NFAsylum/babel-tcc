# Tarefa 111 - Troca de idioma re-traduz a visao no lugar

## Fase
6 - Extensao VS Code

## Objetivo
Corrigir o bug em que trocar de idioma nem sempre re-traduz o arquivo aberto, muda o arquivo em
foco e deixa uma aba traduzida anterior para fechar manualmente.

## Contexto
As visoes traduzidas sao documentos virtuais (FileSystemProvider) com uma unica URI por arquivo. A
causa real do bug estava no `stat()`: ele devolvia `size` = tamanho do arquivo ORIGINAL, que nao muda
entre idiomas e nao bate com o conteudo traduzido que o `readFile` devolve. A doc do
`FileSystemProvider.onDidChangeFile` exige `mtime` que avanca E `size` correto, senao o VS Code aplica
uma otimizacao que IGNORA o evento de mudanca e nao recarrega o editor. Por isso o
`changeEmitter`/reopen do codigo anterior era racy, e o time apelou para fechar+reabrir aba, que e
fragil (tabGroups.close com referencia guardada falha - microsoft/vscode#242867), deixando aba antiga
aberta.

## Decisao
Ver DT-011. Atualizar o conteudo NO LUGAR, sem mexer em aba: corrigir o `stat()` para devolver o
tamanho real do conteudo traduzido + `mtime` que avanca, e na troca de idioma disparar
`onDidChangeFile` para a URI aberta. O VS Code recarrega o `readFile` na mesma aba -> re-traduz, foco
nao muda, nenhuma aba sobra.

(Antes tentou-se codificar o idioma na query da URI - `?lang=` - mas isso FORCA fechar+reabrir aba, a
operacao fragil; deixava as duas abas abertas. Revertido.)

## Escopo
- `translatedContentProvider.ts`: `stat()` devolve `size` do conteudo traduzido + `mtime` que avanca;
  `invalidatePath` limpa o cache do arquivo e dispara `onDidChangeFile` para as visoes abertas;
  `doWriteFile` descarta todos os idiomas em cache do arquivo ao salvar e refresca o atual.
- `autoTranslateManager.ts`: `refreshTranslatedTabs` so chama `invalidatePath(caminho)` por arquivo
  aberto (sem abrir/fechar/refocar); `handleActiveEditorChange` mantem uma visao por arquivo via
  `isAnyTranslatedTabOpenForPath`; fechamentos de aba (toggle on/off e readonly) usam `closeTab(uri)`
  por busca fresca, nao por referencia guardada.
- `extension.ts`: o file-watcher chama `invalidatePath` quando o original muda no disco.

## Fora de escopo
- Comparacao multi-idioma lado a lado (visoes secundarias em readonly) - evolucao possivel.
- O toggle editavel <-> readonly continua reabrindo a aba (o "ser readonly" esta atrelado ao esquema
  da URI), mas com `closeTab(uri)` por busca fresca.
