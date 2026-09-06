# Definition of Done - Tarefa 113

## Context keys
- [x] `src/ui/contextKeys.ts` criado com `CONTEXT_KEYS` e `ContextKeyManager`
- [x] As 4 chaves publicadas na construcao e recalculadas em mudanca de editor ativo e de config
- [x] `ContextKeyManager` registrado em `context.subscriptions` e com `dispose()` liberando as 2
      subscricoes

## Manifesto
- [x] Os 5 comandos com `category` e sem o prefixo `"Babel TCC: "` no `title`
- [x] `icon` codicon nos comandos usados em `editor/title`
- [x] `menus.editor/title` com um botao por vez, cada item com `when`
- [x] `menus.editor/context` com os 3 comandos agrupados, cada item com `when`
- [x] `keybindings` em chord `ctrl+alt+b <tecla>`, todos com variante `mac`
- [x] Nenhum comando escondido do Command Palette sem necessidade (INV-02)

## i18n
- [x] Chave `extension.category` nos 7 `package.nls*.json`
- [x] Titulos sem prefixo nos 7 arquivos
- [x] Nenhuma string nova sem traducao nos 7 nls e nos 6 bundles `l10n/`

## Testes
- [x] `test/ui/contextKeys.test.ts` cobrindo: publicacao inicial das 4 chaves, recalculo por troca de
      editor, recalculo por mudanca de config, ausencia de editor ativo, esquema traduzido, dispose
- [x] `test/manifest/manifest.test.ts` com as assercoes de contrato:
      - [x] todo comando de `COMMANDS` esta em `contributes.commands` e vice-versa
      - [x] toda chave `%x%` do `package.json` existe nos 7 nls, sem chaves orfas
      - [x] todo `command` de `menus.*` e `keybindings` existe em `contributes.commands`
      - [x] todo item de `editor/title` e `editor/context` tem `when` nao vazio
      - [x] todo comando de `editor/title` tem `icon` no formato `$(...)`
      - [x] todo keybinding tem modificador, tem variante `mac`, e nao usa tecla do AltGr ABNT2
- [x] `test/l10n/bundles.test.ts` verificando as chamadas `vscode.l10n.t()` contra os 6 bundles
- [x] `test/__mocks__/vscode.ts` captura `setContext` e expoe helper de leitura
- [x] `test/extension.test.ts` cobre o registro do `ContextKeyManager`
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 244 passam (baseline 218)
- [x] `npm run lint` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Botao aparece na barra de titulo so em arquivo de linguagem suportada
- [x] Botao troca para "mostrar original" quando a visao traduzida esta ativa
- [x] Menu de contexto mostra as acoes certas em cada situacao
- [x] Os 4 keybindings funcionam e nenhum conflita (conferir em Keyboard Shortcuts)
- [x] Chord testado em teclado ABNT2: digitar caracteres com AltGr nao dispara comando
- [x] Command Palette continua listando os 5 comandos, agora com o prefixo vindo de `category`
- [x] Nenhum item de menu aparece para depois responder "tipo de arquivo nao suportado"

## Processo
- [x] DT-012 registrado em `docs/decisoes-tecnicas.md` com contexto, alternativas e consequencias
- [x] `CHANGELOG.md` atualizado
- [x] Commits em conventional commits com escopo `vscode`
- [x] Pasta da tarefa renomeada para `.finalizada`
