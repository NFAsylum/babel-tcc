# Definition of Done - Tarefa 122

- [x] Suite da extensao passa sem `dotnet` no PATH
      (2026-10-12: PATH so com o `node`, `vitest run` - 229 de 229, 0 erros, exit 0. Antes,
      so `test/extension.test.ts`: 15 erros nao tratados, exit 1)
- [x] Nenhum teste executa o `dotnet` real
      (2026-10-12: `dotnet` falso na frente do PATH, suite inteira - 0 chamadas registradas.
      Antes: 15 por execucao de `test/extension.test.ts`)
- [x] `checkDotnetInstalled` coberta: sondagem usada, runtime 8 presente, `dotnet` ausente,
      sondagem com erro, so outra versao instalada, botao de download, mensagem dispensada
      (2026-10-12: 7 testes novos em `describe('.NET runtime check')`)
- [x] Os testes novos detectam erro no codigo de producao
      (2026-10-12: 6 mutacoes em `checkDotnetInstalled`, 6 detectadas - sondagem trocada para
      `--version`, regex de 8 para 9, erro da sondagem ignorado, checagem do runtime
      invertida, checagem do botao invertida, URL trocada)
- [x] Suite verde (`npx vitest run`) - 229 passam (baseline 222), em 3 execucoes seguidas
- [x] `npm run lint` limpo e `npm run build` passando
- [x] Nenhuma mudanca em `src/` (`git diff --stat origin/main -- packages/ide-adapters/vscode/src`
      vazio)
- [x] Tarefa marcada como .finalizada no mesmo PR
