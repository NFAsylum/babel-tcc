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
|  +-- EditInterceptor                     |
|  +-- SaveHandler                         |
|  +-- CompletionProvider                  |
|  +-- HoverProvider                       |
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
+------------------+-----------------------+
                   |
+------------------+-----------------------+
|          Data Layer                      |
|                                          |
|  keywords-base.json                      |
|  pt-br/csharp.json                       |
|  identifier-map.json                     |
+------------------------------------------+
```

## Camadas

| Camada | Responsabilidade | Tecnologia |
|--------|-----------------|------------|
| Presentation | UI, commands, status bar | TypeScript / VS Code API |
| Providers | Content, edit, save, completion, hover | TypeScript |
| CoreBridge | Comunicacao TS <-> C# | JSON via stdin/stdout |
| Orchestration | Coordenar traducao | C# |
| Adapters | Parsear/gerar codigo | C# / Roslyn |
| Data | Tabelas de traducao | JSON |

## Fluxo de traducao

```mermaid
sequenceDiagram
    participant U as User
    participant E as Extension
    participant CB as CoreBridge
    participant O as Orchestrator
    participant A as CSharpAdapter
    participant P as Provider

    U->>E: Open .cs file
    E->>E: Detect C# language
    U->>E: Open Translated View
    E->>CB: translateToNaturalLanguage(source, .cs, pt-br)
    CB->>O: TranslateToNaturalLanguageAsync
    O->>O: ApplyTraduAnnotations
    O->>A: Parse(sourceCode)
    A-->>O: ASTNode tree
    O->>O: Clone AST
    O->>O: TranslateAstForward (keywords + identifiers)
    O->>A: Generate(translatedAst)
    A-->>O: translated source
    O-->>CB: OperationResult<string>
    CB-->>E: JSON response
    E-->>U: Show translated code
```

## Fluxo reverso

```mermaid
sequenceDiagram
    participant U as User
    participant E as Extension
    participant SH as SaveHandler
    participant CB as CoreBridge
    participant O as Orchestrator

    U->>E: Save translated document
    E->>SH: onWillSaveTextDocument
    SH->>CB: translateFromNaturalLanguage(translated, .cs, pt-br)
    CB->>O: TranslateFromNaturalLanguageAsync
    O->>O: Parse translated code
    O->>O: TranslateAstReverse (keywords + identifiers)
    O->>O: Generate original code
    O-->>CB: OperationResult<string>
    CB-->>SH: original C# code
    SH->>SH: Write to disk
    SH-->>U: "File saved successfully"
```

## Decisoes de design

1. **Arquivo no disco sempre original** (DT-003): Traducao e puramente visual
2. **Comunicacao via processo** (DT-002): stdin/stdout JSON, sem portas de rede
3. **IDs numericos para keywords** (DT-005): Desacopla linguagem de programacao da traducao
4. **Anotacoes tradu** (DT-006): Traducao de identificadores definida pelo desenvolvedor
5. **Roslyn para C#** (DT-001): Parser oficial da Microsoft
6. **OperationResult pattern**: Sem exceptions, tratamento explicito de erros

Ver detalhes completos em [Decisoes Tecnicas](../decisoes-tecnicas.md).
