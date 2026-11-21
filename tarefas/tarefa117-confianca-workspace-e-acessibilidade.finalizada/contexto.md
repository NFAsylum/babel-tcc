# Contexto - Tarefa 117

## Dependencias
Nenhuma. Sai do `main` (com as tarefas 113 e 114) e nao toca nos arquivos do PR #186.

## Bloqueia
Nada. Ajuda a tarefa115 (Abrir Exemplo): os exemplos sao justamente o tipo de pasta que chega sem
confianca.

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json (`capabilities`)
- packages/ide-adapters/vscode/package.nls*.json (7 arquivos)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (`resolveTranslationsPath`, `startProcess`,
  `ensureProcess`)
- packages/ide-adapters/vscode/src/extension.ts (`checkDotnetInstalled`)
- packages/ide-adapters/vscode/src/ui/statusBar.ts
- packages/ide-adapters/vscode/l10n/bundle.l10n.*.json (6 arquivos)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonTokenizerService.cs (de onde o Core
  tira o Python: `BABEL_TCC_PYTHON` ou `python3`/`python`/`py -3` no PATH)
- docs/decisoes-tecnicas.md (DT-014)

## Notas
- O Core recebe `--project` com a pasta do workspace e le dela so o mapa de identificadores
  (`.multilingual/identifier-map.json`, JSON). Nao executa nada do workspace.
- `restrictedConfigurations` faz o VS Code ignorar o valor **do workspace** da configuracao em Modo
  Restrito; o valor de usuario continua valendo.
- Para testar o Modo Restrito no F5: a pasta precisa estar **fora** de qualquer pasta confiavel (uma
  pasta confiavel passa a confianca para as subpastas, e `examples/` fica dentro do perfil do usuario).
  Criar `C:\babel-restrito`, copiar um exemplo para la, abrir na janela de teste e responder "nao
  confio nos autores".
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism`.
