# Adicionar Nova Linguagem de Programação

## Índice

- [Visão geral](#visão-geral)
- [1. Criar o Adapter](#1-criar-o-adapter)
- [2. Criar o KeywordMap](#2-criar-o-keywordmap)
- [3. Registrar no LanguageRegistry](#3-registrar-no-languageregistry)
- [4. Criar tabelas de tradução](#4-criar-tabelas-de-tradução)
- [4b. Configurar scripts de subprocesso no .csproj](#4b-configurar-scripts-de-subprocesso-no-csproj)
- [5. Criar testes](#5-criar-testes)
- [6. Configurar extensão VS Code](#6-configurar-extensão-vs-code)
- [Caminho rápido: Text Scan (sem parser)](#caminho-rápido-text-scan-sem-parser)
- [Implementações existentes](#implementações-existentes)

## Visão geral

Para adicionar suporte a uma nova linguagem de programação, é necessário:

1. Implementar a interface `ILanguageAdapter`
2. Criar um mapa de keywords
3. Registrar o adapter no `LanguageRegistry`
4. Criar tabelas de tradução JSON
5. Criar testes

## 1. Criar o Adapter

Criar arquivo implementando `ILanguageAdapter`. Ver `PythonAdapter.cs` como exemplo real de implementação completa, ou `CSharpAdapter.cs` como referência Roslyn.

A interface requer os seguintes métodos:

```csharp
public class NovaLinguagemAdapter : ILanguageAdapter
{
    public string LanguageName => "NovaLinguagem";
    public string[] FileExtensions => [".ext"];
    public string Version => "1.0.0";

    public ASTNode Parse(string sourceCode) { /* Parsear código em AST */ }
    public string Generate(ASTNode ast) { /* Reconstruir código */ }
    public Dictionary<string, int> GetKeywordMap() { /* Mapa keyword -> ID */ }
    public string ReverseSubstituteKeywords(string translatedCode, Func<string, int> lookup) { /* Reverter keywords traduzidas */ }
    public ValidationResult ValidateSyntax(string sourceCode) { /* Validar sintaxe */ }
    public List<string> ExtractIdentifiers(string sourceCode) { /* Extrair identificadores */ }

    // Métodos de suporte a anotações tradu
    public List<TrailingComment> ExtractTrailingComments(string sourceCode) { /* Extrair comentários */ }
    public List<string> GetIdentifierNamesOnLine(string sourceCode, int line) { /* Identifiers na linha */ }
    public string GetFirstStringLiteralOnLine(string sourceCode, int line) { /* String literal na linha */ }
    public (int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line) { /* Escopo do método */ }
}
```

## 2. Criar o KeywordMap

Criar arquivo `LanguageAdapters/Python/PythonKeywordMap.cs` com mapeamento keyword -> ID numérico. Os IDs são por linguagem de programação (cada linguagem começa em 0); os valores abaixo são os reais do Python:

```csharp
public class PythonKeywordMap
{
    public static Dictionary<string, int> Map = new()
    {
        ["class"] = 9,
        ["def"] = 11,
        ["elif"] = 13,
        ["else"] = 14,
        ["for"] = 17,
        ["from"] = 18,
        ["if"] = 20,
        ["import"] = 21,
        ["return"] = 30,
        ["while"] = 32
    };

    public static int GetId(string keyword) => Map.GetValueOrDefault(keyword, -1);
}
```

Os IDs numéricos devem ser únicos por linguagem e consistentes com o keywords-base.json (ver `docs/decisoes-tecnicas.md` DT-005).

## 3. Registrar no LanguageRegistry

No código que inicializa o sistema:

```csharp
LanguageRegistry registry = new LanguageRegistry();
registry.RegisterAdapter(new CSharpAdapter());
registry.RegisterAdapter(new PythonAdapter()); // Novo adapter
```

## 4. Criar tabelas de tradução

Criar arquivos JSON no repositório `babel-tcc-translations`:

```
programming-languages/
  python/
    keywords-base.json    # Keywords originais -> IDs
natural-languages/
  pt-br/
    python.json           # Traduções PT-BR
```

**keywords-base.json** (formato: keyword -> ID):
```json
{
  "keywords": {
    "class": 9,
    "def": 11,
    "elif": 13,
    "else": 14,
    "for": 17,
    "if": 20,
    "return": 30,
    "while": 32
  }
}
```

**pt-br/python.json** (formato: ID -> tradução):
```json
{
  "version": "1.0.0",
  "languageCode": "pt-br",
  "languageName": "Português (Brasil)",
  "programmingLanguage": "Python",
  "translations": {
    "9": "classe",
    "11": "definir",
    "13": "senãose",
    "14": "senão",
    "17": "para",
    "20": "se",
    "30": "retornar",
    "32": "enquanto"
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

**Importante**: Sem essa configuração, o adapter funciona nos testes unitários
(que instanciam diretamente) mas falha em runtime no VS Code com "script not found".
O teste `AllNonCSharpSourceFiles_CopiedToOutput` em `BuildDeployTests.cs` detecta
essa omissão automaticamente.

## 5. Criar testes

Criar arquivo `MultiLingualCode.Core.Tests/LanguageAdapters/PythonAdapterTests.cs` com testes para:

- `Parse_SimpleFunction_ExtractsKeywords`
- `Parse_ClassDeclaration_ExtractsAll`
- `Generate_TranslatedAst_ProducesCorrectOutput`
- `RoundTrip_SimpleCode_PreservesStructure`

## 6. Configurar extensão VS Code

Adicionar a nova linguagem ao registro central em `packages/ide-adapters/vscode/src/config/languages.ts`:

```typescript
export const SUPPORTED_LANGUAGES: LanguageConfig[] = [
  { name: 'CSharp', extensions: ['.cs'], vscodeLangId: 'csharp', registersGrammar: false },
  { name: 'Python', extensions: ['.py'], vscodeLangId: 'python', registersGrammar: false },
  { name: 'VisuAlg', extensions: ['.alg'], vscodeLangId: 'visualg', registersGrammar: true },
  { name: 'PortugolStudio', extensions: ['.por'], vscodeLangId: 'portugol-studio', registersGrammar: true },
  { name: 'NovaLinguagem', extensions: ['.ext'], vscodeLangId: 'novalinguagem', registersGrammar: true },
];
```

O campo `registersGrammar` é `false` para linguagens que o VS Code já conhece nativamente (C#, Python — a Microsoft mantém as gramáticas) e `true` para linguagens novas (VisuAlg, Portugol Studio).

Se `registersGrammar` for `true`, atualizar manualmente o `package.json` (lido estaticamente pelo VS Code):
- `activationEvents`: adicionar `onLanguage:novalinguagem`
- `languages`: adicionar `{ "id": "novalinguagem", "extensions": [".ext"], ... }`
- `grammars`: adicionar entrada apontando para `syntaxes/novalinguagem.tmLanguage.json`

Criar `syntaxes/novalinguagem.tmLanguage.json` para syntax highlighting.

O teste de consistência em `test/config/languages.test.ts` verifica automaticamente que o registro TypeScript está alinhado com o package.json.

## Caminho rápido: Text Scan (sem parser)

Para linguagens que precisam apenas de tradução de keywords (sem tradu
annotations), o TextScanTranslator pode ser usado em vez de um parser
completo. Isso elimina a necessidade de subprocess ou parser externo.

1. Criar `LanguageScanRules` para a linguagem (comentários, strings):

```csharp
public static LanguageScanRules MinhaLinguagem = new LanguageScanRules
{
    LineComment = "//",
    BlockCommentStart = "/*",
    BlockCommentEnd = "*/",
    HasTripleQuoteStrings = false,
    HasSingleQuoteStrings = true,
};
```

2. Implementar `ITextScannable` no adapter:

```csharp
public class MinhaLinguagemAdapter : ILanguageAdapter, ITextScannable
{
    public LanguageScanRules GetScanRules() => LanguageScanRules.MinhaLinguagem;
    // ... restante do adapter
}
```

O TranslationOrchestrator detecta automaticamente adapters com
ITextScannable e usa Text Scan (0-1ms) para arquivos sem tradu.
Para arquivos com tradu, cai para o parser completo.

Performance: 0-1ms para qualquer tamanho de arquivo (vs 2-4s com parser).

## Implementações existentes

- **CSharpAdapter** (`LanguageAdapters/CSharpAdapter.cs`): Usa Roslyn para parsing + Text Scan para keyword-only. Implementa ITextScannable.
- **PythonAdapter** (`LanguageAdapters/Python/PythonAdapter.cs`): Usa subprocesso CPython + Text Scan para keyword-only. Implementa ITextScannable.
- **VisuAlgAdapter** e **PortugolStudioAdapter** (`LanguageAdapters/Portugol/`): Keyword-only via Text Scan, sem parser.

O padrão geral é:

1. Parsear o código em tokens (via parser nativo, subprocesso, ou Text Scan)
2. Classificar cada token como keyword, identifier ou literal
3. Criar nós AST com posições (start/end) para reconstrução
4. `Generate()` aplica substituições na ordem reversa das posições
```
