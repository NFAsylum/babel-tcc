# Contexto - Tarefa 114

## Dependencias
**Tarefa 113.** Duas razoes concretas:

1. O teste de contrato `test/manifest/manifest.test.ts` nasceu la, e a assercao nova desta tarefa
   (enum x pastas de traducao) entra nele.
2. Ambas tocam `package.json` e os 7 `package.nls*.json`. Em branches separadas saindo do `main`, as
   duas competiriam pelos mesmos arquivos no merge.

Por isso esta tarefa sai da branch `feat/ux-descoberta-comandos`, e nao do `main` - e o guia ja previa
as duas no mesmo PR (PR 1 da etapa).

## Bloqueia
Nada. As tarefas seguintes da fase 7 nao dependem desta.

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json (`contributes.configuration.properties`)
- packages/ide-adapters/vscode/package.nls*.json (7 arquivos)
- packages/ide-adapters/vscode/test/manifest/manifest.test.ts
- packages/ide-adapters/vscode/translations/natural-languages/ (as 10 pastas que o enum precisa espelhar)
- .github/workflows/ci.yml (job `vscode-build-test`: checkout do repositorio de traducoes)
- docs/decisoes-tecnicas.md (DT-013)

## Os 10 locales e seus rotulos

| Codigo | enumItemLabels |
|---|---|
| pt-br | Portugues (Brasil) |
| pt-br-ascii | Portugues (Brasil, sem acentos) |
| en-us | English (US) |
| es-es | Espanol |
| fr-fr | Francais |
| de-de | Deutsch |
| it-it | Italiano |
| ja-jp-romaji | Nihongo (romaji) |
| zh-cn | Zhongwen (jianti) |
| ar-sa | Al-arabiya |

Os rotulos reais usam a grafia nativa (acentos, kana, hanzi, alfabeto arabe); a tabela acima esta em
ASCII so para este arquivo de tarefa nao depender de encoding.

## Notas
- `enumItemLabels` **nao** vai para os nls: o nome de um idioma e o mesmo nos 7 idiomas de interface.
  O que vai para os nls e `enumDescriptions`, que e texto explicativo.
- A ordem do `enum` nao e alfabetica por codigo: comeca pelos dois portugueses (publico-alvo), depois
  ingles, e entao os demais. O teste de contrato compara **conjuntos**, nao sequencia, justamente
  para nao travar essa escolha editorial.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism` em vez de `npm test`.
