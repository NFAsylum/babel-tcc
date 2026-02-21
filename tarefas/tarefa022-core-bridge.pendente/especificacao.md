# Tarefa 022 - CoreBridge: Comunicacao TS <-> C#

## Fase
3 - VS Code Extension

## Objetivo
Implementar a ponte de comunicacao entre a extensao TypeScript e o Core C#.

## Escopo
- Implementar CoreBridge em src/services/CoreBridge.ts
- Protocolo de comunicacao:
  - Spawn processo .NET (dotnet MultiLingualCode.Core.dll)
  - Enviar requests via JSON em stdin
  - Receber responses via JSON em stdout
  - Formato: { "method": "...", "params": {...} } -> { "result": ..., "error": ... }
- Metodos:
  - translateToNaturalLanguage(sourceCode, fileExtension, targetLanguage)
  - translateFromNaturalLanguage(translatedCode, fileExtension, sourceLanguage)
  - validateSyntax(sourceCode, fileExtension)
  - getCompletions(sourceCode, position, fileExtension, language)
- Gerenciamento de ciclo de vida do processo (start, restart, dispose)
- Timeout e tratamento de erros
- Testes unitarios com mock do processo
