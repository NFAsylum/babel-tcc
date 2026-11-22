# Contexto - Tarefa 126

## Dependencias
- Tarefa124 (PR #196): mexe no mesmo trecho do `coreBridge.ts`. Esta branch sai da branch da 124
  (`fix/log-ao-desligar`); o PR mostra os commits dela ate o #196 ser mergeado.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/services/coreBridge.ts (`startProcess`, `ensureProcess`,
  `sendRequest`)
- tarefas/tarefa117-confianca-workspace-e-acessibilidade.finalizada/especificacao.md (onde o defeito
  foi registrado)

## Notas
- Como reproduzir no F5: renomear temporariamente `packages/ide-adapters/vscode/bin` (onde fica o
  Core). O `dotnet` nao encontra a DLL e o processo sai logo ao iniciar, a cada tentativa.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism`.
