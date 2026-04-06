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

A API de semantic tokens do VS Code tem apenas UM tipo 'keyword' —
nao ha sub-tipos (keyword.control, keyword.type, etc.). A diferenciacao
de cores no ficheiro original vem da grammar TextMate (scopes como
keyword.control.cs, storage.type.cs, storage.modifier.cs), nao de
semantic tokens standard.

Para replicar as cores, usar custom token types e semanticTokenScopes
no package.json para mapear para scopes TextMate que os temas reconhecem:

1. Definir custom token types no semanticKeywordProvider.ts:
   const TOKEN_TYPES = [
     'keywordControl',    // 0: if, for, return, etc.
     'keywordType',       // 1: int, string, void, etc.
     'keywordModifier',   // 2: public, static, abstract, etc.
     'keywordLiteral',    // 3: true, false, null
     'keywordOther',      // 4: using, namespace, class, new, etc.
     'variable',          // 5: identifiers traduzidos
   ];

2. No package.json, contribuir semanticTokenScopes que mapeiam
   custom types para scopes TextMate que os temas ja suportam:
   "contributes": {
     "semanticTokenScopes": [{
       "scopes": {
         "keywordControl": ["keyword.control"],
         "keywordType": ["storage.type"],
         "keywordModifier": ["storage.modifier"],
         "keywordLiteral": ["constant.language"],
         "keywordOther": ["keyword.other"]
       }
     }]
   }
   Isto faz o tema aplicar as mesmas cores que usaria para os
   scopes TextMate equivalentes.

3. Criar mapa de classificacao:
   const KEYWORD_CATEGORIES: Record<string, number> = {
     'if': 0, 'else': 0, 'for': 0, ...     // keywordControl
     'int': 1, 'string': 1, 'void': 1, ... // keywordType
     'public': 2, 'static': 2, ...          // keywordModifier
     'true': 3, 'false': 3, 'null': 3,      // keywordLiteral
     'using': 4, 'namespace': 4, ...         // keywordOther
   };

4. No provideDocumentSemanticTokens, ao emitir token:
   const original = keywordMap[word.toLowerCase()];
   const tokenType = KEYWORD_CATEGORIES[original] ?? 4;
   builder.push(line, col, word.length, tokenType);

5. Mapa de classificacao deve cobrir C# e Python. Keywords que
   nao estao no mapa usam type 4 (keywordOther) como fallback.

## Escopo
- Modificar semanticKeywordProvider.ts: custom TOKEN_TYPES e legend
- Adicionar semanticTokenScopes ao package.json
- Criar mapa KEYWORD_CATEGORIES para C# e Python
- Usar keyword original do keywordMap para classificar
- Manter token type 'variable' para identifiers traduzidos
- Testes: verificar que keywords de controle, tipo e modifier
  recebem token types diferentes
- Verificar visualmente que cores correspondem ao ficheiro original
