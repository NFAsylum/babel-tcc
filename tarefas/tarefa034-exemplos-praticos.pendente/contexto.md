# Contexto - Tarefa 034

## Dependencias
- Fase 2 completa (traducao funcional para C# e PT-BR)
- Tarefa 014 (CSharp adapter basico para traduzir os exemplos)
- Tarefa 018 (CSharp adapter avancado para exemplos complexos)
- Tarefa 012 (identifier mapper para .multilingual/identifier-map.json)

## Bloqueia
- Tarefa 031 (README usa exemplos como referencia no Quick Start)
- Tarefa 037 (testes de integracao podem usar exemplos como fixtures)

## Arquivos relevantes
- examples/ (diretorio a ser criado)
- packages/core/src/Models/IdentifierMapping.cs (schema do identifier-map.json)
- packages/core/src/Models/TranslationMap.cs (formato de traducao)
- translations/ (arquivos de traducao de referencia)

## Notas
- Os exemplos servem duplo proposito: documentacao e material de teste.
- Manter os exemplos simples o suficiente para serem compreendidos rapidamente.
- O TodoApp deve demonstrar um cenario realista de projeto pequeno.
- Considerar incluir comentarios nos arquivos .cs explicando o que sera traduzido.
- Os identifier-map.json devem ser criados manualmente como referencia "gold standard".
