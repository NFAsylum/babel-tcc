# Definition of Done - Tarefa 113

## Context keys
- [ ] `src/ui/contextKeys.ts` criado com `CONTEXT_KEYS` e `ContextKeyManager`
- [ ] As 4 chaves publicadas na construcao e recalculadas em mudanca de editor ativo e de config
- [ ] `ContextKeyManager` registrado em `context.subscriptions` e com `dispose()` liberando as 2
      subscricoes

## Manifesto
- [ ] Os 5 comandos com `category` e sem o prefixo `"Babel TCC: "` no `title`
- [ ] `icon` codicon nos comandos usados em `editor/title`
- [ ] `menus.editor/title` com um botao por vez, cada item com `when`
- [ ] `menus.editor/context` com os 3 comandos agrupados, cada item com `when`
- [ ] `keybindings` em chord `ctrl+alt+b <tecla>`, todos com variante `mac`
- [ ] Nenhum comando escondido do Command Palette sem necessidade (INV-02)

## i18n
- [ ] Chave `extension.category` nos 7 `package.nls*.json`
- [ ] Titulos sem prefixo nos 7 arquivos
- [ ] Nenhuma string nova sem traducao nos 7 nls e nos 6 bundles `l10n/`

## Testes
- [ ] `test/ui/contextKeys.test.ts` cobrindo: publicacao inicial das 4 chaves, recalculo por troca de
      editor, recalculo por mudanca de config, ausencia de editor ativo, esquema traduzido, dispose
- [ ] `test/manifest/manifest.test.ts` com as assercoes de contrato:
      - [ ] todo comando de `COMMANDS` esta em `contributes.commands` e vice-versa
      - [ ] toda chave `%x%` do `package.json` existe nos 7 nls, sem chaves orfas
      - [ ] todo `command` de `menus.*` e `keybindings` existe em `contributes.commands`
      - [ ] todo item de `editor/title` e `editor/context` tem `when` nao vazio
      - [ ] todo comando de `editor/title` tem `icon` no formato `$(...)`
      - [ ] todo keybinding tem modificador, tem variante `mac`, e nao usa tecla do AltGr ABNT2
- [ ] `test/l10n/bundles.test.ts` verificando as chamadas `vscode.l10n.t()` contra os 6 bundles
- [ ] `test/__mocks__/vscode.ts` captura `setContext` e expoe helper de leitura
- [ ] `test/extension.test.ts` cobre o registro do `ContextKeyManager`
- [ ] Suite verde (`npx vitest run --no-file-parallelism`) - baseline 207, esperado ~230
- [ ] `npm run lint` limpo e `npm run build` passando

## Validacao manual (F5)
- [ ] Botao aparece na barra de titulo so em arquivo de linguagem suportada
- [ ] Botao troca para "mostrar original" quando a visao traduzida esta ativa
- [ ] Menu de contexto mostra as acoes certas em cada situacao
- [ ] Os 4 keybindings funcionam e nenhum conflita (conferir em Keyboard Shortcuts)
- [ ] Chord testado em teclado ABNT2: digitar caracteres com AltGr nao dispara comando
- [ ] Command Palette continua listando os 5 comandos, agora com o prefixo vindo de `category`
- [ ] Nenhum item de menu aparece para depois responder "tipo de arquivo nao suportado"

## Processo
- [ ] DT-012 registrado em `docs/decisoes-tecnicas.md` com contexto, alternativas e consequencias
- [ ] `CHANGELOG.md` atualizado
- [ ] Commits em conventional commits com escopo `vscode`
- [ ] Pasta da tarefa renomeada para `.finalizada`
