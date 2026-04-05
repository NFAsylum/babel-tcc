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
nao no outro.

HandleValidateSyntax tambem cria LanguageRegistry novo a cada chamada
em vez de reutilizar.

## Solucao
Extrair um unico metodo de routing que recebe uma funcao de resolucao
de orchestrator como parametro:

```csharp
public static async Task<CoreResponse> ExecuteMethodCore(
    string method, string paramsJson, string translationsPath,
    string projectPath,
    Func<string, string, string, TranslationOrchestrator> getOrchestrator)
```

- ExecuteMethod: passa `CreateOrchestrator` como delegate
- ExecuteMethodPersistent: passa `GetOrCreateOrchestrator` (com cache closure)
- HandleValidateSyntax: recebe registry como parametro em vez de criar novo

## Escopo
- Extrair ExecuteMethodCore com delegate para orchestrator
- ExecuteMethod e ExecuteMethodPersistent delegam para ExecuteMethodCore
- HandleValidateSyntax recebe registry como parametro
- Zero mudanca de comportamento externo
- Todos os testes existentes devem passar sem alteracao
