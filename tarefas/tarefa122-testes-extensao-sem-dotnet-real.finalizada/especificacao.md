# Tarefa 122 - Testes da extensao nao devem executar o dotnet real

## Fase
Correcao de defeito (encontrado durante a verificacao da tarefa112)

## Objetivo
Fazer a suite de testes da extensao VS Code dar o mesmo resultado em qualquer maquina,
independentemente de haver `dotnet` instalado e de qual runtime ele tem.

## Contexto
`activate()` chama `checkDotnetInstalled`, que executa `dotnet --list-runtimes` para avisar o
usuario quando falta o runtime .NET 8. `test/extension.test.ts` chama `activate()` em quase
todo teste e nao substituia `child_process`. Cada teste, portanto, executava o `dotnet` de
verdade.

Medido na `main` (commit `98234e6`), rodando so `test/extension.test.ts`:

| Ambiente | Testes | Erros nao tratados | Codigo de saida |
|---|---|---|---|
| `dotnet` com runtime 8 no PATH | 19 de 19 | 0 | 0 |
| `dotnet` falso que encerra com erro | 19 de 19 | 15 | 1 |
| Sem `dotnet` no PATH | 19 de 19 | 15 | 1 |

O `dotnet` falso registrou 15 invocacoes de `--list-runtimes` por execucao.

Quando a sondagem falha, o codigo de producao chama
`vscode.window.showErrorMessage(...).then(...)`. No mock, `showErrorMessage` devolve
`undefined`, e o `.then` vira `TypeError: Cannot read properties of undefined (reading 'then')`
dentro de um callback assincrono. O Vitest conta isso como erro nao tratado e encerra com 1,
mesmo com todos os testes passando.

Consequencias:

- quem clona o repositorio sem o runtime .NET 8 (so com o 9, por exemplo, ou sem `dotnet`)
  ve a suite da extensao falhar sem nenhum teste vermelho;
- o job "VS Code Extension" do CI nao instala o .NET; ele passa porque a imagem do runner
  traz o runtime 8. Quando a imagem deixar de trazer, o job quebra sem mudanca no codigo;
- `checkDotnetInstalled` nao tinha nenhum teste. O PR que trocou a sondagem para
  `--list-runtimes` registra isso ("a funcao nao tinha testes").

O codigo de producao esta correto: no VS Code real, `showErrorMessage` sempre devolve um
Thenable. O defeito e do teste.

Durante a tarefa112 a mesma causa apareceu de outra forma: `activate()` tambem aquecia o
cache de keywords, o que subia o processo do Core, e isso surgia como `EPIPE` nao tratado.
Aquele caso foi tratado la, substituindo `warmCache` no setup. Esta tarefa fecha o que
restou.

## Decisao
Substituir `child_process` em `test/extension.test.ts`, como `test/services/coreBridge.test.ts`
ja faz. O setup responde a sondagem como "runtime 8 instalado" por padrao, e cada teste que
quer outro resultado o declara.

Com a sondagem sob controle, `checkDotnetInstalled` ganha testes para os dois resultados.

## Escopo
- `test/extension.test.ts`: mock de `child_process`, resposta padrao da sondagem no
  `beforeEach`, e testes da checagem de runtime.
- `test/__mocks__/vscode.ts`: `env.openExternal`, que a checagem usa e o mock nao tinha.

## Fora de escopo
- Qualquer mudanca em `src/`.
- Instalar o .NET no job da extensao no CI. Deixa de ser necessario.
