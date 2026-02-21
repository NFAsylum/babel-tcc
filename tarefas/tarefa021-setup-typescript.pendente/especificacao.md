# Tarefa 021 - Setup do projeto TypeScript

## Fase
3 - VS Code Extension

## Objetivo
Finalizar a configuracao do projeto TypeScript da extensao VS Code.

## Escopo
- Estrutura final em packages/ide-adapters/vscode/
- Configurar package.json com:
  - contributes.commands (toggle, selectLanguage, openTranslated, showOriginal)
  - contributes.configuration (multilingual.enabled, multilingual.language)
  - activationEvents (onLanguage:csharp)
- Configurar tsconfig.json (strict, ES2020, CommonJS)
- Configurar build com esbuild ou webpack
- Criar src/extension.ts com activate/deactivate basicos
- Testar: extensao ativa e loga no Output Channel
