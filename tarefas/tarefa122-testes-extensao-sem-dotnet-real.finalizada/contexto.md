# Contexto - Tarefa 122

## Dependencias
Nenhuma.

## Bloqueia
Nada.

## Arquivos relevantes
- packages/ide-adapters/vscode/test/extension.test.ts
- packages/ide-adapters/vscode/test/__mocks__/vscode.ts
- packages/ide-adapters/vscode/src/extension.ts (`checkDotnetInstalled`; nao muda)
- packages/ide-adapters/vscode/test/services/coreBridge.test.ts (modelo do mock de
  `child_process`)
- .github/workflows/ci.yml (job "VS Code Extension", que nao instala o .NET)

## Como reproduzir o defeito (antes da correcao)

Com um `dotnet` falso na frente do PATH, que registra as chamadas e encerra com erro:

```bash
mkdir -p /tmp/fakebin
printf '#!/bin/sh\necho "$@" >> /tmp/dotnet_calls.log\nexit 1\n' > /tmp/fakebin/dotnet
chmod +x /tmp/fakebin/dotnet

cd packages/ide-adapters/vscode
PATH=/tmp/fakebin:$PATH npx vitest run test/extension.test.ts; echo "exit=$?"
wc -l < /tmp/dotnet_calls.log
```

Antes: 19 de 19 testes, 15 erros nao tratados, `exit=1`, 15 chamadas registradas.
Depois: todos os testes passam, 0 erros, `exit=0`, e o arquivo de registro nem e criado.

## Notas
- `vi.mock('child_process', ...)` precisa listar `spawn` alem de `execFile`: o `CoreBridge`
  importa `spawn` do mesmo modulo.
- `vi.clearAllMocks()` do `beforeEach` nao desfaz `mockImplementation`; a resposta padrao da
  sondagem e reposta a cada teste logo depois dele.
