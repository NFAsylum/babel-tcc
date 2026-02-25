# Tarefa 050 - Processo Persistente CoreBridge

## Prioridade: BAIXA (pos-v1.0)

## Problema

O `CoreBridge` (TypeScript) faz `spawn('dotnet', args)` para CADA operacao de traducao.
Cada chamada inicializa o runtime .NET, carrega assemblies, parseia JSON de traducao.
Cold start do .NET 8 e de ~200-500ms.

Para traducao em tempo real enquanto o usuario edita, isso e proibitivo.

## Solucao proposta

Migrar para modelo de processo persistente (long-lived process) com stdin/stdout streaming:

1. **Host fica vivo** escutando comandos JSON line-delimited no stdin
2. **CoreBridge** mantem referencia ao processo e envia/recebe mensagens
3. **Protocolo:** uma linha JSON por request, uma linha JSON por response
4. **Lifecycle:** processo inicia quando extensao ativa, encerra quando desativa

### Mudancas no Host (C#)
- Loop principal lendo stdin linha por linha
- Cada linha e um JSON com `method`, `params`, etc.
- Resposta escrita no stdout como uma linha JSON
- Manter estado (translations carregadas, registry) entre chamadas

### Mudancas no CoreBridge (TypeScript)
- `startProcess()`: inicia processo uma vez
- `invokeCore()`: envia JSON no stdin, aguarda resposta no stdout
- `dispose()`: mata o processo
- Timeout por request (nao por processo)
- Reconexao automatica se processo morrer

### Impacto em thread safety
- Com processo persistente, multiplas requests podem chegar em paralelo
- Necessario garantir thread safety nos servicos (tarefa 049 item 8 - ConcurrentDictionary)
- Ou serializar requests com fila

## Arquivos afetados

- `packages/core/MultiLingualCode.Core.Host/Program.cs` (rewrite do Main)
- `packages/ide-adapters/vscode/src/services/coreBridge.ts` (rewrite do invokeCore)

## Dependencias

- Tarefa 049 (thread safety) se optar por paralelismo
