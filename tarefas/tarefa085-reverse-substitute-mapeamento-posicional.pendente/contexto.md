# Contexto - Tarefa 085

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma (mas resolve o bug L1 do PR #52 de forma definitiva)

## Arquivos afetados
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (Generate + ReverseSubstituteKeywords)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs (Generate + ReverseSubstituteKeywords)
- packages/core/MultiLingualCode.Core/LanguageAdapters/AdapterHelpers.cs (CollectReplacements produz ranges)
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (passar ranges entre forward e reverse)
- packages/core/MultiLingualCode.Core/Interfaces/ILanguageAdapter.cs (possivelmente nova assinatura)
- Novo modelo: TranslationResult ou similar
- Testes existentes de ReverseSubstituteKeywords em ambos adapters

## Notas
- O scanner char-by-char pode ser mantido como fallback para compatibilidade quando ranges nao estao disponiveis (ex: chamada direta ao ReverseSubstituteKeywords sem contexto de traducao)
- O bug L1 foi documentado como "limitacao conhecida" no PR #52. Esta tarefa resolve a limitacao
- As traducoes corrigidas (logicoe, igual, etc.) continuam validas — esta tarefa adiciona robustez estrutural
