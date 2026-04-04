# Contexto - Tarefa 092

## Dependencias
- Tarefa 090 (mover KeywordMapService para Core — expor mapas via CoreBridge)

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/src/providers/semanticKeywordProvider.ts (expandir)
- packages/core/MultiLingualCode.Core.Host/Program.cs (expor mapa de identifiers)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (novo metodo)

## Notas
- O IdentifierMapper no Core tem mapeamentos bidirecionais (original <-> traduzido)
- Os mapeamentos vem de anotacoes tradu no codigo e do identifier-map.json
- O semantic provider precisa apenas do mapa traduzido -> original para marcar tokens
