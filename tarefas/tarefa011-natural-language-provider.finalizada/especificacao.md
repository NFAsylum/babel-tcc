# Tarefa 011 - NaturalLanguageProvider

## Fase
1 - Core Engine

## Objetivo
Criar servico que carrega tabelas de traducao e traduz keywords entre IDs e texto.

## Escopo
- Implementar NaturalLanguageProvider em Core/Services/
  - LoadTranslationTableAsync(string programmingLanguage)
  - TranslateKeyword(int keywordId) -> string
  - ReverseTranslateKeyword(string translated) -> int
  - TranslateIdentifier(string identifier, IdentifierContext context) -> string
  - SuggestTranslation(string originalIdentifier) -> string
- Cache de tabelas carregadas
- Suporte a fallback (se traducao nao existe, retorna original)
- Testes com tabelas reais (PT-BR + C#)
