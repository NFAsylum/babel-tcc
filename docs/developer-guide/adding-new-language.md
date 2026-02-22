# Adicionar Nova Linguagem de Programacao

## Indice

- [Visao geral](#visao-geral)
- [1. Criar o Adapter](#1-criar-o-adapter)
- [2. Criar o KeywordMap](#2-criar-o-keywordmap)
- [3. Registrar no LanguageRegistry](#3-registrar-no-languageregistry)
- [4. Criar tabelas de traducao](#4-criar-tabelas-de-traducao)
- [5. Criar testes](#5-criar-testes)
- [Exemplo completo: PythonAdapter](#exemplo-completo-pythonadapter)

## Visao geral

Para adicionar suporte a uma nova linguagem de programacao, e necessario:

1. Implementar a interface `ILanguageAdapter`
2. Criar um mapa de keywords
3. Registrar o adapter no `LanguageRegistry`
4. Criar tabelas de traducao JSON
5. Criar testes

## 1. Criar o Adapter

Criar ficheiro `LanguageAdapters/PythonAdapter.cs` implementando `ILanguageAdapter`:

```csharp
public class PythonAdapter : ILanguageAdapter
{
    public string LanguageName => "Python";
    public string[] FileExtensions => [".py"];
    public string Version => "1.0.0";

    public ASTNode Parse(string sourceCode)
    {
        // Parsear codigo e criar AST com KeywordNode, IdentifierNode, LiteralNode
    }

    public string Generate(ASTNode ast)
    {
        // Reconstruir codigo a partir da AST modificada
    }

    public Dictionary<string, int> GetKeywordMap() => PythonKeywordMap.Map;

    public ValidationResult ValidateSyntax(string sourceCode)
    {
        // Validar sintaxe
    }

    public List<string> ExtractIdentifiers(string sourceCode)
    {
        // Extrair identificadores
    }
}
```

## 2. Criar o KeywordMap

Criar ficheiro `LanguageAdapters/PythonKeywordMap.cs` com mapeamento keyword -> ID numerico:

```csharp
public class PythonKeywordMap
{
    public static Dictionary<string, int> Map = new()
    {
        ["if"] = 30,
        ["else"] = 18,
        ["elif"] = 100,
        ["for"] = 22,
        ["while"] = 78,
        ["def"] = 101,
        ["class"] = 10,
        ["return"] = 52,
        ["import"] = 102,
        ["from"] = 103
    };

    public static int GetId(string keyword) => Map.GetValueOrDefault(keyword, -1);
}
```

Os IDs numericos devem ser unicos e consistentes com o sistema de IDs (ver `docs/decisoes-tecnicas.md` DT-005).

## 3. Registrar no LanguageRegistry

No codigo que inicializa o sistema:

```csharp
LanguageRegistry registry = new LanguageRegistry();
registry.RegisterAdapter(new CSharpAdapter());
registry.RegisterAdapter(new PythonAdapter()); // Novo adapter
```

## 4. Criar tabelas de traducao

Criar ficheiros JSON no repositorio `babel-tcc-translations`:

```
programming-languages/
  python/
    keywords-base.json    # Keywords originais -> IDs
natural-languages/
  pt-br/
    python.json           # Traducoes PT-BR
```

**keywords-base.json** (formato: keyword -> ID):
```json
{
  "keywords": {
    "if": 30,
    "else": 18,
    "elif": 100,
    "for": 22,
    "while": 78,
    "def": 101,
    "class": 10,
    "return": 52
  }
}
```

**pt-br/python.json** (formato: ID -> traducao):
```json
{
  "version": "1.0.0",
  "languageCode": "pt-br",
  "languageName": "Portugues Brasileiro",
  "programmingLanguage": "Python",
  "translations": {
    "30": "se",
    "18": "senao",
    "100": "senaose",
    "22": "para",
    "78": "enquanto",
    "101": "definir",
    "10": "classe",
    "52": "retornar"
  }
}
```

## 5. Criar testes

Criar ficheiro `MultiLingualCode.Core.Tests/LanguageAdapters/PythonAdapterTests.cs` com testes para:

- `Parse_SimpleFunction_ExtractsKeywords`
- `Parse_ClassDeclaration_ExtractsAll`
- `Generate_TranslatedAst_ProducesCorrectOutput`
- `RoundTrip_SimpleCode_PreservesStructure`

## Exemplo completo: PythonAdapter

Ver `CSharpAdapter.cs` como referencia completa de implementacao. O padrao e:

1. Parsear o codigo em tokens
2. Classificar cada token como keyword, identifier ou literal
3. Criar nos AST com posicoes (start/end) para reconstrucao
4. `Generate()` aplica substituicoes na ordem reversa das posicoes
