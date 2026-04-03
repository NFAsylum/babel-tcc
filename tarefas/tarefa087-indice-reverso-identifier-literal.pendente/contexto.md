# Contexto - Tarefa 087

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/core/MultiLingualCode.Core/Services/IdentifierMapper.cs
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (literal reverse)
- packages/core/MultiLingualCode.Core.Tests/Services/IdentifierMapperTests.cs
- packages/core/MultiLingualCode.Core.Tests/Services/TranslationOrchestratorTests.cs

## Notas
- Mesmo padrao do ReverseSubstituteKeywords (tarefa 085) mas para identifiers e literais
- O indice reverso e um cache derivado, nao dados novos — deve ser reconstruido quando o mapa muda
- CONTRIBUTING.md: tudo public, sem var, sem throw
