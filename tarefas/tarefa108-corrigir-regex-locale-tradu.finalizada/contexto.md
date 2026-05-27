# Contexto - Tarefa 108

## Dependencias
Nenhuma.

## Bloqueia
Nenhuma estrita. Habilita anotacoes tradu em ja-jp-romaji/pt-br-ascii, util para o material de
demonstracao (tarefa 043) cobrir identificadores em mais idiomas.

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/TraduAnnotationParser.cs (regex)
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/TraduAnnotationParserTests.cs (testes)
- docs/guia-traducoes.md (convencao de codigo de idioma, estava desatualizada)

## Notas
- Bug latente descoberto ao planejar o super script de demonstracao.
- O TranslationOrchestrator ja casa annotation.TargetLanguage com o idioma alvo por igualdade de
  string, entao capturar o codigo completo no regex faz a traducao aplicar de ponta a ponta.
