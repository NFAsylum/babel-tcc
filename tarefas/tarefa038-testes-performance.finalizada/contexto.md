# Contexto - Tarefa 038

## Dependencias
- Fases 1-3 completas (extensao funcional para medir)
- Tarefa 019 (TranslationOrchestrator completo para profiling)
- Tarefa 022 (Core bridge para medir comunicacao)

## Bloqueia
- Tarefa 041 (code cleanup pode incluir otimizacoes de performance)
- Tarefa 046 (resultados de performance sao dados para o TCC)

## Arquivos relevantes
- packages/core/src/Services/TranslationOrchestrator.cs (hot path principal)
- packages/core/src/Services/CSharp/ (adapter C# - parsing e traducao)
- packages/ide-adapters/vscode/src/core-bridge.ts (comunicacao TS <-> C#)
- benchmarks/ (diretorio a ser criado para scripts e resultados)
- tests/performance/ (diretorio a ser criado para testes de performance)

## Notas
- Performance e critica para experiencia do usuario - traducao lenta inviabiliza uso interativo.
- Usar BenchmarkDotNet para benchmarks C# se possivel.
- Considerar que o primeiro parsing e mais lento (cold start do Roslyn).
- Caching de AST pode melhorar significativamente traducoes repetidas do mesmo arquivo.
- Resultados de benchmark variam por hardware - documentar specs da maquina usada.
- Para o TCC, graficos de performance sao excelentes para a secao de resultados.
