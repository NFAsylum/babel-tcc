# Tarefa 038 - Testes de performance

## Fase
5 - Testes e QA

## Objetivo
Medir e otimizar performance da extensao, garantindo tempos de resposta aceitaveis para uso interativo.

## Escopo
- Definir e medir metricas de tempo de traducao:
  - Arquivos pequenos (<100 linhas): meta < 100ms
  - Arquivos medios (100-500 linhas): meta < 500ms
  - Arquivos grandes (500-2000 linhas): meta < 2 segundos
  - Arquivos muito grandes (2000+ linhas): meta < 5 segundos
- Medir uso de memoria:
  - Consumo de memoria do processo Core em idle
  - Consumo de memoria durante traducao
  - Verificar ausencia de memory leaks apos multiplas traducoes
  - Consumo de memoria da extensao VS Code
- Medir tempo de startup:
  - Tempo de ativacao da extensao VS Code
  - Tempo de inicializacao do processo Core
  - Tempo ate primeira traducao disponivel (time-to-interactive)
- Criar benchmarks automatizados:
  - Script de benchmark com arquivos de tamanhos variados
  - Gerar arquivos sinteticos de teste (100, 500, 1000, 2000 linhas)
  - Executar benchmarks e coletar metricas
  - Comparar resultados entre versoes
- Identificar e corrigir gargalos:
  - Profiling do Core C# (dotnet-trace, dotnet-counters)
  - Profiling da Extension TS (VS Code Developer Tools)
  - Otimizar hot paths identificados
  - Considerar caching de traducoes frequentes
- Documentar resultados:
  - Tabela de benchmarks com hardware de referencia
  - Graficos de performance por tamanho de arquivo
  - Recomendacoes para arquivos muito grandes
