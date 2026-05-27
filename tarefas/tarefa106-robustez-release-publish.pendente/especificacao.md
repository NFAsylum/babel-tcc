# Tarefa 106 - Robustez dos publishes no release

## Fase
6 - Polimento e Deploy

## Objetivo
Tornar o pipeline de release resiliente a falhas transitorias e re-executavel sem publicacao
manual, mantendo semantica fail-fast (o mais proximo de "tudo ou nada" possivel entre servicos
externos).

## Decisao de design (fechada)
**fail-fast + retry + idempotencia.** Sem desacoplar destinos (sem `continue-on-error`).

Justificativa: rollback atomico verdadeiro e impossivel — Marketplace, Open VSX e GitHub Release
sao tres servicos independentes, e o Marketplace nao permite despublicar/republicar uma versao.
Entao "se um falhar, nao publica nenhum" nao se sustenta como rollback. O objetivo real (nunca
publicar a mao) se atinge com retry (cura o transitorio) + idempotencia (um rerun de um clique
completa o que faltou, sem conflito de "versao ja existe"). Mantemos fail-fast porque e o que mais
se aproxima do "tudo ou nada" desejado e evita estados parciais silenciosos.

## Escopo
- **Retry/backoff** nos dois publishes (`vsce publish` e `ovsx publish`), para que blips
  transitorios (ex.: o timeout `Request timeout: /_apis/gallery` do v0.9.1) se curem na mesma
  execucao, sem rerun.
- **Idempotencia por destino**: antes de publicar, verificar se a versao atual ja esta publicada
  naquele registry; se estiver, pular e tratar como sucesso (nao falhar com "versao ja existe").
  Vale para Marketplace e Open VSX.
- **Manter fail-fast**: sequencial, sem `continue-on-error`. Se um destino realmente falhar apos os
  retries, o job falha (sem mascarar). Um rerun de um clique completa os destinos que faltam, e a
  idempotencia evita conflito nos que ja subiram.
- Manter os publishes condicionais aos secrets (VSCE_PAT / OVSX_PAT), como ja e hoje.
- Log claro indicando qual destino falhou e qual foi pulado por ja estar publicado.

## Fora de escopo
- Desacoplar destinos / `continue-on-error` / jobs separados por destino (decidido contra: gera
  estado parcial silencioso, que o usuario nao quer).
- Rollback/despublicacao automatica (impossivel no Marketplace).
