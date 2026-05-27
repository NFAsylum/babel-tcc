# Tarefa 110 - Traduzir keywords get/set em C#

## Fase
7 - Internacionalizacao

## Objetivo
Adicionar as keywords contextuais `get` e `set` (acessadores de propriedade) ao conjunto traduzido
em C#, de forma context-aware (sem super-traduzir identificadores chamados get/set).

## Contexto
A tabela de keywords C# tinha 89 entradas e nao incluia get/set. Numa propriedade
`{ get; init; }`, o `init` traduzia mas `get` ficava no original - inconsistencia visual. get/set
sao contextuais: o Roslyn os emite como `GetKeyword`/`SetKeyword` so em acessador e como
`IdentifierToken` quando sao identificadores, entao da para traduzir apenas no contexto certo.

## Escopo (engine - babel-tcc)
- `RoslynWrapper.IsKeywordKind`: aceitar `SyntaxKind.GetKeyword` e `SyntaxKind.SetKeyword`.
- `CSharpKeywordMap.TextToId`: adicionar `get`=89, `set`=90 (total 91).
- Testes: GetId/IsKeyword para get/set; adapter cria KeywordNode em acessador e NAO cria para
  get/set usados como identificadores.

## Escopo (traducoes - babel-tcc-translations, PR separado)
- `keywords-base.json` (csharp): get=89, set=90.
- `keyword-categories.json`: get/set categoria "other".
- 10 idiomas: traducao de 89/90 (script consistente; unicidade por arquivo).

## Notas de performance
Sem impacto: o despacho Roslyn vs Text Scan continua sendo `sourceCode.Contains("tradu")`. get/set
sao 2 entradas a mais no mapa (lookup O(1)). No fast path (Text Scan) get/set traduzem por palavra
(context-blind, como todos os contextuais ja existentes); no caminho Roslyn, context-aware.

## Fora de escopo
`value`, `add`, `remove`: o Roslyn os emite como `IdentifierToken`, exigiriam checagem de contexto
manual (estilo var/dynamic). Ficam para depois.
