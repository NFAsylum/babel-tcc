# Definition of Done - Tarefa 113

## Visibilidade declarativa
- [x] Toda clausula `when` usa so context keys nativas: `resourceExtname`, `resourceScheme`,
      `config.babel-tcc.enabled` e `editorTextFocus`
- [x] A extensao nao publica context key (nenhum `setContext` no `src/`)
- [x] Extensoes da clausula iguais a `SUPPORTED_LANGUAGES`, comparadas sem diferenciar caixa, como o
      `LanguageDetector`
- [x] Clausulas validadas contra o parser de `when` do VS Code 1.141: todas aceitas, e no maximo um
      botao de `editor/title` por editor

## Manifesto
- [x] Os 5 comandos com `category` e sem o prefixo `"Babel TCC: "` no `title`
- [x] `icon` codicon nos comandos usados em `editor/title`
- [x] `menus.editor/title` com um botao por vez, cada item com `when`
- [x] `menus.editor/context` com os 3 comandos agrupados, cada item com `when`
- [x] `keybindings` em chord `ctrl+alt+u <tecla>`, todos com variante `mac` (`cmd+alt+u <tecla>`)
- [x] Prefixo `ctrl+alt+u` livre no keymap padrao do VS Code 1.141 (varredura do bundle instalado)
- [x] Nenhum comando escondido do Command Palette

## i18n
- [x] Chave `extension.category` nos 7 `package.nls*.json`
- [x] Titulos sem prefixo nos 7 arquivos
- [x] Nenhuma string nova sem traducao nos 7 nls e nos 6 bundles `l10n/`

## Testes
- [x] `test/manifest/manifest.test.ts` com as assercoes de contrato:
      - [x] todo comando de `COMMANDS` esta em `contributes.commands` e vice-versa
      - [x] toda chave `%x%` do `package.json` existe nos 7 nls, sem chaves orfas
      - [x] todo `command` de `menus.*` e `keybindings` existe em `contributes.commands`
      - [x] todo item de `editor/title` e `editor/context` tem `when` nao vazio
      - [x] todo comando de `editor/title` tem `icon` no formato `$(...)`
      - [x] os itens de abrir usam a clausula de arquivo suportado, de original e de habilitado; os de
            voltar usam a clausula de visao traduzida; os keybindings usam as mesmas clausulas
      - [x] nenhuma clausula depende de chave `babelTcc.*`, e todo `config.*` lido e configuracao
            declarada
      - [x] todo keybinding tem modificador, tem variante `mac`, usa o prefixo `ctrl+alt+u` e nao usa
            tecla do AltGr ABNT2
- [x] Testes de mutacao das regras novas: extensao a mais na regex, prefixo antigo, tecla `[` no
      chord, chave `babelTcc.*` de volta, `config.*` com nome errado - cada um quebra o teste certo
- [x] `test/l10n/bundles.test.ts` verificando as chamadas `vscode.l10n.t()` contra os 6 bundles
- [x] `test/extension.test.ts` garante que a ativacao nao publica context key
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 259 passam
- [x] `npm run lint` limpo, `npx tsc --noEmit` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Botao aparece na barra de titulo so em arquivo de linguagem suportada
- [x] Botao troca para "mostrar original" quando a visao traduzida esta ativa
- [x] Tela dividida (visao traduzida de um lado, `README.md` do outro), configuracao padrao: com o
      grupo do `README.md` ativo nenhum botao da extensao; com o da visao traduzida ativo, so
      "mostrar original"
- [x] Tela dividida com `workbench.editor.alwaysShowEditorActions: true` (por padrao o VS Code so
      desenha botoes no grupo ativo; com isso ligado, desenha nos dois ao mesmo tempo - e o cenario
      em que as chaves globais da primeira versao erravam): o grupo do `README.md` segue sem botao
      enquanto o da visao traduzida mostra "mostrar original"
- [x] Com `babel-tcc.enabled` desligado, os itens de abrir somem do titulo e do menu de contexto
- [x] Menu de contexto mostra as acoes certas em cada situacao
- [x] Keyboard Shortcuts buscando `ctrl+alt+u`: so aparecem os 4 atalhos da extensao
- [x] `Ctrl+Alt+B` volta a alternar a Secondary Side Bar com a extensao instalada
- [x] Os 4 keybindings funcionam
- [x] `AltGr+U` no teclado ABNT2 nao produz caractere nem dispara comando
- [x] Command Palette continua listando os 5 comandos, agora com o prefixo vindo de `category`

## Processo
- [x] DT-012 registrado em `docs/decisoes-tecnicas.md`, com a alternativa das context keys nativas e
      a revisao do PR #185
- [x] `CHANGELOG.md` atualizado
- [x] `docs/user-guide/getting-started.md` com botao, menu de contexto e atalhos
- [x] Commits em conventional commits com escopo `vscode`
- [x] Pasta da tarefa renomeada para `.finalizada`
