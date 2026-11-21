# Definition of Done - Tarefa 124

## Codigo
- [x] `CoreBridge.logProcessEvent` descarta eventos do processo depois de `dispose()`
- [x] exit, error, stderr e o timeout do `quit` passam por ele

## Testes
- [x] Mock do processo guarda todos os handlers de `exit` e `error`
- [x] 5 testes: exit ainda logado com a extensao ativa; exit, error, stderr e timeout depois de
      `dispose()` nao escrevem no canal fechado
- [x] Teste de mutacao: 6 mudancas, todas quebram pelo menos um teste
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 345 passam (340 no `main`)
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Abrir um arquivo traduzido, parar a janela de teste e conferir que o `exthost.log` dela nao tem
      `Channel has been closed` vindo do `extension.js`

## Processo
- [x] Decisao e alternativas registradas na especificacao
- [x] `CHANGELOG.md` atualizado
- [x] Pasta da tarefa renomeada para `.finalizada`
