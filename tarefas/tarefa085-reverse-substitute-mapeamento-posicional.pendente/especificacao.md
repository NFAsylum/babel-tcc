# Tarefa 085 - Eliminar ambiguidade no reverse translation de keywords

## Prioridade: HIGH

## Problema
O ReverseSubstituteKeywords atual usa scanner char-by-char que nao distingue identificadores de keywords traduzidas quando tem o mesmo texto. Isso causa corrupcao no round-trip (ex: variavel "e" vira "and" em pt-br). O problema foi mitigado com traducoes mais longas (logicoe, logicoou, igual), mas a fragilidade permanece para qualquer traducao futura que coincida com um nome de variavel.

O problema afeta dois cenarios:
1. **Round-trip**: forward translate -> usuario edita -> reverse translate
2. **Escrita nativa**: usuario escreve codigo traduzido do zero (sem forward previo)

## Abordagens avaliadas

### A) Mapeamento posicional (proposta original)
Generate grava posicoes de keywords traduzidas no output. Reverse substitui apenas nessas posicoes.

**Limitacao critica**: so funciona para round-trip (cenario 1). Na escrita nativa (cenario 2), nao existe Generate previo e portanto nao existem ranges. O fallback seria o scanner bugado — o caminho mais comum ficaria sem protecao.

Riscos adicionais:
- Ranges dessincronizam se usuario edita o codigo traduzido (insere/remove caracteres)
- Dois code paths (ranges vs fallback) duplicam logica e testes

### B) Diff-based no Core (sugerida pelo QA)
O sistema ja tem o ficheiro original e o traduzido. Quando o usuario edita o traduzido, o Core compara antes/depois e aplica apenas as diferencas no original. Keywords nunca precisam de reverse — ja estao no original.

**Vantagem**: resolve ambos os cenarios com um unico code path.
**Complexidade**: diffing de codigo com keywords de tamanho diferente requer matching cuidadoso. Edicoes no meio de keywords traduzidas (ex: usuario apaga metade de "enquanto") precisam de tratamento especial.

### C) Tokenizar o codigo traduzido (alternativa)
Usar o tokenizador da linguagem (Roslyn para C#, subprocess para Python) no codigo traduzido. Keywords traduzidas sao tokens NAME (nao keywords reais), mas identifiers tambem sao NAME. A distincao e feita comparando cada token NAME contra a tabela de traducao reversa — se match, e keyword traduzida.

**Vantagem**: funciona para ambos os cenarios. Nao precisa de Generate previo.
**Limitacao**: mesma ambiguidade do scanner char-by-char (variavel "e" ainda matcha keyword "e"). Porem, o tokenizador resolve strings/comments automaticamente (o scanner precisa de skip manual).

### D) Hibrida: mapeamento posicional + tokenizacao como fallback inteligente
- Round-trip: usar ranges do Generate (cenario 1, mais seguro)
- Escrita nativa: tokenizar o codigo traduzido e aplicar heuristicas (cenario 2)
  - Palavras que sao keywords traduzidas E estao em posicao sintatica de keyword (inicio de statement, apos dois-pontos, etc.) sao revertidas
  - Palavras que sao keywords traduzidas mas estao em posicao de identifier (apos `.`, como argumento de funcao, em assignment) nao sao revertidas
- Fallback final: scanner char-by-char (mantido para compatibilidade)

## Decisao
A decidir durante implementacao. A abordagem D e a mais robusta mas mais complexa. A abordagem A resolve o caso mais comum (round-trip) de forma simples. Documentar qual abordagem foi escolhida e os tradeoffs.

## Escopo minimo
Independente da abordagem escolhida:
- Resolver cenario 1 (round-trip) de forma robusta
- Documentar limitacao do cenario 2 (escrita nativa) se nao resolvido
- Teste: variavel "e" nao e corrompida no round-trip com traducao "e" para "and"
- Teste: variavel "si" nao e corrompida com traducao "si" para "if" em es-es
- Zero regressoes nos testes existentes
