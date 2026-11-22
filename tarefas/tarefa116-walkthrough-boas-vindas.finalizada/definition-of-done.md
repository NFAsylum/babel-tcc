# Definition of Done - Tarefa 116

## Walkthrough
- [x] `contributes.walkthroughs` com 4 passos, midia em Markdown, titulos e descricoes nos 7 nls
- [x] Cada descricao termina com um botao que faz a acao e conclui o passo
- [x] Passos que abrem editor usam `command:toSide:`
- [x] O passo do `tradu` diz que so C# e Python tem o recurso
- [x] Comando "Abrir Introducao" (`babel-tcc.openWalkthrough`), com `category`, titulo nos 7 nls
- [x] `npx vsce ls` lista os 4 arquivos de midia

## Testes
- [x] Contrato do walkthrough no teste do manifesto (id, midia, botao nos 7 nls, destino dos links,
      `toSide:`, eventos de conclusao)
- [x] `test/commands/openWalkthrough.test.ts` e `test/commands/showOriginal.test.ts`
- [x] Exemplo aberto no grupo decidido antes da lista, nunca por cima da pagina; original aberto no
      grupo da visao traduzida
- [x] Passo "Ligue a traducao" so com a traducao desligada, primeiro da lista, unico com o botao de
      alternar
- [x] Teste de mutacao: 27 mudancas (12 do walkthrough, 15 das correcoes do F5), todas quebram pelo
      menos um teste
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 306 passam (284 na tarefa115)
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] "Babel TCC: Abrir Introducao" abre a pagina com os 4 passos
- [x] Com a traducao desligada, aparece primeiro o passo "Ligue a traducao"; o botao liga, e o passo
      some; com a traducao ligada, ele nao aparece
- [x] Passo 1: o botao abre a escolha de idioma, e o passo fica marcado
- [x] Passo 2: o botao abre o exemplo ao lado da pagina, ja traduzido
- [x] Passo 2 de novo, com um exemplo ja aberto ao lado: o segundo tambem abre ao lado, e o passo 3
      mostra o original dele
- [x] Passo 3: depois de editar e salvar, o botao mostra o original, e o passo fica marcado
- [x] Passo 4: o botao abre a secao do tradu no navegador, e o passo fica marcado
- [x] O walkthrough inteiro funciona so com `Tab` e `Enter`
- [x] A pagina aparece traduzida com a interface do VS Code em portugues

## Processo
- [x] Decisoes registradas na especificacao
- [x] `CHANGELOG.md` e `docs/user-guide/getting-started.md` atualizados
- [x] Pasta da tarefa renomeada para `.finalizada`
