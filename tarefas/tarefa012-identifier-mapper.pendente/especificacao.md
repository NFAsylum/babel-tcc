# Tarefa 012 - IdentifierMapper

## Fase
1 - Core Engine

## Objetivo
Criar servico que mapeia identificadores customizados (variaveis, metodos, classes) entre original e traduzido.

## Escopo
- Implementar IdentifierMapper em Core/Services/
  - LoadMap(string projectPath)
  - GetTranslation(string identifier, string targetLanguage) -> string?
  - GetOriginal(string translatedIdentifier, string sourceLanguage) -> string?
  - GetLiteralTranslation(string literal, string targetLanguage) -> string?
  - SaveMap(string projectPath)
- Persistencia em .multilingual/identifier-map.json
- Testes de mapeamento bidirecional
