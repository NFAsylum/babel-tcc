# Contexto - Tarefa 060

## Dependencias
- Tarefa 056 (PythonAdapter)
- Tarefa 057 (tabelas de traducao)

## Bloqueia
- Nenhuma

## Arquivos relevantes
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpAdapterTests.cs (modelo)
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpKeywordMapTests.cs (modelo)
- packages/core/MultiLingualCode.Core.Tests/Services/LanguageRegistryTests.cs (ja testa multiplos adapters)
- packages/core/MultiLingualCode.Core.Tests/CoreIntegrationTests.cs (modelo de integracao)

## Notas
- LanguageRegistryTests.cs ja tem teste `MultipleAdapters_IndependentRegistrations` com 4 adapters incluindo Python — validar que funciona com o adapter real.
- Testes do CSharpAdapter sao o melhor modelo: cobrem Parse, Generate, RoundTrip, ReverseSubstitute, Validate, ExtractIdentifiers.
- Codigo Python para testes deve cobrir construtos comuns: funcoes, classes, loops, try/except, imports, list comprehensions, decorators.
