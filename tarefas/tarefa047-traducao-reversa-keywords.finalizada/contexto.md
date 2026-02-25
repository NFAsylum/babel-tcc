# Contexto - Tarefa 047

## Dependencias
- Nenhuma dependencia nao resolvida

## Relacionamentos
- Bloqueia: tarefa045 (deploy v1) - nao e possivel deployar sem traducao reversa
- Bloqueia: tarefa043 (preparacao marketplace) - funcionalidade core quebrada
- Relacionada: DT-003 (arquivo no disco sempre original) - este principio depende da reversao funcional

## Decisoes tecnicas relevantes
- DT-001: Roslyn para parsing - e a causa do problema (Roslyn nao reconhece keywords traduzidas)
- DT-003: Arquivo no disco sempre original - requer que a reversao funcione

## Documentacao da limitacao
- `docs/poc-resultado.md` secao "Round-trip - PARCIALMENTE VALIDADO" documenta esta limitacao
