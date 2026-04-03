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

Criar ficheiro implementando `ILanguageAdapter`. Ver `PythonAdapter.cs` como exemplo real de implementacao completa, ou `CSharpAdapter.cs` como referencia Roslyn.

A interface requer os seguintes metodos:

```csharp
public class NovaLinguagemAdapter : ILanguageAdapter
{
    public string LanguageName => "NovaLinguagem";
    public string[] FileExtensions => [".ext"];
    public string Version => "1.0.0";

    public ASTNode Parse(string sourceCode) { /* Parsear codigo em AST */ }
    public string Generate(ASTNode ast) { /* Reconstruir codigo */ }
    public Dictionary<string, int> GetKeywordMap() { /* Mapa keyword -> ID */ }
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookup) { /* Reverter keywords traduzidas */ }
    public ValidationResult ValidateSyntax(string sourceCode) { /* Validar sintaxe */ }
    public List<string> ExtractIdentifiers(string sourceCode) { /* Extrair identificadores */ }

    // Metodos de suporte a anotacoes tradu
    public List<TrailingComment> ExtractTrailingComments(string sourceCode) { /* Extrair comentarios */ }
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line) { /* Identifiers na linha */ }
    public string GetFirstStringLiteralOnLine(string sourceCode, int line) { /* String literal na linha */ }
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line) { /* Escopo do metodo */ }
}
```

## 2. Criar o KeywordMap

Criar ficheiro `LanguageAdapters/Python/PythonKeywordMap.cs` com mapeamento keyword -> ID numerico:

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

## 4b. Configurar scripts de subprocesso no .csproj

Se o adapter usar um script externo (como `tokenizer_service.py` do Python),
o script precisa ser copiado para o output directory durante o build.

Adicionar ao `MultiLingualCode.Core.csproj`:

```xml
<ItemGroup>
  <None Include="LanguageAdapters\NovaLinguagem\script_name.py">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Importante**: Sem essa configuracao, o adapter funciona nos testes unitarios
(que instanciam diretamente) mas falha em runtime no VS Code com "script not found".
O teste `AllNonCSharpSourceFiles_CopiedToOutput` em `BuildDeployTests.cs` detecta
essa omissao automaticamente.

## 5. Criar testes

Criar ficheiro `MultiLingualCode.Core.Tests/LanguageAdapters/PythonAdapterTests.cs` com testes para:

- `Parse_SimpleFunction_ExtractsKeywords`
- `Parse_ClassDeclaration_ExtractsAll`
- `Generate_TranslatedAst_ProducesCorrectOutput`
- `RoundTrip_SimpleCode_PreservesStructure`

## 6. Configurar extensao VS Code

Adicionar a nova linguagem ao registro central em `packages/ide-adapters/vscode/src/config/languages.ts`:

```typescript
export const SUPPORTED_LANGUAGES: LanguageConfig[] = [
  { name: 'CSharp', extensions: ['.cs'], vscodeLangId: 'csharp' },
  { name: 'Python', extensions: ['.py'], vscodeLangId: 'python' },
  { name: 'NovaLinguagem', extensions: ['.ext'], vscodeLangId: 'novalinguagem' },
];
```

Atualizar manualmente o `package.json` (lido estaticamente pelo VS Code):
- `activationEvents`: adicionar `onLanguage:novalinguagem`
- `languages`: adicionar `{ "id": "mlc-novalinguagem" }`
- `grammars`: adicionar entrada para `mlc-novalinguagem`

Criar `syntaxes/mlc-novalinguagem.tmLanguage.json` para syntax highlighting.

O teste de consistencia em `test/config/languages.test.ts` verifica automaticamente que o registro TypeScript esta alinhado com o package.json.

## Implementacoes existentes

- **CSharpAdapter** (`LanguageAdapters/CSharpAdapter.cs`): Usa Roslyn para parsing. Referencia para linguagens com parser .NET nativo.
- **PythonAdapter** (`LanguageAdapters/Python/PythonAdapter.cs`): Usa subprocesso CPython (`tokenizer_service.py`) via JSON Lines. Referencia para linguagens sem parser .NET.

O padrao geral e:

1. Parsear o codigo em tokens (via parser nativo ou subprocesso)
2. Classificar cada token como keyword, identifier ou literal
3. Criar nos AST com posicoes (start/end) para reconstrucao
4. `Generate()` aplica substituicoes na ordem reversa das posicoes
