# Fase 1 - Core Engine

**Objetivo:** Implementar motor de traducao central em C#/.NET.
**Pre-requisito:** Fase 0 concluida.

---

## Tarefa 1.1 - Interfaces base

**Escopo:**
- [ ] Implementar `ILanguageAdapter` em `Core/Interfaces/`
  - Propriedades: LanguageName, FileExtensions, Version
  - Metodos: Parse, Generate, GetKeywordMap, ValidateSyntax, ExtractIdentifiers
- [ ] Implementar `IIDEAdapter` em `Core/Interfaces/`
  - Metodos: ShowTranslatedContentAsync, CaptureEditEventAsync, SaveOriginalContentAsync, ProvideAutocompleteAsync, ShowDiagnosticsAsync
- [ ] Implementar `INaturalLanguageProvider` em `Core/Interfaces/`
  - Metodos: LoadTranslationTableAsync, TranslateKeyword, ReverseTranslateKeyword, TranslateIdentifier, SuggestTranslation
- [ ] Criar tipos auxiliares: ValidationResult, EditEvent, CompletionItem, Diagnostic, IdentifierContext
- [ ] Documentar cada interface com XML docs

**Entrega:** Interfaces compilam, documentadas, com testes de contrato basicos.

---

## Tarefa 1.2 - Modelos AST

**Escopo:**
- [ ] Implementar `ASTNode` (classe base abstrata) em `Core/Models/AST/`
  - Propriedades: StartPosition, EndPosition, StartLine, EndLine, Parent, Children
  - Metodos abstratos: ToCode(), Clone()
- [ ] Implementar `KeywordNode` (herda ASTNode)
  - KeywordId, OriginalKeyword
- [ ] Implementar `IdentifierNode` (herda ASTNode)
  - Name, IsTranslatable, TranslatedName
- [ ] Implementar `LiteralNode` (herda ASTNode)
  - Value, Type (String/Number/Boolean/Null), IsTranslatable
- [ ] Implementar `ExpressionNode` e `StatementNode` como containers
- [ ] Testes unitarios: criacao, clone, serializacao

**Entrega:** Hierarquia AST completa com testes passando.

---

## Tarefa 1.3 - Modelos de configuracao e traducao

**Escopo:**
- [ ] Implementar `KeywordTable` em `Core/Models/Translation/`
  - Deserializa keywords-base.json
  - Metodo GetKeywordId(string keyword), GetKeyword(int id)
- [ ] Implementar `LanguageTable` em `Core/Models/Translation/`
  - Deserializa pt-br/csharp.json
  - Metodo GetTranslation(int keywordId), GetKeywordId(string translatedKeyword)
- [ ] Implementar `IdentifierMap` em `Core/Models/Translation/`
  - Deserializa identifier-map.json
  - Bidirecional: original <-> traduzido
- [ ] Implementar `UserPreferences` em `Core/Models/Configuration/`
- [ ] Testes de carga e deserializacao JSON

**Entrega:** Modelos carregam tabelas JSON corretamente, testes passando.

---

## Tarefa 1.4 - Utilitarios e helpers

**Escopo:**
- [ ] Implementar `JsonLoader` em `Core/Utilities/`
  - Carregar e cachear arquivos JSON
  - Validar contra JSON Schema
- [ ] Implementar `FileSystemHelper` em `Core/Utilities/`
  - Resolver caminhos relativos/absolutos
  - Detectar extensao de arquivo
- [ ] Implementar `StringExtensions` em `Core/Utilities/`
  - CamelCase/PascalCase helpers para identificadores
- [ ] Testes unitarios para cada helper

**Entrega:** Utilitarios funcionais com testes.

---

## Tarefa 1.5 - LanguageRegistry

**Escopo:**
- [ ] Implementar `LanguageRegistry` em `Core/Services/`
  - RegisterAdapter(ILanguageAdapter adapter)
  - GetAdapter(string fileExtension) -> ILanguageAdapter?
  - GetSupportedExtensions() -> string[]
  - IsSupported(string fileExtension) -> bool
- [ ] Cache interno de adapters
- [ ] Testes: registro, busca, extensao nao suportada

**Dependencia:** Tarefa 1.1 (interfaces)

**Entrega:** Registry funcional com testes.

---

## Tarefa 1.6 - NaturalLanguageProvider

**Escopo:**
- [ ] Implementar `NaturalLanguageProvider` em `Core/Services/`
  - LoadTranslationTableAsync(string programmingLanguage)
  - TranslateKeyword(int keywordId) -> string
  - ReverseTranslateKeyword(string translated) -> int
  - TranslateIdentifier(string identifier, IdentifierContext context) -> string
  - SuggestTranslation(string originalIdentifier) -> string
- [ ] Cache de tabelas carregadas
- [ ] Suporte a fallback (se traducao nao existe, retorna original)
- [ ] Testes com tabelas reais (PT-BR + C#)

**Dependencia:** Tarefa 1.3 (modelos de traducao)

**Entrega:** Provider carrega tabelas e traduz keywords corretamente.

---

## Tarefa 1.7 - IdentifierMapper

**Escopo:**
- [ ] Implementar `IdentifierMapper` em `Core/Services/`
  - LoadMap(string projectPath)
  - GetTranslation(string identifier, string targetLanguage) -> string?
  - GetOriginal(string translatedIdentifier, string sourceLanguage) -> string?
  - GetLiteralTranslation(string literal, string targetLanguage) -> string?
  - SaveMap(string projectPath)
- [ ] Parser de anotacoes `// tradu:nome`
- [ ] Persistencia em `.multilingual/identifier-map.json`
- [ ] Testes de mapeamento bidirecional

**Dependencia:** Tarefa 1.3 (modelos)

**Entrega:** Mapper funcional que le/grava identifier-map.json.

---

## Tarefa 1.8 - TranslationOrchestrator (esqueleto)

**Escopo:**
- [ ] Implementar `TranslationOrchestrator` em `Core/Services/`
  - Construtor recebe: LanguageRegistry, NaturalLanguageProvider, IdentifierMapper
  - TranslateToNaturalLanguageAsync(sourceCode, fileExtension, targetLanguage) -> string
  - TranslateFromNaturalLanguageAsync(translatedCode, fileExtension, sourceLanguage) -> string
- [ ] Implementar fluxo basico:
  1. Obter adapter pelo fileExtension
  2. Parse codigo -> AST
  3. Carregar tabela de traducao
  4. Percorrer AST traduzindo nodes
  5. Gerar codigo traduzido
- [ ] Fluxo reverso (traduzido -> original)
- [ ] Testes de integracao basicos com mock adapter

**Dependencia:** Tarefas 1.5, 1.6, 1.7

**Entrega:** Orchestrator compila, fluxo basico funciona com mocks.

---

## Dependencias

```
1.1 ──> 1.5
1.3 ──> 1.6
1.3 ──> 1.7
1.1 + 1.2 + 1.4 (paralelas, sem dependencia)
1.5 + 1.6 + 1.7 ──> 1.8
```
