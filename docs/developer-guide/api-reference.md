# Referencia da API

## Indice

- [Interfaces](#interfaces)
- [Modelos AST](#modelos-ast)
- [Servicos](#servicos)
- [Fluxo de dados](#fluxo-de-dados)

## Interfaces

### ILanguageAdapter

Interface para adaptadores de linguagens de programacao.

```csharp
public interface ILanguageAdapter
{
    string LanguageName { get; }
    string[] FileExtensions { get; }
    string Version { get; }

    ASTNode Parse(string sourceCode);
    string Generate(ASTNode ast);
    Dictionary<string, int> GetKeywordMap();
    string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookupTranslatedKeyword);
    ValidationResult ValidateSyntax(string sourceCode);
    List<string> ExtractIdentifiers(string sourceCode);

    // Metodos de suporte a anotacoes tradu (adicionados na tarefa 055)
    List<TrailingComment> ExtractTrailingComments(string sourceCode);
    List<string> GetIdentifierNamesOnLine(string sourceCode, int line);
    string GetFirstStringLiteralOnLine(string sourceCode, int line);
    (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line);
}
```

Implementacoes disponiveis: `CSharpAdapter` (Roslyn) e `PythonAdapter` (subprocesso CPython).

### INaturalLanguageProvider

Interface para provedores de traducao de idiomas naturais.

```csharp
public interface INaturalLanguageProvider
{
    string LanguageCode { get; }
    string LanguageName { get; }

    Task<OperationResult> LoadTranslationTableAsync(string programmingLanguage);
    OperationResultGeneric<string> TranslateKeyword(int keywordId);
    int ReverseTranslateKeyword(string translatedKeyword);
    OperationResultGeneric<string> GetOriginalKeyword(int keywordId);
    OperationResultGeneric<string> TranslateIdentifier(string identifier, IdentifierContext context);
}
```

## Modelos AST

### ASTNode (base)

```csharp
public abstract class ASTNode
{
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public ASTNode Parent { get; set; }
    public List<ASTNode> Children { get; set; }

    public abstract ASTNode Clone();
    public void CopyBaseTo(ASTNode target);
    public static List<ASTNode> CloneChildren(List<ASTNode> children, ASTNode newParent);
}
```

### KeywordNode

```csharp
public class KeywordNode : ASTNode
{
    public int KeywordId;
    public string Text;
}
```

### IdentifierNode

```csharp
public class IdentifierNode : ASTNode
{
    public string Name;
    public string TranslatedName;
    public bool IsTranslatable;
}
```

### LiteralNode

```csharp
public class LiteralNode : ASTNode
{
    public object Value;
    public LiteralType Type;
    public bool IsTranslatable;
}
```

### ExpressionNode

```csharp
public class ExpressionNode : ASTNode
{
    public string ExpressionKind;
    public string RawText;
}
```

### StatementNode

```csharp
public class StatementNode : ASTNode
{
    public string StatementKind;
    public string RawText;
}
```

## Servicos

### TranslationOrchestrator

Coordena o fluxo completo de traducao.

```csharp
public class TranslationOrchestrator
{
    // Traduzir codigo para idioma natural
    public async Task<OperationResultGeneric<string>> TranslateToNaturalLanguageAsync(
        string sourceCode, string fileExtension, string targetLanguage);

    // Traduzir de idioma natural para linguagem de programacao
    public async Task<OperationResultGeneric<string>> TranslateFromNaturalLanguageAsync(
        string translatedCode, string fileExtension, string sourceLanguage);
}
```

### IdentifierMapper

Gerencia mapeamentos bidirecionais de identificadores.

```csharp
public class IdentifierMapper
{
    public OperationResult LoadMap(string projectPath);
    public void SetTranslation(string identifier, string language, string translation);
    public OperationResult<string> GetTranslation(string identifier, string language);
    public OperationResult<string> GetOriginal(string translated, string language);
}
```

### LanguageRegistry

Regista e obtem adaptadores de linguagens.

```csharp
public class LanguageRegistry
{
    public OperationResult RegisterAdapter(ILanguageAdapter adapter);
    public OperationResult<ILanguageAdapter> GetAdapter(string fileExtension);
}
```

## Fluxo de dados

```mermaid
graph TD
    A[Source Code] --> B[ILanguageAdapter.Parse]
    B --> C[ASTNode Tree]
    C --> D[Clone AST]
    D --> E[TranslateAstForward]
    E --> F[Keywords translated via NaturalLanguageProvider]
    E --> G[Identifiers translated via IdentifierMapper]
    F --> H[ILanguageAdapter.Generate]
    G --> H
    H --> I[Translated Source Code]
```

### OperationResult Pattern

Todas as operacoes usam `OperationResult<T>` em vez de exceptions:

```csharp
OperationResult<string> result = orchestrator.TranslateToNaturalLanguageAsync(...);
if (result.IsSuccess)
{
    string translated = result.Value;
}
else
{
    string error = result.ErrorMessage;
}
```
