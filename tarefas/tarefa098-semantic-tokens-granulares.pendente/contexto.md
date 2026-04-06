# Contexto - Tarefa 098

## Dependencias
- Tarefa 086 (semantic tokens provider — ja finalizada)

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/src/providers/semanticKeywordProvider.ts
- packages/ide-adapters/vscode/test/providers/semanticKeywordProvider.test.ts

## Notas
- O SemanticKeywordProvider ja funciona com TOKEN_TYPES = ['keyword', 'variable']
- Expandir para tipos mais granulares nao quebra funcionalidade existente
  (keywords que nao forem classificadas usam fallback 'keyword')
- A grammar C# antiga (removida no PR #96) tinha categorias:
  keywords-control, keywords-types, keywords-modifiers, keywords-literals,
  keywords-other — mesma classificacao pode ser reutilizada
- O keywordMap retorna {traduzida: original} — a keyword original e
  usada para classificar
- Identificado na sessao de review: diferenca visual entre ficheiro
  original (muitas cores) e traduzido (cor unica para todas keywords)
