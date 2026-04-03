# Tarefa 086 - Semantic tokens para syntax highlighting dinamico

## Prioridade: HIGH

## Problema
A grammar TextMate (mlc-csharp.tmLanguage.json) tem keywords traduzidas hardcoded em regex. So funciona para pt-br-ascii. Se o usuario muda para frances, espanhol, ou qualquer outro idioma, as keywords traduzidas nao recebem highlighting — aparecem como texto normal. O mlc-python.tmLanguage.json nem tenta destacar keywords (removido na tarefa 079 porque qualquer tentativa seria hardcoded).

## Solucao: Semantic Tokens API do VS Code

O VS Code tem a API `DocumentSemanticTokensProvider` que permite fornecer tokens programaticamente em runtime, sem dependencia de grammars estaticas. O highlighting e aplicado pelo tema do usuario automaticamente.

### Implementacao

1. **Criar `SemanticKeywordProvider` em `src/providers/`**
   - Implementar `vscode.DocumentSemanticTokensProvider`
   - No `provideDocumentSemanticTokens(document)`:
     a. Obter o KeywordMapService.getMap(document.uri.path) — mapa de keyword traduzida -> original
     b. Percorrer o texto do documento procurando palavras que matchem keywords traduzidas
     c. Para cada match, adicionar um semantic token com tipo `keyword`
   - Retornar `SemanticTokens` construidos via `SemanticTokensBuilder`

2. **Registrar o provider na extension.ts**
   ```typescript
   const legend = new vscode.SemanticTokensLegend(['keyword']);
   vscode.languages.registerDocumentSemanticTokensProvider(
     { scheme: TRANSLATED_SCHEME },
     semanticKeywordProvider,
     legend
   );
   ```

3. **Manter grammars TextMate para comments, strings, numbers**
   As grammars estaticas continuam funcionando para elementos universais (comments, strings, numbers, decorators). Apenas o highlighting de keywords passa a ser semantico.

4. **Remover keywords hardcoded das grammars**
   Limpar as secoes keywords-control, keywords-types, keywords-modifiers, keywords-other do mlc-csharp.tmLanguage.json. Manter apenas comments, strings, numbers, tradu-annotations.

### Vantagens
- Funciona para qualquer idioma automaticamente (usa KeywordMapService que carrega traducoes dinamicamente)
- Funciona para qualquer linguagem (C#, Python, futuras) sem grammar separada por linguagem
- Sem listas hardcoded — tudo derivado das tabelas de traducao
- Consistente com o registro central de linguagens (tarefa 074)

### Notas tecnicas
- Semantic tokens tem prioridade sobre TextMate tokens — se ambos definem o mesmo range, o semantico vence
- O `SemanticTokensBuilder` requer tokens em ordem de posicao (linha, coluna) — nao pode ser aleatorio
- Performance: o KeywordMapService ja cacheia o mapa. O scan do documento e O(palavras * keywords), mas keywords sao ~20-90, e o scan pode ser otimizado com Set lookup O(1) por palavra
- O provider e invocado pelo VS Code quando o documento muda — nao precisa de manual trigger
