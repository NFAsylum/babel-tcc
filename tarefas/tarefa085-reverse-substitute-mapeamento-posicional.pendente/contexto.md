# Contexto - Tarefa 085

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma (mas resolve o bug L1 do PR #52)

## Arquivos afetados
Dependem da abordagem escolhida. Possivelmente:
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs
- packages/core/MultiLingualCode.Core/LanguageAdapters/AdapterHelpers.cs
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs
- Testes existentes de ReverseSubstituteKeywords em ambos adapters

## Notas
- O bug L1 foi documentado como "limitacao conhecida" no PR #52
- As traducoes corrigidas (logicoe, igual, etc.) continuam validas como mitigacao
- A review do QA (PR #92) identificou que mapeamento posicional nao cobre escrita nativa
- A spec foi reescrita com 4 abordagens avaliadas, decisao adiada para implementacao
- Abordagem diff-based (sugerida pelo QA) e mais robusta mas mais complexa
