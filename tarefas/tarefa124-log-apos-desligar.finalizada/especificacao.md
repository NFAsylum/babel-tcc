# Tarefa 124 - Nao escrever no canal de log depois de desligar

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Eliminar o erro nao tratado `Error: Channel has been closed` que a extensao deixa no log do VS Code
(`exthost.log`) toda vez que e desligada ou recarregada.

## Contexto
Achado ao ler os logs do VS Code durante a validacao da tarefa116 (2026-11-09). O erro ja aparecia em
sessoes anteriores (2026-11-08), antes das tarefas 115 e 116:

```
[error] Error: Channel has been closed
    at Object.appendLine (...extensionHostProcess.js...)
    at ChildProcess.<anonymous> (...dist/extension.js...)
    at Process.ChildProcess._handle.onexit (node:internal/child_process...)
```

Ao desligar, o VS Code chama `deactivate()`, que chama `CoreBridge.dispose()`: ele pede ao processo do
Core para sair (`quit`) e volta, sem esperar. Em seguida o VS Code descarta as `subscriptions`, entre
elas o canal de log "Babel TCC". O processo do Core termina depois, no tempo dele, e o handler de
`exit` registrado em `startProcess()` chama `outputChannel.appendLine` num canal ja fechado, que lanca a
excecao dentro do handler. O mesmo vale para `error`, para o `stderr` e para o timeout que mata o
processo se o `quit` nao responder.

O usuario nao ve nada e nenhuma funcao quebra, mas e um erro nao tratado a cada desligamento.

## Decisao
`CoreBridge.logProcessEvent(message)`: os quatro eventos que vem do processo (exit, error, stderr e o
timeout do `quit`) passam por ele, e depois de `dispose()` o evento e descartado. O canal para onde ele
iria ja nao existe.

Alternativas descartadas:
- **try/catch em volta do `appendLine`:** esconderia qualquer outra falha de escrita no log, e o
  padrao do projeto reserva try/catch para fronteiras de I/O com motivo comentado; aqui o motivo e
  conhecido e deterministico.
- **Esperar o processo sair dentro do `deactivate()`:** atrasaria o fechamento do VS Code (ate o
  timeout de 2 s) para salvar uma linha de log que ninguem le.
- **Mudar a ordem das `subscriptions`:** nao resolve; o processo termina de forma assincrona depois de
  todo o descarte.

## Escopo
- `src/services/coreBridge.ts`: `logProcessEvent` e os quatro pontos que o usam.
- `test/services/coreBridge.test.ts`: o mock do processo passa a guardar todos os handlers (o
  `dispose()` registra um segundo `exit`, que antes sobrescrevia o primeiro no mock); 5 testes novos.
- `CHANGELOG.md` (Fixed).

## Fora de escopo
- O bug do `MAX_CRASHES` (o `startProcess` zera o `crashCount`), registrado na tarefa117.
- Horario nas linhas do log (fica para a tarefa118).
