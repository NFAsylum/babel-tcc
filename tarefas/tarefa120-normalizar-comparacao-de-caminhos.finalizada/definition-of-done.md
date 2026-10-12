# Definition of Done - Tarefa 120

## Correcao
- [x] Funcao de normalizacao criada, normalizando **apenas** a letra do drive
- [x] Funcao de comparacao usada nos 12 pontos listados na especificacao (4 comparacoes de URI
      e 8 chaves de mapa)
- [x] `closeTab` e `isAnyTranslatedTabOpenForPath` passam a comparar da mesma forma
- [x] Nenhuma comparacao de caminho sensivel a caixa sobrou no `src/`
      (`grep -rn "uri.path ===\|uri.toString() ===" src/` volta so os usos ja normalizados)

## Testes
- [x] Unitarios da normalizacao: drive maiusculo, drive minusculo, caminho sem drive, caminho ja
      normalizado, string vazia
- [x] Regressao: `isAnyTranslatedTabOpenForPath` encontra a aba quando ela esta em `/C:/` e a busca
      vem em `/c:/`
- [x] Regressao: `closeTab` fecha a aba com as duas grafias
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 224 passam (baseline 207 nesta branch, que sai do
      `main` e portanto nao tem o trabalho da tarefa113)
- [x] `npm run lint` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] "Mostrar Codigo Original" abre o original e ele **permanece** aberto
- [x] O log nao mostra mais `AutoTranslate: replaced` logo apos `Showing original for`
- [x] Repetir abrindo e fechando a janela 3 vezes, dado que o defeito e intermitente
- [x] Troca de idioma com visao traduzida aberta continua recarregando no lugar (DT-011 nao regrediu)

## Processo
- [x] Branch propria, saindo do `main` atualizado
- [x] Commits em conventional commits com escopo `vscode`
- [x] Pasta da tarefa renomeada para `.finalizada`
