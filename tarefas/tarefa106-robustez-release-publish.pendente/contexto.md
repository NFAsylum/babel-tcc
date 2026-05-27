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
- Objetivo: que uma falha transitoria de um registry nao bloqueie os demais nem exija rerun manual.
