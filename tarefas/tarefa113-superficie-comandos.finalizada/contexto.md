# Contexto - Tarefa 113

## Dependencias
Nenhuma no codigo. Recomendado sair de uma `main` atualizada, apos os PRs de documentacao e da
release 1.1.0 - mas nao ha conflito tecnico: aqueles PRs mexem em `description`/`keywords`/versao,
e esta tarefa mexe em `commands`/`menus`/`keybindings`.

## Bloqueia
- Tarefa 114 (configuracoes navegaveis): a assercao nova dela entra no teste de contrato do manifesto
  criado aqui.
- As tarefas seguintes da fase 7 que registram comandos novos (comando "Abrir Exemplo" e
  walkthrough): dependem do padrao de `category`/`icon` e do teste de contrato definidos aqui.
- A auditoria de acessibilidade da fase 7: audita, entre outras coisas, as superficies criadas aqui.

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json
- packages/ide-adapters/vscode/package.nls*.json (7 arquivos)
- packages/ide-adapters/vscode/src/config/constants.ts (COMMANDS, CONFIG_KEYS)
- packages/ide-adapters/vscode/src/config/languages.ts (SUPPORTED_LANGUAGES - fonte das extensoes da
  clausula `when`)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (TRANSLATED_SCHEME,
  READONLY_SCHEME)
- packages/ide-adapters/vscode/test/__mocks__/vscode.ts
- packages/ide-adapters/vscode/test/config/languages.test.ts (modelo de teste que le o package.json
  do disco - o teste de contrato do manifesto segue esse padrao)
- docs/decisoes-tecnicas.md (DT-012)

## Notas
- **Regras de superficie que esta tarefa precisa respeitar:**
  - o Command Palette continua sendo a via canonica - menus e atalhos sao atalhos, nunca a unica via;
    nenhum comando e escondido do Palette;
  - `category` compoe o prefixo "Babel TCC:"; o `title` nao o repete;
  - nenhuma string literal na UI: toda string do manifesto vai para os 7 `package.nls*.json`, e toda
    string de runtime para os 6 `l10n/bundle.l10n.*.json`;
  - icone em `editor/title` so como codicon (`$(...)`), que se adapta a tema claro, escuro e alto
    contraste;
  - tudo alcancavel so pelo teclado;
  - visibilidade por clausula `when`, nunca por checagem dentro do comando.
- `editor/title` recebe **um** botao por vez: "abrir visao traduzida" num original suportado,
  "mostrar original" numa visao traduzida. Dois icones concorrentes na barra do editor sao poluicao.
- Teclas que o ABNT2 alcanca por AltGr (AltGr = Ctrl+Alt no Windows) e que nao podem entrar num chord:
  `1 2 3 4 5 6 = [ ] q w e c /` (produzem ¹ ² ³ £ ¢ ¬ § ª º / ? ° ₢).
- **Prefixo `ctrl+alt+b` (versao inicial, descartada):** verificado em teclado ABNT2 real em
  2026-08-16, mas e atalho nativo do VS Code ("Toggle Secondary Side Bar"). Ver especificacao, secao
  "Revisao do PR #185".
- **Prefixo `ctrl+alt+u` (atual):** escolhido por varredura do keymap padrao do VS Code 1.141 (letras
  livres em Ctrl+Alt e Cmd+Alt: `a d e h j m n q u x z`), excluindo as alcancadas por AltGr no ABNT2
  (`q`, `e`) e as que o macOS reserva em Cmd+Opt (`h`, `d`, `m`). Apenas o prefixo precisa da
  verificacao de AltGr - a segunda tecla do chord e pressionada sem modificador.
- As clausulas `when` foram validadas contra o parser do proprio VS Code (codigo-fonte da tag 1.141.0)
  antes da validacao manual: todas sao aceitas, e avaliadas por editor mostram no maximo um botao em
  `editor/title`.
- O teste de contrato do manifesto e o item de maior valor desta tarefa: e o analogo do
  `scripts/validate.py` do repo de traducoes aplicado a camada de UI. Manifesto sem validacao
  drifta - foi exatamente assim que a `description` do Marketplace ficou desatualizada por 2 releases.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism` em vez de `npm test`.
