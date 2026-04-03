# Contexto - Tarefa 071

## Dependencias
- Nenhuma

## Bloqueia
- Tarefa 072 (testes de round-trip com aspas dependem desta correcao)

## Arquivos afetados
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (extrair metodo)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs (usar helper, corrigir literais)
- Novo arquivo: packages/core/MultiLingualCode.Core/LanguageAdapters/AdapterHelpers.cs (ou similar)

## Notas
- `CollectReplacements` e chamado por ambos CSharpAdapter e PythonAdapter
- Eliminar acoplamento direto entre PythonAdapter e CSharpAdapter
