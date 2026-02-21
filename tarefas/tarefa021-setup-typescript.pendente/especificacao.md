# Tarefa 021 - Setup do projeto TypeScript

## Fase
3 - VS Code Extension

## Objetivo
Finalizar a configuracao do projeto TypeScript da extensao VS Code.

## Escopo
Incremento sobre scaffold da Tarefa 002 (que ja criou tsconfig, package.json basico, ESLint, build scripts e launch.json):
- Adicionar ao package.json:
  - contributes.commands (toggle, selectLanguage, openTranslated, showOriginal)
  - contributes.configuration (multilingual.enabled, multilingual.language)
  - activationEvents (onLanguage:csharp)
- Configurar bundling com esbuild ou webpack
- Criar src/extension.ts com activate/deactivate que registra providers
- Testar: extensao ativa ao abrir arquivo .cs e loga no Output Channel
