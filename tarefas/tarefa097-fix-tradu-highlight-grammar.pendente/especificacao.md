# Tarefa 097 - Corrigir highlighting de anotacoes tradu na grammar TextMate

## Prioridade: HIGH

## Problema
As anotacoes tradu (ex: `// tradu[pt-br]:Calculadora`) nao recebem highlighting
nos ficheiros traduzidos. O pattern `#comments` na grammar TextMate matcha
`//.*$` ANTES de `#tradu-annotations` ser avaliado. Em TextMate grammars, o
primeiro pattern que matcha na mesma posicao ganha. Resultado: anotacoes tradu
sao capturadas como `comment.line.double-slash` e nunca como `markup.italic.tradu`.

Afecta ambas grammars:
- mlc-csharp.tmLanguage.json: `//.*$` captura `// tradu[pt-br]:...`
- mlc-python.tmLanguage.json: `#.*$` captura `# tradu[pt-br]:...`

O regex tradu foi corrigido no PR #76 (tarefa 079) para matchear o formato
`tradu[idioma]:`, mas nunca funcionou porque o comment pattern tem prioridade.

## Solucao
Modificar o comment pattern para excluir linhas que contem anotacoes tradu.
Usar negative lookahead:

### C# (mlc-csharp.tmLanguage.json)
Antes:
```json
{ "name": "comment.line.double-slash.mlc-csharp", "match": "//.*$" }
```
Depois:
```json
{ "name": "comment.line.double-slash.mlc-csharp", "match": "//(?!\\s*tradu(\\[.*?\\])?:).*$" }
```
O `(?!\\s*tradu...)` e negative lookahead — matcha `//` seguido de qualquer
coisa EXCETO `tradu[idioma]:`. Comentarios normais continuam highlighting.
Anotacoes tradu caem para o pattern `#tradu-annotations`.

### Python (mlc-python.tmLanguage.json)
Antes:
```json
{ "name": "comment.line.number-sign.mlc-python", "match": "#.*$" }
```
Depois:
```json
{ "name": "comment.line.number-sign.mlc-python", "match": "#(?!\\s*tradu(\\[.*?\\])?:).*$" }
```

## Alternativa: inverter a ordem dos patterns
Mover `#tradu-annotations` antes de `#comments` na lista de patterns.
TextMate avalia na ordem da lista — tradu matcharia primeiro.

Problema: nao funciona. TextMate matcha pela POSICAO no texto, nao
pela ordem na lista. Se dois patterns matcham na mesma posicao, o
primeiro na lista ganha. Mas ambos `//.*$` e `//\s*tradu...$` matcham
na posicao do `//`. A ordem da lista IMPORTA neste caso, e inverter
DEVERIA funcionar.

Recomendacao: testar ambas abordagens (inverter ordem vs negative
lookahead). A inversao e mais simples. O negative lookahead e mais
explicito e nao depende da ordem.

## Escopo
- Corrigir comment pattern em mlc-csharp.tmLanguage.json
- Corrigir comment pattern em mlc-python.tmLanguage.json
- Verificar que anotacoes tradu recebem highlighting
- Verificar que comentarios normais continuam com highlighting
