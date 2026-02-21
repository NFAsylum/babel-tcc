# Tarefa 013 - TranslationOrchestrator (esqueleto)

## Fase
1 - Core Engine

## Objetivo
Criar o coordenador central de traducao com fluxo basico funcional.

## Escopo
- Implementar TranslationOrchestrator em Core/Services/
  - Construtor recebe: LanguageRegistry, NaturalLanguageProvider, IdentifierMapper
  - TranslateToNaturalLanguageAsync(sourceCode, fileExtension, targetLanguage) -> string
  - TranslateFromNaturalLanguageAsync(translatedCode, fileExtension, sourceLanguage) -> string
- Implementar fluxo basico:
  1. Obter adapter pelo fileExtension
  2. Parse codigo -> AST
  3. Carregar tabela de traducao
  4. Percorrer AST traduzindo nodes
  5. Gerar codigo traduzido
- Fluxo reverso (traduzido -> original)
- Testes de integracao basicos com mock adapter
