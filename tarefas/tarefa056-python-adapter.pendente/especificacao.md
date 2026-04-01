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

### ExtractTrailingComments(string sourceCode) -> List<TrailingComment>
- Novo metodo da interface (tarefa 055)
- Usar tokens COMMENT do subprocesso para extrair comentarios `#`
- Remover prefixo `# ` do texto do comentario
