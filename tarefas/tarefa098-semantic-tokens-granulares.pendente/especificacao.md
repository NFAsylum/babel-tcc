# Tarefa 098 - Semantic tokens granulares para keywords traduzidas

## Prioridade: MEDIUM

## Problema
O SemanticKeywordProvider emite um unico token type `keyword` para todas
as keywords traduzidas. O ficheiro traduzido tem todas as keywords com a
mesma cor. O ficheiro original C# (com grammar C# do VS Code) distingue:
- keyword.control (if, for, while, return) — roxo
- keyword.type (int, string, bool, void) — azul
- keyword.modifier (public, static, abstract) — azul claro
- keyword.literal (true, false, null) — azul escuro
- entity.name.type (nomes de classe) — verde

O resultado e que o ficheiro traduzido parece visualmente "flat" comparado
com o original — menos cores, menos distincao entre tipos de keywords.

## Solucao
Expandir o SemanticTokensLegend com token types mais granulares e
classificar cada keyword traduzida com base na keyword original.

### Classificacao de keywords
O keywordMap retorna {traduzida: original}. Com a keyword original,
classificar:

- control: if, else, for, foreach, while, do, switch, case, default,
  return, break, continue, throw, try, catch, finally, goto, yield
- type: void, int, string, bool, decimal, float, double, long, short,
  byte, char, object, dynamic, var
- modifier: public, protected, private, internal, static, const,
  readonly, virtual, override, abstract, sealed, async, partial,
  extern, volatile, unsafe, fixed
- literal: true, false, null
- other: using, namespace, class, struct, enum, interface, new, this,
  base, typeof, sizeof, as, is, ref, await, delegate, event, operator,
  implicit, explicit, checked, unchecked, lock, out, params

Para Python, classificacao similar:
- control: if, elif, else, for, while, break, continue, return, pass,
  raise, try, except, finally, with, yield
- type: (Python nao tem keywords de tipo builtin)
- modifier: (Python nao tem modifiers como keywords)
- literal: True, False, None
- other: def, class, import, from, as, in, is, not, and, or, lambda,
  global, nonlocal, del, assert

### Implementacao

1. Expandir TOKEN_TYPES no semanticKeywordProvider.ts:
   const TOKEN_TYPES = ['keyword', 'variable', 'type', 'macro'];
   (VS Code mapeia 'keyword' para controle, 'type' para tipos,
   'macro' para modifiers — verificar quais token types o tema
   do utilizador suporta)

2. Criar mapa de classificacao:
   const KEYWORD_CATEGORIES: Record<string, number> = {
     'if': 0, 'else': 0, 'for': 0, ...  // keyword (control)
     'int': 2, 'string': 2, ...          // type
     'public': 3, 'static': 3, ...       // macro (modifier)
     'true': 0, 'false': 0, ...          // keyword (literal)
   };

3. No provideDocumentSemanticTokens, ao emitir token:
   const original = keywordMap[word.toLowerCase()];
   const tokenType = KEYWORD_CATEGORIES[original] ?? 0;
   builder.push(line, col, word.length, tokenType);

4. Mapa de classificacao deve cobrir C# e Python. Keywords que
   nao estao no mapa usam type 0 (keyword generico) como fallback.

### Token types do VS Code
O VS Code suporta estes semantic token types standard:
- keyword: for control flow and general keywords
- type: for type names
- variable: for variables
- function: for function names
- class: for class names
- string, number, regexp, operator, etc.

O tema do utilizador decide as cores. A extensao so precisa emitir
os token types correctos.

## Escopo
- Modificar semanticKeywordProvider.ts: expandir TOKEN_TYPES e legend
- Criar mapa KEYWORD_CATEGORIES para C# e Python
- Usar keyword original do keywordMap para classificar
- Manter token type 'variable' para identifiers (indice 1)
- Testes: verificar que keywords de controle, tipo e modifier
  recebem token types diferentes
