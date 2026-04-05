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
Cada caller resolve o orchestrator ANTES de chamar o metodo de routing.
O switch unificado recebe o orchestrator ja pronto:

```csharp
public static async Task<CoreResponse> ExecuteMethodCore(
    string method,
    string paramsJson,
    TranslationOrchestrator orchestrator,
    LanguageRegistry registry,
    string translationsPath)
```

Para metodos que precisam de orchestrator (Translate, Reverse), ele ja
vem resolvido. Para metodos que nao precisam (ValidateSyntax, GetSupportedLanguages),
o registry e translationsPath sao usados diretamente.

Os callers ficam:
- RunSingleRequest: cria orchestrator via CreateOrchestrator(), passa ao Core
- RunPersistent: resolve via GetOrCreateOrchestrator() do cache, passa ao Core

A criacao do orchestrator depende do languageCode que esta dentro do
paramsJson. Para resolver isso sem parsear JSON duas vezes, opcoes:
- Opcao A: parsear o JSON no caller para extrair languageCode, criar
  orchestrator, passar ao Core
- Opcao B: mover os cases que precisam de orchestrator para um sub-metodo
  que recebe apenas a funcao de resolucao, sem delegate generico
- Opcao C: criar um RequestContext record com orchestrator + registry +
  translationsPath e passar como parametro unico

A decisao entre opcoes deve ser tomada durante a implementacao com base
no que resulta em codigo mais limpo.

## Escopo
- Unico switch de routing (sem duplicacao)
- HandleValidateSyntax recebe registry como parametro
- Zero mudanca de comportamento externo
- Todos os testes existentes devem passar sem alteracao
- Nao usar delegates com tipos ambiguos (Func<string, string, string, T>)
