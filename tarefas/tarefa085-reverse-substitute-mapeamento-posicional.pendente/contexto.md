# Contexto - Tarefa 085

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma (mas resolve o bug L1 do PR #52)

## Arquivos afetados
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (novo metodo diff-based)
- packages/core/MultiLingualCode.Core.Host/Program.cs (expor novo metodo)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (enviar 3 codigos)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (passar original + traduzido anterior)
- packages/core/MultiLingualCode.Core.Tests/ (testes do diff)

## Notas
- Abordagem escolhida: diff de 3 vias no Core (discutida e aprovada com o usuario)
- O QA identificou que mapeamento posicional nao cobre escrita nativa (PR #92 review)
- Diff de 3 vias resolve ambos cenarios (round-trip e escrita nativa) com um unico code path
- A logica vai no Core (compartilhada entre IDEs), nao na extensao
- ReverseSubstituteKeywords char-by-char mantido como fallback
