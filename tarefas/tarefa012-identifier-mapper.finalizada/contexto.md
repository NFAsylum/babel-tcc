# Contexto - Tarefa 012

## Dependencias
- Tarefa 008 (modelo IdentifierMap)

## Bloqueia
- Tarefa 013 (TranslationOrchestrator usa IdentifierMapper)
- Tarefa 017 (sistema tradu integra com IdentifierMapper)

## Arquivos relevantes
- docs/decisoes-tecnicas.md (DT-006 anotacao tradu)
- docs/plano-geral.txt linhas 1424-1451 (exemplo identifier-map.json)

## Notas
O mapeamento e por projeto (.multilingual/ na raiz do projeto).
Este servico e responsavel apenas pelo storage e lookup dos mapeamentos.
O parsing de anotacoes // tradu: e responsabilidade da Tarefa 017.
