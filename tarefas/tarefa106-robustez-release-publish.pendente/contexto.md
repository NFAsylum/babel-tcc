# Contexto - Tarefa 106

## Dependencias
- Tarefa 100 (pipeline de release com Marketplace + Open VSX ja consolidado)

## Bloqueia
Nenhuma.

## Arquivos relevantes
- .github/workflows/release.yml (job Package & Release)

## Notas
- O release v0.9.1 falhou na primeira tentativa por timeout transitorio da API do Marketplace
  (`Request timeout: /_apis/gallery`) no passo `vsce publish`.
- Como Marketplace, Open VSX e GitHub Release rodam sequencialmente no mesmo job, a falha do
  Marketplace fez os outros dois serem pulados. Um rerun manual (`gh run rerun --failed`) resolveu.
- O rerun so funcionou porque nada tinha sido publicado ainda; se o Marketplace tivesse passado e
  o Open VSX falhasse, o rerun bateria em "versao ja existe" no Marketplace — dai a necessidade de
  idempotencia.
- Decisao fechada com o usuario: **fail-fast + retry + idempotencia, sem desacoplar destinos**.
  Rollback atomico verdadeiro e impossivel (Marketplace nao despublica versao); o objetivo de nunca
  publicar a mao vem do retry (cura transitorio) + idempotencia (rerun de um clique completa o resto).
