# Tarefa 117 - Confianca do workspace e acessibilidade do status bar

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Fazer a extensao funcionar numa pasta aberta em Modo Restrito, sem confiar no que essa pasta traz, e
fazer o status bar ser anunciado corretamente por leitor de tela.

## Contexto
O manifesto nao declarava `capabilities`. Sem essa declaracao o VS Code desabilita a extensao, sem
aviso, em toda pasta nao confiavel - o caso do professor que baixa uma pasta de exemplos. Para quem
usa, a extensao simplesmente nao liga.

O item do status bar mostra `$(globe) PT-BR`. O codicon nao e lido por leitor de tela, entao o item era
anunciado como "PT-BR", sem dizer o que e, em que estado esta, nem que abre o seletor de idioma.

## Decisao
Ver DT-014. `untrustedWorkspaces.supported: "limited"`:

- funciona com as tabelas de traducao empacotadas;
- `babel-tcc.translationsPath` em `restrictedConfigurations`: o VS Code ignora o valor do workspace;
- a pasta `babel-tcc-translations` ao lado do workspace so e detectada com o workspace confiavel;
- o Core e a checagem do .NET rodam com a pasta da extensao como diretorio atual, porque no Windows um
  comando sem caminho e procurado no diretorio atual antes do PATH.

O risco que justifica as restricoes: ao salvar a visao traduzida, a traducao reversa usa as tabelas
para reescrever o original. Tabelas trazidas por uma pasta desconhecida poderiam corromper o codigo.

`virtualWorkspaces.supported: false`: a extensao precisa de arquivos reais e de processo local.

Status bar: `name = "Babel TCC"` (nome do item no menu da propria barra de status) e
`accessibilityInformation` com papel `button` e um rotulo em frase completa, traduzido nos 6 bundles,
que diz o estado e o idioma sem depender do codicon.

## Escopo
- `package.json`: `capabilities`; descricoes nos 7 `package.nls*.json`.
- `src/services/coreBridge.ts`: pasta irma condicionada a `isTrusted`; `cwd` do Core; o log so registra
  "restarting process after crash" quando houve crash (antes registrava tambem a primeira
  inicializacao, relatando um crash que nao aconteceu).
- `src/extension.ts`: `cwd` da checagem do .NET.
- `src/ui/statusBar.ts`: `name` e `accessibilityInformation`; 2 frases novas nos 6 `l10n/bundle.l10n.*.json`.
- Testes: manifesto, `coreBridge`, `extension`, `statusBar`; `isTrusted` no mock do `vscode`.
- Docs: DT-014, `CHANGELOG.md`, `docs/user-guide/troubleshooting.md`.

## Fora de escopo
- O restante do plano original da tarefa117: diagnostico de ambiente por linguagem (.NET e Python),
  comando "Verificar Ambiente", `LogOutputChannel` e revisao das mensagens de erro. Reavaliar depois
  das tarefas 115 e 116, conforme o tempo.
- Reiniciar o Core quando o usuario confia no workspace (`onDidGrantWorkspaceTrust`). Hoje a mudanca
  vale ao recarregar a janela; documentado.
- O limite de crashes do `CoreBridge` (`MAX_CRASHES`) nunca dispara: `startProcess` zera o
  `crashCount` a cada inicio, entao um Core que cai e e reiniciado volta sempre a zero, e o aviso de
  motor instavel nunca aparece. O teste existente so passa porque forca `crashCount = 3`. Corrigir
  muda o comportamento em falha (o aviso passaria a aparecer a cada requisicao) e pede desenho
  proprio; fica para tarefa separada.
- Auditoria de acessibilidade completa (tarefa119). Esta tarefa faz so o item do status bar, que era
  o passo 1 previsto para a 119.
