# Definition of Done - Tarefa 120

## Correcao
- [x] Funcao de normalizacao criada, normalizando **apenas** a letra do drive
- [x] Funcao de comparacao usada nos 16 pontos listados na especificacao (4 comparacoes de URI,
      8 chaves de mapa e as 4 chaves de controle que a revisao do PR #186 encontrou: `saveQueue`,
      `processingUris` nas suas duas pontas e `seen`)
- [x] `closeTab` e `isAnyTranslatedTabOpenForPath` passam a comparar da mesma forma
- [x] Nenhuma comparacao e nenhuma chave de mapa sensivel a caixa sobrou nos providers
      (`translatedContentProvider.ts` e `autoTranslateManager.ts`): varredura do `src/` por
      `.path ===`, `toString()` usado como chave e `Map`/`Set` indexados por caminho
- [x] Registrado o que fica de fora, e por que: a **construcao** da URI traduzida a partir do caminho
      do original (`Uri.parse` em `extension.ts` e `autoTranslateManager.ts`) continua copiando a
      grafia que recebe. Esta tarefa faz a extensao **tolerar** as duas grafias; eliminar a segunda
      na origem e tarefa propria (ver especificacao, "Fora de escopo")

## Testes
- [x] Unitarios da normalizacao: drive maiusculo, drive minusculo, caminho sem drive, caminho ja
      normalizado, string vazia
- [x] Regressao: `isAnyTranslatedTabOpenForPath` encontra a aba quando ela esta em `/C:/` e a busca
      vem em `/c:/`
- [x] Regressao: `closeTab` fecha a aba com as duas grafias
- [x] Regressao, nas duas direcoes (`/C:` -> `/c:` e `/c:` -> `/C:`), para cada mapa do provider:
      `mtimeMap` (`invalidatePath` -> `stat`), `recentWrites`, `cache` (invalidacao por prefixo, e a
      cache gravada pelo save), `displayLanguages`, `renderedContent` (lida pelo save e gravada por
      ele) e a fila `saveQueue` (um save espera o outro)
- [x] Regressao, nas duas direcoes, no `AutoTranslateManager`: a trava `processingUris` entre a troca
      em lote e o evento de editor ativo, a deduplicacao `seen` do `refreshTranslatedTabs` e o
      `reloadTranslatedView`
- [x] Teste de mutacao: cada uma das 19 normalizacoes, revertida isoladamente, quebra pelo menos um
      teste (antes da revisao, reverter o `translatedContentProvider.ts` inteiro nao quebrava nenhum)
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 309 passam (266 no `main`)
- [x] `npm run lint` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] "Mostrar Codigo Original" abre o original e ele **permanece** aberto
- [x] O log nao mostra mais `AutoTranslate: replaced` logo apos `Showing original for`
- [x] Repetir abrindo e fechando a janela 3 vezes, dado que o defeito e intermitente
- [x] Troca de idioma com visao traduzida aberta continua recarregando no lugar (DT-011 nao regrediu)
- [x] Revalidado depois dos ajustes da revisao do PR #186: "Mostrar Codigo Original" permanece,
      troca de idioma recarrega, e salvar a visao traduzida grava o original

## Processo
- [x] `CHANGELOG.md` da extensao com entrada em `Fixed`
- [x] Branch propria, saindo do `main` atualizado
- [x] Commits em conventional commits com escopo `vscode`
- [x] Pasta da tarefa renomeada para `.finalizada`
