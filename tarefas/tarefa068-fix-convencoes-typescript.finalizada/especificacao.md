# Tarefa 068 - Corrigir violacoes de convencoes TypeScript

## Prioridade: LOW

## Problema (L2 da auditoria)
4 funcoes helper em testes TS sem return type explicito:
- coreBridge.test.ts (2 ocorrencias)
- extension.test.ts (2 ocorrencias)
1 com eslint-disable justificado (manter).

Convencao CONTRIBUTING.md: "Tipos explicitos em parametros e retornos".

## Escopo
- Adicionar return types explicitos nas 4 funcoes helper
- Verificar se ha outros tipos implicitos em producao (1 reportado em producao)
- ESLint deve passar sem erros
