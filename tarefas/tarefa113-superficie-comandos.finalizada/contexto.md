# Contexto - Tarefa 113

## Dependencias
Nenhuma no codigo. Recomendado sair de uma `main` atualizada, apos os PRs de documentacao e da
release 1.1.0 - mas nao ha conflito tecnico: aqueles PRs mexem em `description`/`keywords`/versao,
e esta tarefa mexe em `commands`/`menus`/`keybindings`.

## Bloqueia
- Tarefa 114 (comando "Abrir Exemplo") e 115 (walkthrough): ambas registram comandos novos e
  dependem do padrao de `category`/`icon` e do teste de contrato do manifesto definidos aqui.
- Tarefa 118 (auditoria de acessibilidade): audita, entre outras coisas, as superficies criadas aqui.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/ui/contextKeys.ts (novo)
- packages/ide-adapters/vscode/src/ui/statusBar.ts (modelo de classe com Disposable e subscricoes)
- packages/ide-adapters/vscode/src/config/constants.ts (COMMANDS, CONFIG_KEYS)
- packages/ide-adapters/vscode/src/extension.ts (registro em subscriptions)
- packages/ide-adapters/vscode/src/services/configurationService.ts (onDidChangeConfiguration)
- packages/ide-adapters/vscode/src/services/languageDetector.ts (isSupported)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (isTranslatedScheme,
  TRANSLATED_SCHEME, READONLY_SCHEME)
- packages/ide-adapters/vscode/package.json
- packages/ide-adapters/vscode/package.nls*.json (7 arquivos)
- packages/ide-adapters/vscode/test/__mocks__/vscode.ts
- packages/ide-adapters/vscode/test/config/languages.test.ts (modelo de teste que le o package.json
  do disco - o teste de contrato do manifesto segue esse padrao)
- docs/decisoes-tecnicas.md (DT-012)
- guia-ux-acessibilidade-vscode_1.md, secoes 3 (invariantes) e 5.1 (detalhamento desta tarefa)

## Notas
- **Invariantes que esta tarefa precisa respeitar** (do guia da etapa): INV-02 (Palette continua
  canonico), INV-03 (`category` em vez de prefixo no titulo), INV-04 (zero string literal na UI),
  INV-05 (so codicon como icone), INV-09 (tudo alcancavel por teclado), INV-11 (visibilidade por
  `when` + context key), INV-14 (chord com prefixo proprio, verificado em ABNT2).
- `editor/title` recebe **um** botao por vez: "abrir visao traduzida" quando o arquivo e suportado e
  a visao nao esta aberta, "mostrar original" quando ja se esta na visao traduzida. Dois icones
  concorrentes na barra do editor sao poluicao.
- Teclas do chord a evitar por causa do AltGr do ABNT2 (AltGr = Ctrl+Alt no Windows): entre outras
  `q`, `w`, `e`, `c`, `1`, `2`, `3`, `/`.
- **Prefixo `ctrl+alt+b` verificado em teclado ABNT2 real (2026-08-16):** nao produz caractere no
  Bloco de Notas, no Notas Autoadesivas nem no VS Code. Liberado para uso. Apenas o prefixo precisa
  dessa verificacao - a segunda tecla do chord e pressionada sem modificador, entao nao passa pelo
  AltGr.
- O teste de contrato do manifesto e o item de maior valor desta tarefa: e o analogo do
  `scripts/validate.py` do repo de traducoes aplicado a camada de UI. Manifesto sem validacao
  drifta - foi exatamente assim que a `description` do Marketplace ficou desatualizada por 2 releases.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism` em vez de `npm test`.
