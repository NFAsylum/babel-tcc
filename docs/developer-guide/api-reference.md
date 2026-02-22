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
    ValidationResult ValidateSyntax(string sourceCode);
    List<string> ExtractIdentifiers(string sourceCode);
}
```

### INaturalLanguageProvider

Interface para provedores de traducao de idiomas naturais.

```csharp
public interface INaturalLanguageProvider
{
    string LanguageCode { get; }

    Task LoadTranslationTableAsync(string programmingLanguage);
    OperationResult<string> TranslateKeyword(int keywordId);
    int ReverseTranslateKeyword(string translatedKeyword);
}
```

## Modelos AST

### ASTNode (base)

```csharp
public class ASTNode
{
    public List<ASTNode> Children;
    public ASTNode Parent;
    public int StartPosition, EndPosition;
    public int StartLine, EndLine;
    public ASTNode Clone();
}
```

### KeywordNode

```csharp
public class KeywordNode : ASTNode
{
    public int KeywordId;
    public string OriginalKeyword;
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
    public async Task<OperationResult<string>> TranslateToNaturalLanguageAsync(
        string sourceCode, string fileExtension, string targetLanguage);

    // Traduzir de idioma natural para linguagem de programacao
    public async Task<OperationResult<string>> TranslateFromNaturalLanguageAsync(
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
    public void RegisterAdapter(ILanguageAdapter adapter);
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
