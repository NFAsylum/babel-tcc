# Tarefa 008 - Modelos de configuracao e traducao

## Fase
1 - Core Engine

## Objetivo
Criar classes que representam as tabelas de traducao e configuracoes carregadas dos JSONs.

## Escopo
- Implementar KeywordTable em Core/Models/Translation/
  - Deserializa keywords-base.json
  - Metodo GetKeywordId(string keyword), GetKeyword(int id)
- Implementar LanguageTable em Core/Models/Translation/
  - Deserializa pt-br/csharp.json
  - Metodo GetTranslation(int keywordId), GetKeywordId(string translatedKeyword)
- Implementar IdentifierMap em Core/Models/Translation/
  - Deserializa identifier-map.json
  - Bidirecional: original <-> traduzido
- Implementar UserPreferences em Core/Models/Configuration/
- Testes de carga e deserializacao JSON
