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
Usar wrapper functions tipadas por tipo de provider em vez de helper
generico com any[]. Cada wrapper conhece a assinatura exata:

```typescript
const SCHEMES = [TRANSLATED_SCHEME, READONLY_SCHEME];

function registerCompletionForSchemes(
    provider: vscode.CompletionItemProvider,
    trigger: string
): void {
    for (const scheme of SCHEMES) {
        context.subscriptions.push(
            vscode.languages.registerCompletionItemProvider(
                { scheme }, provider, trigger
            )
        );
    }
}

function registerHoverForSchemes(provider: vscode.HoverProvider): void {
    for (const scheme of SCHEMES) {
        context.subscriptions.push(
            vscode.languages.registerHoverProvider({ scheme }, provider)
        );
    }
}

function registerSemanticForSchemes(
    provider: vscode.DocumentSemanticTokensProvider,
    legend: vscode.SemanticTokensLegend
): void {
    for (const scheme of SCHEMES) {
        context.subscriptions.push(
            vscode.languages.registerDocumentSemanticTokensProvider(
                { scheme }, provider, legend
            )
        );
    }
}
```

Uso:
```typescript
registerCompletionForSchemes(completionProvider, '.');
registerHoverForSchemes(hoverProvider);
registerSemanticForSchemes(semanticProvider, SEMANTIC_TOKENS_LEGEND);
```

Vantagens:
- Type safety mantida (cada wrapper tem assinatura exata)
- Sem any[] — TypeScript detecta erros de parametro em compile time
- Adicionar novo scheme: mudar SCHEMES em 1 lugar
- Adicionar novo provider: 1 chamada em vez de 4 linhas

## Test helpers duplicados
Extrair test-utils.ts com helpers compartilhados:
- makeDocument() (duplicado em completionProvider.test.ts e hoverProvider.test.ts)
- makeContext() (duplicado em coreBridge.test.ts e extension.test.ts)
- makeOutputChannel() (duplicado em coreBridge.test.ts e extension.test.ts)

## Escopo
- Extrair wrapper functions tipadas por tipo de provider
- Reduzir 8 registros para 4 chamadas (sem perder type safety)
- Extrair test-utils.ts com helpers compartilhados
- Atualizar contagem de subscriptions no extension.test.ts se necessario
- ESLint + testes TS passam
- Nao usar any, any[], ou unknown em assinaturas de funcao
