# Contexto - Tarefa 099

## Dependencias
- Tarefa 061 (Text Scan implementado para C#, benchmark confirmado)

## Bloqueia
- Suporte rapido a novas linguagens (JS, Java, Go, etc.)

## Bug existente (PR #122)
O TextScanTranslator atual e hardcoded para C#. Quando usado com Python,
keywords dentro de # comentarios sao traduzidas incorretamente porque
o scanner trata # como preprocessor (C#) em vez de comentario (Python).
O PR #122 restringe Text Scan a C# (adapter.LanguageName == "CSharp").
Esta tarefa resolve o bug ao generalizar o scanner com LanguageScanRules
que configura # como LineComment para Python.

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/Services/TextScanTranslator.cs (generalizar)
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (integrar)
- packages/core/MultiLingualCode.Core/Interfaces/ITextScannable.cs (NOVO — interface separada)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonAdapter.cs (usar Text Scan)
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (extrair regras)
- docs/developer-guide/adding-new-language.md (documentar caminho Text Scan)

## Notas
- PythonAdapter.ReverseSubstituteKeywords (linhas 203-296) ja implementa
  scan linear com skip de # comments e strings Python. Usar como referencia.
- O TextScanTranslator.cs atual tem 181 linhas. A generalizacao deve
  manter a simplicidade — configuracao por dados, nao por heranca.
- Novas linguagens sem tradu nao precisam de ILanguageAdapter completo.
  Um adapter minimo com ITextScannable + GetKeywordMap() seria suficiente.
