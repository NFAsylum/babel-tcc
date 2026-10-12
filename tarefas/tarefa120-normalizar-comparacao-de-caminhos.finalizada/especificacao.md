# Tarefa 120 - Normalizar a comparacao de caminhos de URI

## Fase
Correcao de defeito (encontrada durante a validacao manual da tarefa113)

## Objetivo
Fazer com que duas URIs que apontam para o mesmo arquivo sejam sempre reconhecidas como iguais
pela extensao, independentemente da caixa da letra do drive que o Windows entregar.

## Contexto

Durante a validacao manual da tarefa113 o comando "Mostrar Codigo Original"
(`babel-tcc.showOriginal`) parou de funcionar em C#: ele abria o arquivo original e o
`AutoTranslateManager` substituia de volta pela visao traduzida no mesmo instante, fechando a aba
do original. Para o usuario, a tela "pisca" e volta ao idioma traduzido.

O `AutoTranslateManager.handleActiveEditorChange` tem uma trava justamente para esse caso: se ja
existe uma aba traduzida daquele arquivo, ele nao faz nada. A trava nao estava segurando.

### Evidencia medida

O `dist/extension.js` foi instrumentado temporariamente para registrar o que a trava compara. Duas
execucoes do **mesmo** cenario, com uma unica aba aberta:

```
--- execucao 1 (bug reproduz) ---
DIAG procurando: [/c:/Users/.../examples/csharp/Calculator/Calculator.cs]
DIAG   aba: scheme=[babel-tcc-translated] path=[/C:/Users/.../Calculator.cs] igual=false
DIAG   aba: scheme=[file]                 path=[/C:/Users/.../Calculator.cs] igual=false
DIAG RESULTADO: nenhuma aba traduzida bateu
AutoTranslate: replaced ... with translated view

--- execucao 2 (funciona) ---
DIAG procurando: [/c:/Users/.../examples/csharp/Calculator/Calculator.cs]
DIAG   aba: scheme=[babel-tcc-translated] path=[/c:/Users/.../Calculator.cs] igual=true
DIAG RESULTADO: encontrou, vai retornar
```

A unica diferenca entre as duas execucoes e a **caixa da letra do drive**: `/c:` contra `/C:`.
Na execucao 1, ate a aba do proprio arquivo `file` veio com `/C:/` enquanto o editor ativo
reportava `/c:/` — o mesmo arquivo, com duas grafias, no mesmo instante.

O Windows nao garante a caixa do drive, e o VS Code propaga o que recebe. Como todas as
comparacoes da extensao usam `===`, que e sensivel a caixa, elas falham de forma intermitente:
o bug aparece ou nao dependendo de como o documento entrou no editor naquela sessao.

## Decisao

Centralizar a comparacao numa unica funcao de normalizacao, e normalizar **apenas a letra do
drive** — nao o caminho inteiro.

Motivo de nao normalizar o caminho todo: em Linux os caminhos sao genuinamente sensiveis a caixa, e
`/home/User/a.cs` e `/home/user/a.cs` sao arquivos diferentes. Baixar tudo para minusculo faria dois
arquivos distintos passarem por iguais. A letra do drive e segura porque em Linux ela nao existe —
a normalizacao vira no-op — e no Windows `C:` e `c:` sao sempre o mesmo volume.

## Escopo

- Modulo novo com a funcao de normalizacao e a de comparacao (nome e local a confirmar na
  implementacao; sugestao: `src/services/uriPaths.ts`, seguindo o precedente de `isTranslatedScheme`,
  que ja e funcao livre exportada e usada por varios modulos).
- Trocar as comparacoes sensiveis a caixa por essa funcao. Sao **12 pontos**, em dois grupos.

  **Comparacoes entre duas URIs** (onde duas grafias do mesmo arquivo se encontram):

  | Arquivo | Comparacao | Consequencia da falha |
  |---|---|---|
  | `autoTranslateManager.ts` | `tab.input.uri.path === path` (`isAnyTranslatedTabOpenForPath`) | **provado**: `showOriginal` e desfeito |
  | `autoTranslateManager.ts` | `tab.input.uri.toString() === uriString` (`closeTab`) | aba nao e fechada, sobra aba duplicada |
  | `autoTranslateManager.ts` | `d.uri.toString() === uri.toString()` (`reloadTranslatedView`) | troca de idioma nao recarrega a visao — o mecanismo do DT-011 |
  | `translatedContentProvider.ts` | `doc.uri.path === originalPath` (`invalidatePath`) | evento de mudanca nao dispara para a visao aberta |

  **Chaves de mapa montadas a partir do caminho.** Este grupo nao apareceu no `grep` inicial e so
  ficou visivel lendo o provider inteiro. Tres desses mapas sao **gravados por uma origem e lidos
  por outra**, que e exatamente onde a caixa diverge:

  | Mapa | Gravado em | Lido em | Consequencia da falha |
  |---|---|---|---|
  | `mtimeMap` | `invalidatePath`, com a URI de `workspace.textDocuments` | `stat()`, com a URI da chamada | `stat` devolve o mtime antigo e o editor nao recarrega (DT-011) |
  | `recentWrites` | `markSelfWrite`, no save | `isRecentSelfWrite`, pelo **file-watcher** do `extension.ts` | a propria escrita nao e suprimida e dispara invalidacao espuria |
  | `cache` | `buildCacheKey`, da URI traduzida | invalidado por prefixo em `invalidatePath`, com o caminho do file-watcher | cache velho nao e limpo |
  | `displayLanguages` | `readFile` | `displayLanguageFor` | idioma errado na traducao reversa do save |
  | `renderedContent` | `readFile` e `doWriteFile` | `doWriteFile` | baseline vazio no merge de 3 vias |

  Corrigir so o primeiro grupo deixaria metade do defeito de pe, inclusive no mecanismo do DT-011.

- Testes unitarios da funcao de normalizacao (drive maiusculo, drive minusculo, caminho sem drive,
  caminho ja normalizado).
- Teste de regressao de `isAnyTranslatedTabOpenForPath` com aba em `/C:/` e busca em `/c:/`.

## Fora de escopo
- Qualquer mudanca de comportamento dos comandos. Isto e correcao de defeito, nao feature.
- O `showOriginal` abrir o original em modo *preview* (sem `preview: false`), o que faz a aba ser
  descartada na proxima abertura. E um incomodo separado, observado no mesmo teste; avaliar em
  tarefa propria para nao misturar com a correcao de comparacao.

## Origem
Encontrada em teste manual exploratorio durante a `tarefa113`, nao por teste automatizado. A trava
`isAnyTranslatedTabOpenForPath` foi introduzida na `tarefa111`, cujo DoD registra
"Uma visao traduzida por arquivo (handleActiveEditorChange via isAnyTranslatedTabOpenForPath)" — e
cuja validacao manual foi "trocar idioma com 2+ **arquivos** abertos". Arquivos diferentes, nunca
duas grafias do mesmo caminho: por isso passou.
