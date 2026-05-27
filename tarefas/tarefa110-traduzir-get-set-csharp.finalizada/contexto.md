# Contexto - Tarefa 110

## Dependencias
Nenhuma.

## Bloqueia
Nenhuma. Coordenada com PR no repo babel-tcc-translations (IDs 89/90 precisam existir nos dois).

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/RoslynWrapper.cs (IsKeywordKind)
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpKeywordMap.cs (TextToId)
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpKeywordMapTests.cs
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/CSharpAdapterTests.cs
- (outro repo) babel-tcc-translations: keywords-base.json, keyword-categories.json, 10x csharp.json

## Notas
- O engine degrada de forma segura se as traducoes ainda nao tiverem 89/90: provider.TranslateKeyword
  falha e o BuildTranslationMap apenas ignora, entao get/set so nao traduzem ate o PR de traducoes
  entrar. Sem crash, sem quebra de CI.
- Descoberto ao revisar por que get nao traduzia ao lado de init em { get; init; }.
