# Tarefa 113 - Superficie de comandos: menus, keybindings e clausulas when

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Tornar os 5 comandos da extensao descobriveis fora do Command Palette, por botao na barra de
titulo do editor, menu de contexto e atalhos de teclado - com visibilidade controlada de forma
declarativa por clausulas `when` no manifesto.

## Contexto
Hoje a extensao contribui 5 comandos e **nenhuma** outra superficie de UI: nao existem
`contributes.menus`, `contributes.keybindings` nem chamadas a `setContext`. Quem nao souber de
antemao que a extensao existe e como ela se chama nao tem como aciona-la. Para o publico-alvo
(estudante iniciante, professor demonstrando em aula) isso equivale a funcionalidade inexistente.

Alem disso, os titulos dos comandos carregam o prefixo `"Babel TCC: "` no proprio `title`. O VS Code
compoe esse prefixo automaticamente a partir de `category`, e o faz **apenas** no Command Palette -
dentro de um menu de contexto o prefixo e ruido, porque o contexto ja e obvio.

## Decisao
Ver DT-012. Visibilidade de item de UI e declarativa: cada item de menu e cada keybinding tem
clausula `when`, em vez de aparecer sempre e responder "tipo de arquivo nao suportado" depois do
clique. As clausulas usam so context keys nativas do VS Code:

- `resourceExtname` e `resourceScheme` para o que e de cada editor (o arquivo e suportado? e uma
  visao traduzida?). O VS Code as avalia por grupo de editor, entao com a tela dividida cada barra de
  titulo responde pelo proprio arquivo.
- `config.babel-tcc.enabled` para o estado habilitado, lido direto da configuracao.

A extensao nao publica nenhuma context key e nao tem codigo de UI para isso.

As checagens defensivas dentro dos comandos **permanecem**: o Command Palette continua expondo todos
os comandos, entao um comando ainda pode ser invocado num contexto invalido.

Keybindings usam chord com prefixo proprio (`ctrl+alt+u`, `cmd+alt+u` no mac), nao combinacoes de
uma tecla so. O prefixo precisa estar livre no keymap padrao do VS Code - um chord de extensao sobre
um atalho nativo inutiliza o nativo para quem instala a extensao - e fora das teclas que o `AltGr`
do teclado ABNT2 alcanca, porque no Windows o `AltGr` chega as aplicacoes como `Ctrl+Alt`.

### Conformidade com docs/padroes-codigo.md

A tarefa nao adiciona codigo de producao: `src/` fica identico ao `main`. O codigo novo esta todo nos
testes, que seguem as regras - sem operador ternario, sem `?.` nem `??`, sem `throw`. As propriedades
opcionais das interfaces que descrevem o `package.json` no teste de contrato espelham o schema do
manifesto, onde `when`, `mac` e `icon` podem faltar; funcoes `...Of` normalizam cada uma para valor
nao opcional na leitura, sem propagar.

## Escopo
- `package.json`: `category` e `icon` nos 5 comandos; `shortTitle` onde o rotulo curto ajuda;
  `contributes.menus` (`editor/title` e `editor/context`); `contributes.keybindings`.
- `package.nls.json` + 6 traducoes: remover o prefixo `"Babel TCC: "` dos titulos, adicionar a chave
  `extension.category` e as chaves de `shortTitle`.
- `test/manifest/manifest.test.ts` (novo): testes de contrato entre `package.json`, os 7 arquivos
  nls, os comandos de `constants.ts`, o registro de linguagens (`languages.ts`) e as constantes de
  scheme da visao traduzida.
- `test/l10n/bundles.test.ts` (novo): contrato entre as chamadas `vscode.l10n.t()` do `src/` e os 6
  bundles de traducao de runtime.
- `test/__mocks__/vscode.ts`: capturar chamadas de `setContext`.
- `test/extension.test.ts`: garantir que a ativacao nao publica context key.
- `docs/decisoes-tecnicas.md`: registrar DT-012.
- `docs/user-guide/getting-started.md`: botao, menu de contexto e atalhos.

## Fora de escopo
- Qualquer mudanca no comportamento dos comandos. Esta tarefa e 100% superficie de UI.
- Configuracoes navegaveis (`enum` de idiomas) - tarefa 114, no mesmo PR.
- Comando "Abrir Exemplo", walkthrough, diagnostico de ambiente, progresso e auditoria de
  acessibilidade - tarefas seguintes da fase 7, cada uma com o proximo numero livre quando comecar.
- Limpeza dos `activationEvents` redundantes (`onLanguage:visualg` e `onLanguage:portugol-studio`,
  que o VS Code gera automaticamente desde a 1.74 a partir de `contributes.languages`). E legitima e
  toca o mesmo arquivo, mas e mudanca de ativacao, nao de superficie - avaliar em tarefa propria
  para nao misturar risco de regressao com trabalho de descoberta.

## Revisao do PR #185 (2026-10-12)
A primeira versao publicava 4 context keys proprias (`babelTcc.enabled`, `babelTcc.supportedFile`,
`babelTcc.translatedView`, `babelTcc.readonlyView`) a partir do editor ativo, numa classe
`ContextKeyManager`, e usava o prefixo `ctrl+alt+b`. A revisao apontou:

1. `ctrl+alt+b` e o atalho nativo de "Toggle Secondary Side Bar"; com a extensao instalada ele
   deixava de funcionar. A verificacao anterior cobriu o ABNT2, mas nao o keymap do VS Code.
2. `setContext` grava no escopo global e `editor/title` e desenhado por grupo: com a tela dividida, o
   botao de todos os grupos seguia o editor ativo.
3. `babelTcc.readonlyView` era publicada e nenhuma clausula a lia.
4. `ContextKeyManager` tinha construtor, proibido pelo padrao.

As quatro foram resolvidas pela troca descrita em "Decisao" e registradas no DT-012.
