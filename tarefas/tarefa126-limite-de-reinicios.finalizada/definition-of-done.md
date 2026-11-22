# Definition of Done - Tarefa 126

## Codigo
- [x] `startProcess` nao zera mais o `crashCount`; so uma resposta valida do protocolo zera (uma linha
      de texto qualquer, como o erro do `dotnet`, nao)
- [x] No limite, aviso uma vez so, com o botao "Abrir Log"; requisicoes seguintes recusadas sem aviso
- [x] `ensureProcess` devolve um booleano, sem `throw`

## Testes
- [x] Teste antigo (contador forcado na mao) trocado por 5 testes com quedas de verdade: para depois de
      3 seguidas, avisa uma vez com o botao, o botao abre o log, uma resposta encerra a sequencia, uma
      linha que nao e resposta nao encerra
- [x] Teste de mutacao: 9 mudancas, todas quebram pelo menos um teste
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 349 passam (345 na tarefa124)
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Com a pasta `bin` renomeada, o aviso aparece uma vez, o botao abre o log, e o log mostra 3
      reinicios e depois "not restarting"
- [x] Com a pasta `bin` restaurada, a traducao volta a funcionar normalmente

## Processo
- [x] Decisao registrada na especificacao
- [x] `CHANGELOG.md` atualizado
- [x] Pasta da tarefa renomeada para `.finalizada`
