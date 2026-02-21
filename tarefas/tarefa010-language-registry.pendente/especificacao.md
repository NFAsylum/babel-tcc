# Tarefa 010 - LanguageRegistry

## Fase
1 - Core Engine

## Objetivo
Criar servico que registra e gerencia adaptadores de linguagem disponiveis.

## Escopo
- Implementar LanguageRegistry em Core/Services/
  - RegisterAdapter(ILanguageAdapter adapter)
  - GetAdapter(string fileExtension) -> ILanguageAdapter?
  - GetSupportedExtensions() -> string[]
  - IsSupported(string fileExtension) -> bool
- Cache interno de adapters
- Testes: registro, busca, extensao nao suportada
