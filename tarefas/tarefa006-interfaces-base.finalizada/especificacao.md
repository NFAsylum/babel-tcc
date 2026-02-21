# Tarefa 006 - Interfaces base

## Fase
1 - Core Engine

## Objetivo
Definir os contratos (interfaces) que todas as camadas do sistema devem seguir.

## Escopo
- Implementar ILanguageAdapter em Core/Interfaces/
  - Propriedades: LanguageName, FileExtensions, Version
  - Metodos: Parse, Generate, GetKeywordMap, ValidateSyntax, ExtractIdentifiers
- Implementar IIDEAdapter em Core/Interfaces/
  - Metodos: ShowTranslatedContentAsync, CaptureEditEventAsync, SaveOriginalContentAsync, ProvideAutocompleteAsync, ShowDiagnosticsAsync
- Implementar INaturalLanguageProvider em Core/Interfaces/
  - Metodos: LoadTranslationTableAsync, TranslateKeyword, ReverseTranslateKeyword, TranslateIdentifier, SuggestTranslation
- Criar tipos auxiliares: ValidationResult, EditEvent, CompletionItem, Diagnostic, IdentifierContext
- Documentar cada interface com XML docs
