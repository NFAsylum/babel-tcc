# Contexto - Tarefa 022

## Dependencias
- Tarefa 021 (setup TypeScript)
- Tarefa 019 (Core deve estar funcional para comunicar)

## Bloqueia
- Tarefa 023 (TranslatedContentProvider)
- Tarefa 028 (CompletionProvider)

## Arquivos relevantes
- docs/arquitetura.md (comunicacao TS <-> C#)
- docs/decisoes-tecnicas.md (DT-002 stdin/stdout)
- docs/plano-geral.txt linhas 978-1106 (codigo CoreBridge)

## Notas
Manter processo vivo entre chamadas para evitar overhead de spawn.
O Core precisa de um entry point CLI que receba --method e --params.
