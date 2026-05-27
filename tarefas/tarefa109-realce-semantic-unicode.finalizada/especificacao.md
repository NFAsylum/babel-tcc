# Tarefa 109 - Realce semantic Unicode-aware (chines, arabe, acentos)

## Fase
6 - Polimento e Deploy

## Objetivo
Corrigir o realce de cor das keywords/identificadores traduzidos para que funcione em qualquer
escrita (chines, arabe, etc.), nao so latim.

## Contexto do bug
O `SemanticKeywordProvider` identificava as palavras a colorir com o regex
`\b[a-zA-ZÀ-ÿ_][a-zA-ZÀ-ÿ0-9_]*\b`. Dois problemas:
1. O intervalo `[a-zA-ZÀ-ÿ]` cobre apenas latim, entao keywords traduzidas para chines (hanzi) ou
   arabe nao casavam, nao recebiam semantic token e caiam na cor padrao do editor.
2. O `\b` e ASCII-only: uma palavra iniciada por letra acentuada perdia a 1a letra
   (`öffentlich` -> `ffentlich`). Latente hoje porque as tabelas de-de usam digrafos ASCII.

Confirmado rodando: o regex antigo retorna `[]` para `抽象 类` e `مجرد فئة`, e captura `ffentlich`
de `öffentlich`.

## Escopo
- Trocar o regex por `/[\p{L}_][\p{L}\p{N}_]*/gu` (Unicode-aware, sem `\b` ASCII).
- Testes cobrindo chines, arabe e palavra com acento inicial (comprimento do token).

## Fora de escopo
- Layout RTL do arabe: e responsabilidade do editor (VS Code/Monaco aplica o Algoritmo
  Bidirecional Unicode). A extensao so fornece texto e cor; nao define direcao de paragrafo.
