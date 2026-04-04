# Contexto - Tarefa 090

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/core/MultiLingualCode.Core.Host/Program.cs (novo metodo GetKeywordMap)
- packages/ide-adapters/vscode/src/providers/keywordMap.ts (simplificar)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (novo metodo)
- packages/ide-adapters/vscode/src/services/languageDetector.ts (avaliar)

## Notas
- O NaturalLanguageProvider no Core ja tem LoadTranslationTableAsync, TranslateKeyword, ReverseTranslateKeyword
- O KeywordMapService na extensao le os mesmos ficheiros JSON que o NaturalLanguageProvider
- A duplicacao foi introduzida no PR #47 para evitar latencia de IPC para hover/completion. A solucao e cachear o mapa na extensao apos uma unica chamada IPC
