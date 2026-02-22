# Arquitetura

## Visao Geral

O projeto segue uma arquitetura em camadas com separacao clara de responsabilidades. O principio fundamental e: **o arquivo no disco esta sempre na linguagem de programacao original**. A traducao e puramente visual, aplicada em tempo real pelo editor.

## Camadas

```
┌─────────────────────────────────────────────────────┐
│           PRESENTATION (VS Code Extension)          │
│  Commands, StatusBar, LanguageSelector              │
├─────────────────────────────────────────────────────┤
│           PROVIDERS (TypeScript)                    │
│  TranslatedContentProvider, EditInterceptor,        │
│  SaveHandler, CompletionProvider, HoverProvider      │
├─────────────────────────────────────────────────────┤
│           CORE BRIDGE (TypeScript -> C#)            │
│  JSON via stdin/stdout, spawn processo .NET         │
├─────────────────────────────────────────────────────┤
│           CORE ENGINE (C#/.NET)                     │
│  TranslationOrchestrator, LanguageRegistry,         │
│  NaturalLanguageProvider, IdentifierMapper          │
├─────────────────────────────────────────────────────┤
│           LANGUAGE ADAPTERS (C#)                    │
│  CSharpAdapter (Roslyn), PythonAdapter (futuro)     │
├─────────────────────────────────────────────────────┤
│           DATA LAYER (JSON)                         │
│  keywords-base.json, pt-br/csharp.json,             │
│  identifier-map.json                                │
└─────────────────────────────────────────────────────┘
```

## Fluxo de Traducao (Codigo -> Idioma Humano)

1. Usuario abre arquivo `.cs` no VS Code
2. Extension detecta linguagem e aciona `TranslatedContentProvider`
3. Provider envia codigo original para `CoreBridge`
4. CoreBridge envia para processo .NET via JSON/stdin
5. `TranslationOrchestrator` recebe o codigo:
   a. `LanguageRegistry` retorna o `CSharpAdapter`
   b. `CSharpAdapter.Parse()` gera AST via Roslyn
   c. `NaturalLanguageProvider` carrega tabela PT-BR
   d. Orchestrator percorre AST traduzindo cada node
   e. Codigo traduzido e gerado a partir da AST
6. CoreBridge retorna codigo traduzido via stdout/JSON
7. Provider exibe codigo traduzido no editor

## Fluxo Reverso (Salvar)

1. Usuario edita codigo traduzido e salva
2. `SaveHandler` captura evento de save
3. Envia codigo traduzido para CoreBridge
4. Orchestrator faz traducao reversa (PT-BR -> C#)
5. Codigo C# original e salvo no disco

## Repositorios

| Repositorio | Conteudo | Tecnologia |
|---|---|---|
| `babel-tcc` | Core Engine + Extension + Exemplos + Docs | C# + TypeScript |
| `babel-tcc-translations` | Tabelas de keywords e traducoes | JSON |

Veja [repos.txt](repos.txt) para detalhes da estrategia de organizacao.

## Interfaces Principais

### ILanguageAdapter
Cada linguagem de programacao suportada implementa esta interface:
- `LanguageName` / `FileExtensions` / `Version` -> identificacao do adapter
- `Parse(code)` -> AST
- `Generate(AST)` -> code
- `GetKeywordMap()` -> mapa keyword->ID
- `ValidateSyntax(code)` -> resultado
- `ExtractIdentifiers(code)` -> lista

### INaturalLanguageProvider
Cada idioma humano e provido por esta interface:
- `LanguageCode` / `LanguageName` -> identificacao do idioma
- `LoadTranslationTableAsync(programmingLanguage)` -> carrega tabela
- `TranslateKeyword(id)` -> texto traduzido
- `ReverseTranslateKeyword(texto)` -> ID
- `TranslateIdentifier(nome, contexto)` -> nome traduzido

### IIDEAdapter
Cada IDE suportada implementa esta interface para integracao visual.

## Modelo AST

```
ASTNode (abstrato)
├── KeywordNode    (keywords: if, class, void, etc.)
├── IdentifierNode (nomes: variaveis, metodos, classes)
├── LiteralNode    (valores: strings, numeros, booleans)
├── ExpressionNode (expressoes compostas)
└── StatementNode  (declaracoes e blocos)
```

## Comunicacao TypeScript <-> C#

O `CoreBridge` comunica com o Core via processo .NET:

```
Extension (TS) ──JSON/stdin──> Processo .NET (C#)
Extension (TS) <──JSON/stdout── Processo .NET (C#)
```

Protocolo:

Request (via argumentos CLI):
```bash
dotnet MultiLingualCode.Core.Host.dll \
  --method TranslateToNaturalLanguage \
  --params '{"sourceCode":"...","fileExtension":".cs","targetLanguage":"pt-br"}' \
  --translations /path/to/translations \
  --project /path/to/project
```

Response (via stdout):
```json
{ "success": true, "result": "codigo traduzido...", "error": "" }
```

## Sistema "tradu"

Desenvolvedores podem anotar identificadores customizados para traducao:

```csharp
public int Add(int a, int b)  // tradu:Somar,a:primeiroNumero,b:segundoNumero
```

Essas anotacoes sao detectadas pelo `TraduAnnotationParser` e registradas no `IdentifierMapper`, que persiste em `.multilingual/identifier-map.json`.

## Extensibilidade

- **Nova linguagem:** Implementar `ILanguageAdapter` + criar tabela de keywords
- **Novo idioma:** Criar JSON de traducao em `natural-languages/<codigo>/`
- **Nova IDE:** Implementar `IIDEAdapter` + criar plugin/extensao
