# Arquitetura

## Indice

- [Visao geral](#visao-geral)
- [Camadas](#camadas)
- [Fluxo de traducao](#fluxo-de-traducao)
- [Fluxo reverso](#fluxo-reverso)
- [Decisoes de design](#decisoes-de-design)

## Visao geral

```
+------------------------------------------+
|          VS Code Extension               |
|  (TypeScript)                            |
|                                          |
|  extension.ts                            |
|  +-- StatusBar                           |
|  +-- Commands (toggle, selectLanguage)   |
|  +-- TranslatedContentProvider           |
|  +-- AutoTranslateManager               |
|  +-- CompletionProvider                  |
|  +-- HoverProvider                       |
|  +-- KeywordMapService                   |
+------------------+-----------------------+
                   |
            CoreBridge (JSON via stdin/stdout)
                   |
+------------------+-----------------------+
|          Core Engine                     |
|  (C# / .NET 8)                          |
|                                          |
|  TranslationOrchestrator                 |
|  +-- LanguageRegistry                    |
|  +-- NaturalLanguageProvider             |
|  +-- IdentifierMapper                    |
|  +-- TraduAnnotationParser               |
+------------------+-----------------------+
                   |
+------------------+-----------------------+
|          Language Adapters               |
|                                          |
|  CSharpAdapter (Roslyn)                  |
|  +-- RoslynWrapper                       |
|  +-- CSharpKeywordMap                    |
|                                          |
|  PythonAdapter (subprocesso CPython)     |
|  +-- PythonTokenizerService              |
|  +-- PythonKeywordMap                    |
|  +-- tokenizer_service.py               |
+------------------+-----------------------+
                   |
+------------------+-----------------------+
|          Data Layer                      |
|                                          |
|  programming-languages/                  |
|    csharp/keywords-base.json             |
|    python/keywords-base.json             |
|  natural-languages/                      |
|    pt-br/csharp.json, python.json        |
|    (10 idiomas x 2 linguagens)           |
|  identifier-map.json                     |
+------------------------------------------+
```

## Camadas

| Camada | Responsabilidade | Tecnologia |
|--------|-----------------|------------|
| Presentation | UI, commands, status bar | TypeScript / VS Code API |
| Providers | Content, auto-translate, completion, hover, keyword map | TypeScript |
| CoreBridge | Comunicacao TS <-> C# | JSON via stdin/stdout |
| Orchestration | Coordenar traducao | C# |
| Adapters | Parsear/gerar codigo | C# / Roslyn (C#), subprocesso CPython (Python) |
| Data | Tabelas de traducao | JSON |

## Fluxo de traducao

```mermaid
sequenceDiagram
    participant U as User
    participant E as Extension
    participant CB as CoreBridge
    participant O as Orchestrator
    participant A as Adapter (C#/Python)
    participant P as Provider

    U->>E: Open .cs or .py file
    E->>E: Detect language (LanguageDetector)
    U->>E: Traduzir Codigo (Editavel/Readonly)
    E->>CB: translateToNaturalLanguage(source, ext, pt-br)
    CB->>O: TranslateToNaturalLanguageAsync
    O->>O: ApplyTraduAnnotations (via adapter)
    O->>A: Parse(sourceCode)
    A-->>O: ASTNode tree
    O->>O: Clone AST
    O->>O: TranslateAstForward (keywords + identifiers)
    O->>A: Generate(translatedAst)
    A-->>O: translated source
    O-->>CB: OperationResultGeneric<string>
    CB-->>E: JSON response
    E-->>U: Show translated code
```

## Fluxo reverso

```mermaid
sequenceDiagram
    participant U as User
    participant E as Extension
    participant TCP as TranslatedContentProvider
    participant CB as CoreBridge
    participant O as Orchestrator

    U->>E: Save translated document
    E->>TCP: writeFile (translated content)
    TCP->>CB: translateFromNaturalLanguage(translated, ext, pt-br)
    CB->>O: TranslateFromNaturalLanguageAsync
    O->>O: ReverseSubstituteKeywords
    O->>O: Parse translated code
    O->>O: TranslateAstReverse (keywords + identifiers)
    O->>O: Generate original code
    O-->>CB: OperationResultGeneric<string>
    CB-->>TCP: original code
    TCP->>TCP: Write to disk
    TCP-->>U: "File saved successfully"
```

## Decisoes de design

1. **Arquivo no disco sempre original** (DT-003): Traducao e puramente visual
2. **Comunicacao via processo** (DT-002): stdin/stdout JSON, sem portas de rede
3. **IDs numericos para keywords** (DT-005): Desacopla linguagem de programacao da traducao
4. **Anotacoes tradu** (DT-006): Traducao de identificadores definida pelo desenvolvedor
5. **Roslyn para C#** (DT-001): Parser oficial da Microsoft
6. **Subprocesso CPython para Python**: Tokenizador nativo garante 100% compatibilidade
7. **OperationResult pattern**: Sem exceptions, tratamento explicito de erros
8. **TraduAnnotationParser agnostico**: Desacoplado do Roslyn, funciona com qualquer adapter

Ver detalhes completos em [Decisoes Tecnicas](../decisoes-tecnicas.md).
