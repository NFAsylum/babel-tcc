# Tarefa 084 - Testes de cobertura para MultiLingualCode.Core.Host

## Prioridade: MEDIUM

## Problema
Coverage Report mostra apenas 15% de line rate no MultiLingualCode.Core.Host.
Os unicos metodos testados sao CreateRegistry() e HandleValidateSyntax()
(adicionados na tarefa 075). O restante do Program.cs nao tem cobertura.

## Escopo

### Testes para ExecuteMethod (4 cases + default)
- ExecuteMethod com "TranslateToNaturalLanguage": traduz codigo C# para pt-br
- ExecuteMethod com "TranslateFromNaturalLanguage": reverte traducao
- ExecuteMethod com "ValidateSyntax": valida sintaxe C#
- ExecuteMethod com "GetSupportedLanguages": retorna lista de idiomas
- ExecuteMethod com metodo desconhecido: retorna erro

### Testes para handlers individuais
- HandleTranslateToNaturalLanguage: traduz e retorna resultado
- HandleTranslateToNaturalLanguage com codigo invalido: retorna erro
- HandleTranslateFromNaturalLanguage: reverte e retorna resultado
- HandleGetSupportedLanguages: lista idiomas do diretorio de traducoes
- HandleGetSupportedLanguages com diretorio inexistente: retorna lista vazia

### Testes para GetOrCreateOrchestrator (modo persistente)
- Primeira chamada: cria orchestrator
- Segunda chamada mesmo idioma: retorna do cache
- Chamada com idioma diferente: cria novo orchestrator
- Cache contem ambos apos duas chamadas

### Testes para RunSingleRequest
- Request com metodo valido: retorna exit code 0
- Request com metodo invalido: retorna exit code 1

### Testes para model classes
- CoreResponse: propriedades default
- TranslateRequest: propriedades default
- ReverseTranslateRequest: propriedades default
- ValidateRequest: propriedades default

### Fora de escopo
- RunPersistent (stdin/stdout): testado indiretamente via coreBridge.test.ts
- Main (entry point): dificil de testar em isolamento, coberto via
  RunSingleRequest e RunPersistent

## Arquivos afetados
- packages/core/MultiLingualCode.Core.Tests/Integration/HostTests.cs (NOVO)
- packages/core/MultiLingualCode.Core.Tests/Models/CoreResponseTests.cs (NOVO, opcional)

## Meta de cobertura
De 15% para 60%+ no Host. Os metodos excluidos (Main, RunPersistent) sao
entry points com IO que representam ~40% do codigo.
