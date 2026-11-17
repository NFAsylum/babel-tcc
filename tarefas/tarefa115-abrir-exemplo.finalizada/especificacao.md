# Tarefa 115 - Comando "Abrir Exemplo" e exemplos no pacote

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Permitir experimentar a extensao sem ter projeto nenhum: o professor em aula, ou o aluno que acabou de
instalar. Um comando lista exemplos curtos e abre o escolhido ja traduzido.

## Contexto
Os exemplos ficam em `examples/`, na raiz do repositorio. O `vsce` so empacota o que esta dentro da
pasta da extensao, entao **nenhum exemplo chega a quem instala pelo Marketplace**. Hoje, para ver a
extensao funcionando, a pessoa precisa ter um arquivo `.cs`, `.py`, `.js`, `.alg` ou `.por` a mao.

## Decisao

### Empacotamento
`scripts/copy-examples.js`, no padrao do `copy-translations.js`: copia `examples/` para
`packages/ide-adapters/vscode/examples/` (artefato de build, no `.gitignore`), pula pastas de build e
cache, e **falha alto** se a origem nao existir ou estiver vazia. Encadeado no `prepackage` e no
`release.yml`, que chama os passos de copia separadamente e nao passa pelo `prepackage`.

### Comando
`babel-tcc.openExample` ("Abrir Exemplo"), em `src/commands/openExample.ts`:

1. QuickPick com titulo e placeholder, montado de um **catalogo tipado** (rotulo, linguagem, o que o
   exemplo demonstra), nao de uma listagem da pasta: os rotulos sao traduziveis e a ordem e
   pedagogica - o "Ola, mundo" primeiro, depois um exemplo por linguagem. Rotulos so em texto, sem
   codicon.
2. Cancelar nao faz nada.
3. O exemplo escolhido e copiado para o armazenamento global da extensao
   (`globalStorageUri/examples/<versao>/...`).
4. Abre a visao traduzida da copia (editavel ou somente leitura, conforme `babel-tcc.readonly`), ou o
   original se a traducao estiver desligada, numa aba permanente (`preview: false`).
5. Falha de disco vira mensagem com botao "Abrir Log", nunca excecao saindo do comando.

Atalho `Ctrl+Alt+U` `X` ("eXemplo"): o `E` e uma das teclas que o AltGr alcanca no ABNT2, que o teste
de contrato do manifesto recusa.

### Onde a copia do exemplo fica (vira DT-015)
Alternativas consideradas:

- **Abrir direto de dentro da extensao:** a pasta da extensao e somente leitura em instalacoes do
  Marketplace, entao editar e salvar o exemplo - o passo seguinte natural, e o passo "edite e salve"
  do walkthrough da tarefa116 - falharia.
- **`os.tmpdir()`:** limpeza imprevisivel pelo sistema e permissoes variaveis no Linux.
- **Pedir ao usuario uma pasta:** atrito exatamente no momento em que se quer zero atrito.
- **`globalStorageUri` (escolhida):** gravavel, multiplataforma, nao depende de pasta aberta, e e da
  propria extensao.

Uma copia ja existente **nao e sobrescrita**, para as edicoes do usuario sobreviverem a reabrir o
exemplo. A pasta de versao faz uma versao nova da extensao comecar de copias novas, com os exemplos
atualizados.

A copia e sempre aberta como `file:`. O VS Code pode entregar o `globalStorageUri` com o esquema
proprio `vscode-userdata:`, e o resto da extensao so reconhece um arquivo original pelo esquema
`file:`. Validando no F5, um exemplo aberto com a traducao desligada nao virava a visao traduzida ao
religar a traducao: a aba nao era encontrada como arquivo original. `resolveExampleCopy` converte a URI
para `file:`.

O registro formal vira o DT-015 em `docs/decisoes-tecnicas.md` quando o PR da tarefa117 (que traz o
DT-014, no fim do mesmo arquivo) for mergeado. Escrever agora no fim do arquivo geraria conflito
entre os dois PRs.

## Limitacoes conhecidas
- A copia fica fora de qualquer pasta aberta, entao o `FileSystemWatcher` da extensao nao a observa:
  alterar o arquivo da copia por fora (outro editor) nao recarrega a visao traduzida. Para um exemplo
  descartavel e aceitavel.
- Nao ha comando para restaurar um exemplo ao original. Apagar a copia faz o proximo "Abrir Exemplo"
  criar outra.

## Escopo
- `scripts/copy-examples.js` (novo); `prepackage` e script `copy-examples` no `package.json`; passo
  "Copy examples" no `release.yml`; `packages/ide-adapters/vscode/examples/` no `.gitignore`.
- `src/commands/openExample.ts` (novo); `COMMANDS.OPEN_EXAMPLE`; registro no `extension.ts`.
- `package.json`: comando (com `category` e `icon: "$(book)"`) e keybinding; titulo nos 7 nls; 14
  frases novas nos 6 bundles de runtime.
- `test/commands/openExample.test.ts` (novo); `Uri.joinPath`, `fs.copy` e `fs.createDirectory` no mock.
- `CHANGELOG.md`, `docs/user-guide/getting-started.md`.

## Fora de escopo
- O walkthrough de boas-vindas (tarefa116), que usa este comando.
- Exemplos novos. O catalogo usa os que ja existem no repositorio.
