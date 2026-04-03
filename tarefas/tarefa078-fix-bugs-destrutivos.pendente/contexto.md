# Contexto - Tarefa 078

## Dependencias
- Nenhuma

## Bloqueia
- Tarefa 045 (deploy v1 — nao publicar com bugs destrutivos)

## Arquivos afetados
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (HIGH-001)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (HIGH-003)

## Notas
- HIGH-001: o Clear foi intencional no design original (limpar e reconstruir a partir de tradu), mas nao considerou que o identifier-map pode ter mapeamentos manuais ou de sessoes anteriores
- HIGH-003: pattern classico de resource leak em async code sem try/finally
