# Contexto - Tarefa 124

## Dependencias
- Tarefa117 (mergeada no #193): mexeu no mesmo trecho do `coreBridge.ts`. Esta tarefa sai do `main`
  depois dela.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/services/coreBridge.ts (`startProcess`, `dispose`)
- packages/ide-adapters/vscode/src/extension.ts (`deactivate`, ordem das `subscriptions`)
- Log do VS Code: `%APPDATA%\Code\logs\<sessao>\window<N>\exthost\exthost.log`

## Notas
- Como conferir: rodar a extensao no F5, abrir um arquivo traduzido (para o Core subir), parar ou
  recarregar a janela de teste e procurar `Channel has been closed` no `exthost.log` dessa janela.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism`.
