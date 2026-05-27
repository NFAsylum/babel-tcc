# Tarefa 106 - Robustez dos publishes no release

## Fase
6 - Polimento e Deploy

## Objetivo
Tornar o pipeline de release resiliente a falhas transitorias de rede/registry, para que um
timeout em um publish nao bloqueie os outros nem exija rerun manual.

## Escopo
- Adicionar retry com backoff ao `vsce publish` (Marketplace) e ao `ovsx publish` (Open VSX).
- Desacoplar os passos para que a falha de um nao impeca os demais:
  - opcao A: passos com `if: always()` / `continue-on-error` no mesmo job
  - opcao B: jobs separados por destino (Marketplace, Open VSX, GitHub Release)
- Garantir que o GitHub Release seja criado mesmo se um dos registries falhar.
- Reportar claramente no log qual destino falhou (sem mascarar a falha).
- Manter o publish condicional aos secrets (VSCE_PAT / OVSX_PAT), como ja e hoje.
- Validar que a republicacao e idempotente: se uma versao ja foi publicada num registry, o rerun
  do destino que falhou nao quebra por "versao ja existe".
