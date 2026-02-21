# Contexto - Tarefa 010

## Dependencias
- Tarefa 006 (ILanguageAdapter deve existir)

## Bloqueia
- Tarefa 013 (TranslationOrchestrator usa LanguageRegistry)

## Arquivos relevantes
- docs/arquitetura.md (camada Core Engine)

## Notas
O registry mapeia extensoes (ex: ".cs") para adapters (ex: CSharpAdapter).
Deve permitir registrar multiplos adapters.
