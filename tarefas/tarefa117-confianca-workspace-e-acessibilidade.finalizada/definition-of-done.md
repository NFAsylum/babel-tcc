# Definition of Done - Tarefa 117

## Confianca do workspace
- [x] `capabilities.untrustedWorkspaces.supported` = `"limited"`, com descricao nos 7 nls
- [x] `babel-tcc.translationsPath` em `restrictedConfigurations`
- [x] Pasta `babel-tcc-translations` ao lado do workspace so detectada com o workspace confiavel
- [x] Core e checagem do .NET iniciados com `cwd` na pasta da extensao
- [x] `capabilities.virtualWorkspaces.supported` = `false`, com descricao nos 7 nls

## Acessibilidade do status bar
- [x] `name` = "Babel TCC"
- [x] `accessibilityInformation` com papel `button` e rotulo em frase completa, nos dois estados
- [x] Rotulo traduzido nos 6 bundles de runtime (o teste de contrato dos bundles cobre)

## Log
- [x] "restarting process after crash" so aparece depois de um crash de verdade

## Testes
- [x] Manifesto: `limited`, `translationsPath` restrita, so configuracoes declaradas sao restritas,
      virtual nao suportado
- [x] `coreBridge`: pasta irma ignorada sem confianca; `cwd` do Core; sem mensagem de crash na
      primeira inicializacao; com mensagem depois de um crash
- [x] `extension`: checagem do .NET com `cwd` na pasta da extensao
- [x] `statusBar`: `name`, papel `button`, rotulo sem codicon nos dois estados
- [x] Teste de mutacao: cada uma das 11 mudancas, revertida isoladamente, quebra pelo menos um teste
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 279 passam (266 no `main`)
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Pasta fora de `C:\Users\klebe` (que ja e confiavel e passa a confianca para as subpastas),
      aberta respondendo "nao confio": a extensao ativa e traduz (zh-cn e it-it), "Mostrar Codigo
      Original" funciona, e o log mostra "workspace not trusted, not looking for a translations folder
      next to it" e "using embedded translations"
- [x] Workspace confiavel (`examples`): comportamento de antes, validado no F5 do PR #186
- [x] O menu de contexto da barra de status lista "Babel TCC"
- [ ] Anuncio do leitor de tela (Narrador ou NVDA) conferido - pendente, entra no roteiro da
      auditoria de acessibilidade (tarefa119)
- [x] O log da ativacao nao mostra mais "restarting process after crash"
- [x] Observado e documentado: em Modo Restrito o VS Code poe caixa em todo caractere fora do ASCII
      basico (`editor.unicodeHighlight.nonBasicASCII`), entao uma visao em chines aparece em caixas.
      Protecao do proprio VS Code; a extensao nao a desliga (DT-014 e guia de solucao de problemas)

## Processo
- [x] DT-014 registrado em `docs/decisoes-tecnicas.md`
- [x] `CHANGELOG.md` atualizado
- [x] `docs/user-guide/troubleshooting.md` com a secao de Modo Restrito
- [x] Pasta da tarefa renomeada para `.finalizada`
