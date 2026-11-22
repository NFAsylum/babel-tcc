# Tarefa 116 - Walkthrough de boas-vindas

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Mostrar a extensao funcionando logo depois da instalacao, sem o usuario precisar descobrir sozinho os
comandos: a pagina nativa de "Primeiros passos" do VS Code (`contributes.walkthroughs`), com quatro
passos curtos que se fazem pelo teclado.

## Contexto
Ate aqui, quem instalava a extensao recebia so a barra de status e os comandos na paleta. O comando
"Abrir Exemplo" (tarefa115) tirou a necessidade de ter um projeto; faltava o caminho que leva o
usuario ate ele e explica o que acontece ao salvar e o que e o `tradu`.

## Decisao

### Os quatro passos
Walkthrough `babelTccGetStarted` ("Primeiros passos com o Babel TCC"):

| Passo | Botao | Conclui com |
|---|---|---|
| Ligue a traducao (so com a traducao desligada) | `command:babel-tcc.toggle` | o comando, ou a configuracao `babel-tcc.enabled` mudar |
| Escolha o seu idioma | `command:babel-tcc.selectLanguage` | o comando, ou a configuracao `babel-tcc.language` mudar |
| Abra um exemplo | `command:toSide:babel-tcc.openExample` | o comando |
| Edite, salve e veja o original | `command:toSide:babel-tcc.showOriginal` | o comando |
| Traduza nomes com tradu | link para a secao "Sistema tradu" do README | abrir o link |

- **Cada descricao termina com um link sozinho na ultima linha**, que o VS Code desenha como botao.
  O botao faz a acao e o mesmo evento conclui o passo: o walkthrough inteiro anda so com `Tab` e
  `Enter` (INV-09).
- **`toSide:` nos passos que abrem editor.** Com `command:toSide:`, o VS Code abre um grupo vazio ao
  lado do walkthrough e o ativa antes de rodar o comando. So isso nao bastou; o F5 mostrou dois
  defeitos, corrigidos nos comandos:
  - **O exemplo abria por cima do walkthrough.** O "Abrir Exemplo" mostra uma lista, e fechar a lista
    devolve o foco para quem o tinha antes, a pagina do walkthrough: o grupo ativo na hora de abrir ja
    era o da pagina. Agora o comando decide o grupo **antes** de mostrar a lista. E o `toSide:` so
    cria e ativa um grupo novo quando a pagina e o unico grupo: com um grupo ja aberto ao lado (um
    segundo exemplo), o grupo ativo continua sendo o da pagina. Por isso a regra e: grupo ativo vazio
    ou mostrando um editor de texto, abre nele (o caso da paleta e o do grupo criado pelo `toSide:`);
    grupo ativo mostrando outra coisa, como a pagina, abre ao lado (`ViewColumn.Beside`).
  - **"Mostrar Codigo Original" respondia "Nenhum editor ativo".** Com a pagina em foco nao existe
    editor de texto ativo, e a troca de grupo feita pelo `toSide:` nao chega a tempo a
    `activeTextEditor`. Agora, sem editor ativo, o comando usa a visao traduzida que esta na tela,
    desde que seja **so uma** (com duas, nao da para saber qual o usuario quis, e o aviso continua). O
    original abre no grupo dessa visao.
- **Passo "Ligue a traducao", so com a traducao desligada.** O F5 mostrou que, com a traducao
  desligada, o passo 2 abre o exemplo no original e o passo 3 nao tem o que mostrar: o guia inteiro
  perde o sentido sem avisar. O passo novo vem primeiro e tem `when: !config.babel-tcc.enabled`, entao
  so aparece nesse caso. O botao usa o "Alternar Traducao", que tambem desligaria a traducao de quem ja
  a tem ligada; por isso esse botao so e seguro num passo visivel apenas com ela desligada, e o teste
  do manifesto fixa as duas coisas. Alternativas descartadas: um comando novo so de ligar (mais um
  comando na paleta para um uso so) e fazer o "Abrir Exemplo" traduzir mesmo desligado (contrariaria
  a escolha do usuario).
- **O passo 4 diz que o `tradu` so existe em C# e Python**, para nao prometer o que JavaScript,
  VisuAlg e Portugol Studio nao fazem. O link aponta para `#sistema-tradu` (o guia citava
  `#anotacoes-tradu`, ancora que nao existe no README).

### Midia em Markdown, so com codigo
Cada passo tem midia em Markdown (INV-08): o leitor de tela le, segue o tema e nao precisa de par
claro/escuro. O conteudo e **so codigo**, rotulado por codigos de idioma (`// C#`, `// pt-br`), sem
prosa: o texto explicativo esta nas descricoes, que sao traduzidas nos 7 nls, enquanto um arquivo de
midia so existe em uma versao. Assim a midia nao fica em ingles para quem le em portugues, nem em
portugues para quem le em japones. As palavras-chave dos trechos vieram das tabelas reais de
traducao.

Alternativa descartada: um arquivo de midia por idioma, escolhido por placeholder do nls no caminho.
Seriam 28 arquivos para manter sincronizados, por um ganho pequeno sobre as descricoes ja traduzidas.

### Comando "Abrir Introducao"
O VS Code so abre o walkthrough sozinho depois da instalacao. `babel-tcc.openWalkthrough` ("Abrir
Introducao") o reabre com `workbench.action.openWalkthrough`, montando a referencia a partir do id da
extensao em execucao (`context.extension.id`) em vez de um literal.

## Contrato no teste do manifesto
- Um walkthrough, com o id que o comando abre.
- Toda midia e Markdown e o arquivo existe na pasta da extensao.
- Toda descricao termina com um link nos 7 nls, com o mesmo destino em todos (so o rotulo e
  traduzido).
- Todo link de comando aponta para um comando declarado; os que abrem editor usam `toSide:`.
- Cada passo e concluido pelo proprio botao; os eventos so citam comandos e configuracoes que existem.

## Limitacoes conhecidas
- O walkthrough so aparece sozinho numa instalacao nova; quem ja tinha a extensao chega a ele pelo
  "Abrir Introducao".
- Com a janela muito estreita, o VS Code nao abre o grupo ao lado: o exemplo abre por cima do
  walkthrough, que continua numa aba.

## Escopo
- `package.json`: `contributes.walkthroughs` e o comando `babel-tcc.openWalkthrough`; 11 chaves novas
  nos 7 nls.
- `media/walkthrough/*.md` (4 arquivos).
- `src/commands/openWalkthrough.ts` (novo); `COMMANDS.OPEN_WALKTHROUGH` e `WALKTHROUGH_ID`; registro no
  `extension.ts`.
- `src/commands/showOriginal.ts` (novo, `resolveEditorForShowOriginal`) e o comando no `extension.ts`
  abrindo o original no grupo da visao traduzida; `openExample.ts` guardando o grupo antes da lista.
- `test/commands/openWalkthrough.test.ts` e `test/commands/showOriginal.test.ts` (novos); bloco
  `walkthrough` no teste do manifesto; `visibleTextEditors` e `tabGroups.activeTabGroup` no mock.
- `CHANGELOG.md`, `docs/user-guide/getting-started.md`.

## Fora de escopo
- Diagnostico de ambiente (Python ausente etc.), listado no guia junto da 116: fica para a tarefa118.
- Midia em imagem ou video.
