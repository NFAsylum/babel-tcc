# Tarefa 111 - URI traduzida codifica o idioma-alvo

## Fase
6 - Extensao VS Code

## Objetivo
Corrigir o bug em que trocar de idioma nem sempre re-traduz o arquivo aberto, muda o arquivo em
foco e deixa uma aba traduzida anterior para fechar manualmente.

## Contexto
As visoes traduzidas sao documentos virtuais (FileSystemProvider) cuja URI NAO codificava o
idioma-alvo: `babel-tcc-translated:/path/file.cs` era a mesma URI para pt-br, en, etc. O idioma era
resolvido a parte, lendo da config dentro de `provideContent`/`buildCacheKey`.

O VS Code indexa a working copy de documentos editaveis de FileSystemProvider pela URI. Com a URI
identica entre idiomas, `openTextDocument(mesmaUri)` apos `close` devolve o documento em cache e nao
chama `readFile` de novo - o conteudo antigo persistia. O workaround anterior (close + invalidateCache
via changeEmitter + bump de mtime + reopen) e racy: o evento Changed dispara para uma URI cuja aba
acabou de fechar, entao o VS Code costuma ignora-lo. Daí o "as vezes nao re-traduz". Alem disso o loop
de refresh reabre as abas em sequencia e nunca restaura o foco original, entao com 2+ arquivos o foco
ia para a ultima aba reaberta.

Historico: os commits 9e03712 (applyEdit) e 570d2f4 (volta para close+reopen) ja oscilaram nesse
mecanismo. A raiz e a URI unica por arquivo.

## Decisao
Ver DT-011. Codificar o idioma-alvo na query da URI virtual (`?lang=<idioma>`) e fazer o provider ler
o idioma DA URI (com fallback para a config). Cada idioma vira um documento distinto -> reabrir e
sempre uma leitura fresca, sem depender de evento/timing.

Padrao seguro: manter UMA visao traduzida por arquivo. Na troca de idioma, abrir a URI do idioma novo
e fechar a aba do idioma antigo, restaurando o foco. Sem multiplas copias divergentes.

## Escopo
- `translatedContentProvider.ts`: helpers `buildTranslatedUri`/`getLanguageFromUri`; provider le o
  idioma via `getTargetLanguage(uri)` em `provideContent`, `buildCacheKey`, `doWriteFile`;
  `invalidateCache` preserva a query; novo `invalidatePath` para refrescar todas as visoes abertas de
  um arquivo quando o original muda no disco.
- `autoTranslateManager.ts`: construir URIs com idioma; `refreshTranslatedTabs` abre-novo -> fecha-antigo
  -> restaura foco (sem `invalidateAll`); `switchScheme` idem; `handleActiveEditorChange` usa
  `isAnyTranslatedTabOpenForPath` (uma visao por arquivo); `replaceOriginalsWithTranslated` usa URI com
  idioma.
- `extension.ts`: comandos open* constroem a URI com idioma; o file-watcher chama `invalidatePath`.
- Mock do vscode (testes): `Uri` passa a suportar query (`parse`, `from`, `with`, `toString`).

## Fora de escopo
Comparacao multi-idioma lado a lado (visoes secundarias em readonly). Fica como evolucao possivel
(monografia §4.9): o padrao seguro mantem uma visao editavel por arquivo.
