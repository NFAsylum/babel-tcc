# Definition of Done - Tarefa 115

## Empacotamento
- [x] `scripts/copy-examples.js` copia `examples/` para dentro da extensao e falha alto sem origem
- [x] Encadeado no `prepackage` e no `release.yml`
- [x] Pasta de destino no `.gitignore` e fora do `.vscodeignore`
- [x] `npx vsce ls` lista os 19 arquivos de exemplo

## Comando
- [x] `babel-tcc.openExample` com `category`, `icon` e titulo nos 7 nls
- [x] QuickPick com titulo, placeholder e um item por exemplo do catalogo, so texto
- [x] Copia para `globalStorageUri/examples/<versao>/`, sem sobrescrever copia existente
- [x] Abre a visao traduzida (editavel ou somente leitura) ou o original com a traducao desligada,
      em aba permanente
- [x] Falha de disco vira mensagem com botao "Abrir Log"
- [x] Atalho `Ctrl+Alt+U` `X`
- [x] 14 frases novas nos 6 bundles (o teste de contrato dos bundles cobre)

## Testes
- [x] `test/commands/openExample.test.ts`: catalogo (arquivos existem no repositorio, linguagem
      suportada, as 5 extensoes cobertas, sem repeticao), picker, cancelamento, copia, copia existente
      preservada, os tres modos de abertura, falha com e sem o botao, copia sempre em URI `file:`
- [x] Teste de mutacao: 11 mudancas no comando, todas quebram pelo menos um teste
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 284 passam (266 no `main`)
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] "Babel TCC: Abrir Exemplo" sem nenhuma pasta aberta: a lista aparece com os 6 exemplos
- [x] Escolher um exemplo abre a visao traduzida
- [x] Editar, salvar, fechar e abrir o mesmo exemplo de novo: a edicao continua la
- [x] Com a traducao desligada, o exemplo abre no original
- [x] `Ctrl+Alt+U` `X` abre a lista
- [x] Exemplo aberto com a traducao desligada e sem pasta aberta: religar a traducao troca a aba
      pela visao traduzida

## Processo
- [x] Decisao de onde guardar a copia registrada na especificacao (vira DT-015 depois do merge da 117)
- [x] `CHANGELOG.md` e `docs/user-guide/getting-started.md` atualizados
- [x] Pasta da tarefa renomeada para `.finalizada`
