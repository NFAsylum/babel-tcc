# Definition of Done - Tarefa 073

- [x] Teste de colisao de identifiers entre .cs e .py no mesmo projeto
- [x] Teste de isolamento por operacao (traduzir .cs depois .py)
- [x] Teste de round-trip multi-linguagem (traduzir ambos, reverter ambos)
- [x] Se colisao confirmada: proposta de fix documentada
- [x] Todos os testes passam

## Bug confirmado

ApplyTraduAnnotations (TranslationOrchestrator.cs:258-260) faz:
1. IdentifierMapperService.Data.Identifiers.Clear()
2. IdentifierMapperService.Data.Literals.Clear()
3. Reconstroi apenas com anotacoes do arquivo atual
4. SaveMap() grava o mapa parcial no disco

Resultado: traduzir arquivo B destroi permanentemente os mapeamentos do
arquivo A no disco. Reverse translation do arquivo A falha.

## Testes que confirmam o bug
- ApplyTraduAnnotations_SequentialFiles_ClearsMemoryBetweenCalls
- ApplyTraduAnnotations_SequentialFiles_DestroysPersistedData
- ApplyTraduAnnotations_RoundTripMultiFile_SecondFileBreaksFirst

Nota: os testes documentam o comportamento atual (destrutivo). Eles PASSAM
porque verificam que o bug EXISTE. Quando o fix for implementado (tarefa 062),
esses testes deverao ser atualizados para verificar preservacao.

## Propostas de fix (ver tarefa 062)
- Opcao A: merge em vez de Clear (nao perder dados de outros arquivos)
- Opcao B: separar identifier-map por linguagem
- Opcao C: nao salvar no disco durante traducao automatica
