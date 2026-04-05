# Tarefa 094 - Simplificar registro de providers na extension.ts

## Prioridade: MEDIUM

## Problema
extension.ts registra cada provider duas vezes (TRANSLATED_SCHEME + READONLY_SCHEME).
Sao 8 registros que seguem o mesmo padrao:

```typescript
const fooRegistration = register({ scheme: TRANSLATED_SCHEME }, provider);
const fooRegistrationReadonly = register({ scheme: READONLY_SCHEME }, provider);
context.subscriptions.push(fooRegistration);
context.subscriptions.push(fooRegistrationReadonly);
```

Adicionar um novo provider requer 4 linhas (2 registros + 2 pushes).
Adicionar um novo scheme (ex: preview) multiplicaria todos os registros.

## Solucao
Extrair helper que registra para ambos os schemes:

```typescript
const SCHEMES = [TRANSLATED_SCHEME, READONLY_SCHEME];

function registerForAllSchemes(
    registerFn: (selector: DocumentSelector, ...args: any[]) => Disposable,
    ...args: any[]
): void {
    for (const scheme of SCHEMES) {
        context.subscriptions.push(registerFn({ scheme }, ...args));
    }
}
```

Tambem considerar extrair helpers de teste duplicados:
- makeDocument() em completionProvider.test.ts e hoverProvider.test.ts (identicos)
- makeContext() e makeOutputChannel() em coreBridge.test.ts e extension.test.ts

## Escopo
- Extrair helper de registro dual-scheme
- Reduzir 8 registros para 4 chamadas
- Extrair test-utils.ts com helpers compartilhados (makeDocument, makeContext, etc.)
- Atualizar contagem de subscriptions no extension.test.ts se necessario
- ESLint + testes TS passam
