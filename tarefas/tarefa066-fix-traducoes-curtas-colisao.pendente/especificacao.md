# Tarefa 066 - Revisar traducoes curtas com risco de colisao

## Prioridade: LOW

## Problema (L4 da auditoria)
Mesmo mecanismo que B1 (tarefa 062) — ReverseSubstituteKeywords substitui qualquer palavra que match uma keyword traduzida, incluindo variaveis. Traducoes de 2 letras ou menos sao as mais perigosas.

Traducoes com risco:
- es-es: "si"(if), "en"(in), "es"(is), "no"(not)
- it-it: "se"(if), "in"(in), "da"(from)
- de-de: "in"(in), "wo"(where)

Risco menor que B1 porque essas palavras sao nomes de variavel incomuns em codigo real.

## Escopo
- Repositorio: babel-tcc-translations
- Revisar todas as traducoes de 2 letras ou menos em todos os idiomas
- Para cada uma, avaliar se e um nome de variavel plausivel na linguagem alvo
- Trocar as mais perigosas por alternativas mais longas
- Documentar as que foram mantidas com justificativa

## Notas
- en-us nao precisa de revisao — traducoes identicas as keywords originais funcionam no round-trip
- A solucao definitiva e corrigir o ReverseSubstituteKeywords para usar contexto sintatico (bug L1 do PR #52), mas isso e mudanca arquitetural
