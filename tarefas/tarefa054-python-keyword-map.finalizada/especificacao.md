# Tarefa 054 - PythonKeywordMap.cs

## Fase
7 - Suporte a Python

## Objetivo
Criar mapa bidirecional de keywords do Python com IDs numericos unicos.

## Escopo
- Criar `packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonKeywordMap.cs`

### 35 hard keywords do Python 3
False, None, True, and, as, assert, async, await, break, class, continue, def, del, elif, else, except, finally, for, from, global, if, import, in, is, lambda, nonlocal, not, or, pass, raise, return, try, while, with, yield

### Estrutura (baseada em CSharpKeywordMap, sem Roslyn)
- `Dictionary<string, int> TextToId` — keyword texto -> ID numerico
- `Dictionary<int, string> IdToText` — ID -> keyword texto (construido no static constructor)
- `GetMap()` — retorna copia do TextToId
- `GetId(string keywordText)` — retorna ID ou -1
- `GetText(int id)` — retorna texto ou string vazia

### Diferencas em relacao ao CSharpKeywordMap
- Case-sensitive (Python e case-sensitive: `True` != `true`)
- NAO tera metodo `IsKeyword(SyntaxKind)` (isso e especifico do Roslyn)
- A classificacao keyword vs identifier vem do campo `isKeyword` retornado pelo tokenizador Python (tarefa 052)
- IDs sao independentes dos IDs do C# — cada linguagem tem seu proprio espaco de IDs

### Atribuicao de IDs
- Definir IDs de 0 a 34 para as 35 keywords
- Os IDs devem ser consistentes com o `keywords-base.json` que sera criado na tarefa 057
