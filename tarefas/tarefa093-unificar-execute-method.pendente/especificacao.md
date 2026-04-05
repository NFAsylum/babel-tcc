# Tarefa 093 - Unificar ExecuteMethod e ExecuteMethodPersistent

## Prioridade: HIGH

## Problema
Program.cs tem dois switch statements quase identicos:
- ExecuteMethod (modo single-request, ~45 linhas)
- ExecuteMethodPersistent (modo persistente, ~45 linhas)

Os 5 cases sao copiados. A unica diferenca e como o orchestrator e obtido:
- ExecuteMethod chama CreateOrchestrator() a cada request
- ExecuteMethodPersistent chama GetOrCreateOrchestrator() com cache

Adicionar um novo metodo (ex: "GetLanguageInfo") exige editar ambos os
switch statements. Se um for esquecido, o metodo funciona num modo mas
nao no outro (ja aconteceu com ApplyTranslatedEdits que so existe no
Persistent).

HandleValidateSyntax tambem cria LanguageRegistry novo a cada chamada
em vez de reutilizar.

## Solucao
Separar o routing em dois niveis: metodos que precisam de orchestrator
e metodos que nao precisam.

```csharp
public static async Task<CoreResponse> RouteRequest(
    string method,
    string paramsJson,
    LanguageRegistry registry,
    string translationsPath,
    string projectPath,
    Dictionary<string, TranslationOrchestrator> orchestratorCache)
{
    // Nivel 1: metodos que NAO precisam de orchestrator
    switch (method)
    {
        case "ValidateSyntax":
            return HandleValidateSyntax(paramsJson, registry);
        case "GetSupportedLanguages":
            return HandleGetSupportedLanguages(translationsPath);
    }

    // Nivel 2: metodos que precisam de orchestrator (criado sob demanda)
    string languageCode = ExtractLanguageCode(method, paramsJson);
    TranslationOrchestrator orchestrator = GetOrCreateOrchestrator(
        orchestratorCache, languageCode, translationsPath, projectPath);

    switch (method)
    {
        case "TranslateToNaturalLanguage":
            return await HandleTranslateToNaturalLanguage(orchestrator, paramsJson);
        case "TranslateFromNaturalLanguage":
            return await HandleTranslateFromNaturalLanguage(orchestrator, paramsJson);
        default:
            return new CoreResponse { Success = false, Error = $"Unknown method: {method}" };
    }
}
```

Vantagens:
- Unico ponto de routing (sem duplicacao)
- Orchestrator criado apenas quando necessario (sem desperdicio)
- Sem nullable: metodos do nivel 1 nunca tocam orchestrator
- Sem delegates ambiguos
- ValidateSyntax recebe registry compartilhado (sem criar novo)

Os callers ficam:
- RunSingleRequest: cria registry e cache vazio, chama RouteRequest
- RunPersistent: cria registry e cache persistente, chama RouteRequest

ExtractLanguageCode e um metodo auxiliar que parseia paramsJson para
obter targetLanguage ou sourceLanguage conforme o metodo.

## Escopo
- Unico metodo RouteRequest com routing em dois niveis
- HandleValidateSyntax recebe registry como parametro
- ExtractLanguageCode como helper de parse
- ExecuteMethod e ExecuteMethodPersistent substituidos por RouteRequest
- Zero mudanca de comportamento externo
- Todos os testes existentes devem passar sem alteracao
- Nao usar nullable, delegates com tipos ambiguos, ou any
