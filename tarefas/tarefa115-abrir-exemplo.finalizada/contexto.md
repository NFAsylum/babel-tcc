# Contexto - Tarefa 115

## Dependencias
Nenhuma no codigo. Sai do `main`, em paralelo ao PR da tarefa117: as duas mexem no `package.json`, nos
nls, no CHANGELOG e no mock de testes, mas em trechos diferentes, conferidos por simulacao de merge.

## Bloqueia
- Tarefa116 (walkthrough): o passo "experimente" abre um exemplo com este comando, e o passo "edite e
  salve" depende de a copia ser gravavel.

## Arquivos relevantes
- scripts/copy-translations.js (modelo do script de copia)
- .github/workflows/release.yml (passos de copia, chamados um a um)
- packages/ide-adapters/vscode/.vscodeignore (conferido: nao exclui `examples/`)
- packages/ide-adapters/vscode/src/extension.ts (`openTranslatedEditable`, modelo de abrir visao)
- examples/ (os 6 do catalogo: `csharp/HelloWorld/Program.cs`, `csharp/Calculator/Calculator.cs`,
  `python/shapes.py`, `javascript/calculadora.js`, `visualg/media_notas.alg`,
  `portugol-studio/calculadora.por`)

## Notas
- `npx vsce ls` confirmou os 19 arquivos de exemplo no pacote.
- Desde o VS Code 1.74 um comando contribuido ativa a extensao sozinho; nao precisa de
  `onCommand:` em `activationEvents`.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism`.
