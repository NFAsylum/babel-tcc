# Tarefa 056 - PythonAdapter.cs implementando ILanguageAdapter

## Fase
7 - Suporte a Python

## Objetivo
Implementar o adapter principal para Python, conectando o tokenizador subprocess com o sistema de traducao.

## Escopo
- Criar `packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs`

### Propriedades
- `LanguageName` -> `"Python"`
- `FileExtensions` -> `[".py"]`
- `Version` -> `"1.0.0"`

### Parse(string sourceCode) -> ASTNode
- Chamar `PythonTokenizerService.Tokenize()` para obter tokens
- Criar `StatementNode` raiz com `StatementKind = "Module"`, `RawText = sourceCode`
- Para cada token do subprocesso:
  - `isKeyword=true` -> `KeywordNode` com `KeywordId` de `PythonKeywordMap.GetId(token.String)`
  - `typeName="NAME"` e `isKeyword=false` -> `IdentifierNode` com `IsTranslatable=true`
  - `typeName="STRING"` -> `LiteralNode` com `Type=LiteralType.String`, `IsTranslatable=true`
  - `typeName="NUMBER"` -> `LiteralNode` com `Type=LiteralType.Number`, `IsTranslatable=false`
- Converter posicoes: tokenizador Python retorna linhas 1-based. Verificar convencao do projeto (CSharpAdapter usa 0-based via Roslyn) e converter se necessario.
- Converter (line, col) para offsets absolutos (StartPosition/EndPosition) para que `Generate()` funcione.

### Generate(ASTNode ast) -> string
- Mesmo padrao do CSharpAdapter: coletar replacements de KeywordNode/IdentifierNode/LiteralNode, ordenar por posicao decrescente, aplicar substituicoes no RawText
- Avaliar se `CSharpAdapter.CollectReplacements()` pode ser reutilizado (e estatico) ou se deve ser extraido para helper compartilhado

### GetKeywordMap() -> Dictionary<string, int>
- Retornar `PythonKeywordMap.GetMap()`

### ReverseSubstituteKeywords(translatedCode, lookupTranslatedKeyword) -> string
- Scanner char-by-char similar ao CSharpAdapter, mas adaptado para Python:
  - Pular comentarios `#` (ate fim da linha)
  - Pular strings com todas as variantes Python: `'`, `"`, `'''`, `"""`, com prefixos `r`, `b`, `f`, `rb`, `fr`, `br`
  - NAO precisa tratar `//`, `/* */`, `@""`, char literals `''` (especificos de C#)
  - Para cada word: chamar `lookupTranslatedKeyword(word)`, se ID >= 0 substituir por `PythonKeywordMap.GetText(id)`

### ValidateSyntax(string sourceCode) -> ValidationResult
- Enviar codigo ao `PythonTokenizerService` — se retornar `ok=false`, criar `Diagnostic` com a mensagem de erro
- Limitacao: tokenizacao valida lexico, nao sintaxe completa (ex: indentacao incorreta pode nao ser detectada). Documentar.

### ExtractIdentifiers(string sourceCode) -> List<string>
- Usar `PythonTokenizerService.Tokenize()` e filtrar tokens onde `typeName="NAME"` e `isKeyword=false`
- Retornar lista distinta

### Metodos de suporte a anotacoes tradu (tarefa 055)
Novos metodos da interface refatorada:

**ExtractTrailingComments(string sourceCode) -> List<TrailingComment>**
- Usar tokens COMMENT do subprocesso para extrair comentarios `#`
- Remover prefixo `# ` do texto do comentario

**GetIdentifierNamesOnLine(string sourceCode, int line) -> List<string>**
- Filtrar tokens NAME (nao-keyword) na linha especificada

**GetFirstStringLiteralOnLine(string sourceCode, int line) -> string**
- Filtrar tokens STRING na linha especificada, retornar o primeiro

**GetContainingMethodRange(string sourceCode, int line) -> (int StartLine, int EndLine)**
Detectar o bloco `def` que contem a linha especificada. Python usa indentacao sintatica, o que torna isso mais complexo que em C# (que usa `{}`).

Algoritmo recomendado usando os tokens do subprocesso:
1. Tokenizar o source code (reutilizar resultado se ja tokenizado)
2. Encontrar a keyword `def` mais proxima **acima** da linha, considerando:
   - Funcoes aninhadas: se houver multiplos `def` acima, escolher o `def` cuja indentacao (coluna) seja <= a indentacao da linha alvo
   - Decoradores: o `StartLine` do metodo deve ser a linha do primeiro `@decorator` acima do `def`, nao o `def` em si
3. Determinar `EndLine` do metodo:
   - A partir da linha seguinte ao `def`, percorrer as linhas do source code
   - O metodo termina quando encontrar uma linha nao-vazia cuja indentacao seja **menor ou igual** a indentacao do `def` (retorno ao nivel do `def` ou superior)
   - Linhas vazias e linhas so com comentario nao encerram o metodo
   - Se chegar ao fim do arquivo, `EndLine` = ultima linha
4. Se nenhum `def` for encontrado acima da linha, retornar `(-1, -1)` indicando que a linha nao esta dentro de um metodo

Edge cases a tratar:
- **Funcoes aninhadas** (`def` dentro de `def`): o algoritmo do passo 2 resolve ao comparar niveis de indentacao
- **Decoradores** (`@decorator` antes do `def`): incluir no range do metodo
- **Metodos de classe**: mesmo algoritmo — `def` dentro de `class` tem indentacao maior
- **Expressoes multi-linha** (parenteses abertos): linhas de continuacao tem indentacao arbitraria mas nao encerram o metodo pois a proxima "real" statement restaura a indentacao
