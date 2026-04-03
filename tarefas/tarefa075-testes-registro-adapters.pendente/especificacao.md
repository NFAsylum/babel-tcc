# Tarefa 075 - Testes de registro de adapters e consistencia Host/Registry

## Prioridade: MEDIUM

## Problema
Cada adapter novo precisa ser registrado manualmente em `CreateOrchestrator()` e `HandleValidateSyntax()` no Program.cs. Esquecer qualquer um causa falha silenciosa. Nao ha teste que verifica que todas as linguagens esperadas estao registradas.

Alem disso, `HandleValidateSyntax()` cria um registry local separado do `CreateOrchestrator()` — se um for atualizado e o outro nao, validacao funciona para uma linguagem mas traducao nao (ou vice-versa).

## Escopo

### 1. Testes de registro
- Teste que verifica que CreateOrchestrator registra todos os adapters esperados (C# e Python)
- Teste que verifica que HandleValidateSyntax aceita todas as extensoes suportadas
- Teste que verifica que ambos os metodos suportam as mesmas extensoes

### 2. Eliminar duplicacao de registro
Avaliar se o registry pode ser criado uma unica vez e compartilhado:
- Extrair metodo `CreateRegistry()` usado por ambos `CreateOrchestrator()` e `HandleValidateSyntax()`
- Ou criar constante com lista de adapters a registrar

### 3. Teste de consistencia Core vs VS Code
- Verificar que extensoes registradas no C# (LanguageRegistry) correspondem as do TypeScript (SUPPORTED_EXTENSIONS)
- Pode ser um teste de integracao que le ambos ou uma convencao documentada
