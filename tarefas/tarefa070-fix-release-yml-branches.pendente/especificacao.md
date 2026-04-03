# Tarefa 070 - Corrigir release.yml que falha em pushes para branches

## Prioridade: MEDIUM

## Problema (M6 da auditoria)
O release.yml aparece como failure em pushes para branches (nao apenas tags/releases). Nao bloqueia merge mas polui a aba Actions do GitHub.

## Escopo
- Verificar o trigger do release.yml (on.push vs on.release vs on.tags)
- Ajustar para executar apenas em tags de release (ex: v*)
- Ou adicionar condicao de skip para branches normais
