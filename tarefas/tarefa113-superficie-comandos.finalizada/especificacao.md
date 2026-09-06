# Tarefa 113 - Superficie de comandos: menus, keybindings e context keys

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Tornar os 5 comandos da extensao descobriveis fora do Command Palette, por botao na barra de
titulo do editor, menu de contexto e atalhos de teclado - com visibilidade controlada de forma
declarativa por context keys.

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
cláusula `when` sobre context keys publicadas pela extensao, em vez de aparecer sempre e responder
"tipo de arquivo nao suportado" depois do clique. As context keys ficam centralizadas num unico
modulo (`src/ui/contextKeys.ts`), com uma unica funcao de recalculo disparada por mudanca de editor
ativo e por mudanca de configuracao.

As checagens defensivas dentro dos comandos **permanecem**: o Command Palette continua expondo todos
os comandos (INV-02), entao um comando ainda pode ser invocado num contexto invalido.

Keybindings usam chord com prefixo proprio (`ctrl+alt+b`), nao combinacoes de uma tecla so, para nao
colidir com os atalhos padrao do VS Code. Atencao ao publico-alvo brasileiro: no Windows o `AltGr`
do teclado ABNT2 chega as aplicacoes como `Ctrl+Alt`, entao nenhuma tecla do chord pode ser uma das
que o ABNT2 usa com AltGr.

### Conformidade com docs/padroes-codigo.md

O PR #177 unificou os padroes e estendeu as proibicoes absolutas de C# para todas as linguagens,
TypeScript incluido. O codigo novo desta tarefa segue as regras, mesmo onde as classes TypeScript
vizinhas ainda nao seguem:

- **Sem construtor:** `ContextKeyManager` e criado por static factory (`ContextKeyManager.create`).
  As classes TS existentes (`StatusBar`, `ConfigurationService`, `CoreBridge`, `KeywordMapService`)
  usam construtor, mas sao codigo anterior ao padrao unificado - a adequacao delas e trabalho da
  tarefa112, nao desta. Escolha deliberada: nao adicionar desvio novo a pilha que esta sendo limpa.
- **Sem operador ternario:** usar `if/else` explicito.
- **Sem tipos nulos fora da fronteira de API.** `vscode.window.activeTextEditor` e
  `TextEditor | undefined` por definicao da API do VS Code; o nulo e recebido apenas nesse ponto e
  normalizado na hora para string vazia, sem propagar. Mesmo tratamento que o padrao ja aceita para
  as fronteiras com APIs Java no plugin IntelliJ.
- **Sem `private`, sem `throw`, sem valores hardcoded, sem nomes genericos.**

Se alguma dessas regras se mostrar impraticavel na camada de UI do VS Code, a saida e discutir e
alterar `docs/padroes-codigo.md` em PR proprio - nao contornar em silencio.

## Escopo
- `src/ui/contextKeys.ts` (novo): classe `ContextKeyManager` que publica e mantem sincronizadas
  `babelTcc.enabled`, `babelTcc.supportedFile`, `babelTcc.translatedView` e `babelTcc.readonlyView`.
- `src/config/constants.ts`: constante `CONTEXT_KEYS` com os nomes das chaves.
- `src/extension.ts`: instanciar o `ContextKeyManager` e registrar em `context.subscriptions`.
- `package.json`: `category` e `icon` nos 5 comandos; `shortTitle` onde o rotulo curto ajuda;
  `contributes.menus` (`editor/title` e `editor/context`); `contributes.keybindings`.
- `package.nls.json` + 6 traducoes: remover o prefixo `"Babel TCC: "` dos titulos, adicionar a chave
  `extension.category` e as chaves de `shortTitle`.
- `test/ui/contextKeys.test.ts` (novo).
- `test/manifest/manifest.test.ts` (novo): testes de contrato entre `package.json`, os 7 arquivos
  nls e os comandos registrados em `constants.ts`.
- `test/l10n/bundles.test.ts` (novo): contrato entre as chamadas `vscode.l10n.t()` do `src/` e os 6
  bundles de traducao de runtime.
- `test/__mocks__/vscode.ts`: capturar chamadas de `setContext` para os testes de context key.
- `test/extension.test.ts`: cobrir o registro do `ContextKeyManager`.
- `docs/decisoes-tecnicas.md`: registrar DT-012.

## Fora de escopo
- Qualquer mudanca no comportamento dos comandos. Esta tarefa e 100% superficie de UI.
- Walkthrough, comando de exemplo, diagnostico de ambiente e progresso - sao as tarefas 113 a 118.
- Configuracoes navegaveis (`enum` de idiomas) - tarefa 113.
- Limpeza dos `activationEvents` redundantes (`onLanguage:visualg` e `onLanguage:portugol-studio`,
  que o VS Code gera automaticamente desde a 1.74 a partir de `contributes.languages`). E legitima e
  toca o mesmo arquivo, mas e mudanca de ativacao, nao de superficie - avaliar em tarefa propria
  para nao misturar risco de regressao com trabalho de descoberta.
