# Tarefa 065 - Corrigir violacoes de convencoes em testes C#

## Prioridade: MEDIUM

## Problemas (M2 + M3 + M4 da auditoria)

### M2: 7 campos private/internal em testes C#
Violam CONTRIBUTING.md regra "tudo public". Todos em arquivos de teste.

### M3: 9 usos de nullable (?/??) no codigo C#
3 em Program.cs (necessarios para API boundary com .NET — aceitar como excecao).
6 em testes (corrigir).

### M4: 5 arquivos de teste com multiplas classes
Violam "uma classe por ficheiro". Todos em testes.

## Escopo
- Trocar private/internal para public nos 7 campos de teste
- Remover nullable dos 6 usos em testes
- Extrair classes extras de 5 arquivos de teste para ficheiros separados
- Garantir que todos os 453+ testes continuam passando
