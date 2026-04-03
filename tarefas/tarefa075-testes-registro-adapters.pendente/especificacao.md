# Tarefa 075 - Testes de registro de adapters e consistencia Host/Registry

## Prioridade: MEDIUM

## Problema
Cada adapter novo precisa ser registrado em `CreateRegistry()` no Program.cs. Nao ha teste que verifica que todas as linguagens esperadas estao registradas. Se alguem adicionar um adapter mas esquecer de registra-lo, a falha e silenciosa.

Nota: `CreateOrchestrator()` e `HandleValidateSyntax()` ja compartilham `CreateRegistry()` — a duplicacao de registro foi resolvida anteriormente. O foco desta tarefa e **testes de consistencia**, nao refactoring.

Problema adicional: `HandleValidateSyntax()` instancia `new PythonAdapter()` (com subprocesso) a cada chamada. Isso e ineficiente e cria processos orfaos — ver tarefa 076 para testes de lifecycle.

## Escopo

### 1. Testes de registro
- Teste que verifica que CreateRegistry() registra .cs e .py
- Teste que verifica que HandleValidateSyntax aceita .cs e .py
- Teste que verifica que ambos suportam as mesmas extensoes

### 2. Teste de consistencia Core vs VS Code
- Verificar que extensoes registradas no C# (LanguageRegistry via CreateRegistry) correspondem as do TypeScript (SUPPORTED_EXTENSIONS no languageDetector)
- Pode ser um teste que le ambas as fontes ou uma convencao documentada
