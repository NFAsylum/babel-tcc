# Contexto - Tarefa 061

## Dependencias
- Tarefa 050 (processo persistente) — baseline de performance
- Dados de benchmark da tarefa 050 como referencia

## Bloqueia
- Nenhuma (pesquisa independente)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (parse/generate via Roslyn)
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (orquestra traducao)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (comunicacao com Core)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (cache de traducoes)
- tarefas/tarefa050-processo-persistente-corebridge.pendente/benchmark.md (dados de referencia)

## Notas
- Esta tarefa e de pesquisa, nao de implementacao
- O desempenho atual e aceitavel para arquivos < 1.000 linhas (24ms)
- O problema existe apenas para arquivos grandes (>5.000 linhas)
- Qualquer otimizacao deve manter compatibilidade com Python adapter
  (que usa tokenizer externo em vez de Roslyn)
