# Contexto - Tarefa 116

## Dependencias
- Tarefa115: o passo "Abra um exemplo" usa o comando "Abrir Exemplo", e o passo "Edite e salve"
  depende de a copia do exemplo ser gravavel. A branch sai de `feat/ux-onboarding`.

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json (`contributes.walkthroughs`)
- packages/ide-adapters/vscode/media/walkthrough/ (midia dos passos)
- packages/ide-adapters/vscode/src/commands/openWalkthrough.ts
- packages/ide-adapters/vscode/test/manifest/manifest.test.ts (contrato do walkthrough)
- README.md, secao "Sistema tradu" (destino do link do passo 4)

## Notas
- `npx vsce ls` confirmou os 4 arquivos de midia no pacote.
- Desde o VS Code 1.74 um comando contribuido ativa a extensao sozinho: o botao de um passo funciona
  mesmo antes de a extensao ter ativado.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism`.
