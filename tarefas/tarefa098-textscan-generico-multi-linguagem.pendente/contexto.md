# Contexto - Tarefa 098

## Dependencias
- Tarefa 061 (Text Scan implementado para C#, benchmark confirmado)

## Bloqueia
- Suporte rapido a novas linguagens (JS, Java, Go, etc.)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/Services/TextScanTranslator.cs (generalizar)
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (integrar)
- packages/core/MultiLingualCode.Core/Interfaces/ILanguageAdapter.cs (GetScanRules)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs (usar Text Scan)
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (extrair regras)
- docs/developer-guide/adding-new-language.md (documentar caminho Text Scan)

## Notas
- PythonAdapter.ReverseSubstituteKeywords (linhas 203-290) ja implementa
  scan linear com skip de # comments e strings Python. Usar como referencia.
- O TextScanTranslator.cs atual tem 175 linhas. A generalizacao deve
  manter a simplicidade — configuracao por dados, nao por heranca.
- Novas linguagens sem tradu nao precisam de ILanguageAdapter completo.
  Um adapter minimo com GetScanRules() + GetKeywordMap() seria suficiente.
