# Tarefa 079 - Corrigir syntax highlighting (grammar keywords + tradu regex)

## Prioridade: HIGH

## Problemas

### HIGH-002: mlc-csharp.tmLanguage.json keywords erradas
Arquivo: syntaxes/mlc-csharp.tmLanguage.json

Keywords na grammar nao correspondem as traducoes reais em pt-br-ascii/csharp.json:
- Grammar: `quebrar` | Traducao real: `parar` (break, ID 4)
- Grammar: `lancamento` | Traducao real: `lancar` (throw, ID 63) 
- Grammar: `async` | Traducao real: `assincrono` (async, ID 78)
- Grammar: `de` | Nao corresponde a nenhuma keyword C#

Alem disso, a grammar so funciona para pt-br-ascii. Outros idiomas (pt-br com acentos, es-es, etc.) nao tem highlighting de keywords.

Fix: alinhar keywords na grammar com traducoes reais do pt-br-ascii. Documentar que highlighting de keywords so funciona para pt-br-ascii (limitacao de TextMate grammars estaticas — ver tarefa 074 para solucao com registro central).

### MEDIUM-003: regex de tradu annotations nao matcha formato real
Arquivos: mlc-python.tmLanguage.json, mlc-csharp.tmLanguage.json

Regex atual: `#\s*tradu:.*$` (Python) e `//\s*tradu:.*$` (C#)
Formato real das anotacoes: `# tradu[pt-br]:NomeTraduzido`

O regex espera `tradu:` sem colchetes. O formato real tem `[idioma]:` entre `tradu` e o conteudo. Anotacoes reais nunca sao destacadas.

Fix: corrigir regex para `#\s*tradu(\[.*?\])?:.*$` (Python) e equivalente C#.
