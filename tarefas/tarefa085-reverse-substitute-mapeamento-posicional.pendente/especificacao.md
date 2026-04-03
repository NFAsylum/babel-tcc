# Tarefa 085 - ReverseSubstituteKeywords com mapeamento posicional

## Prioridade: HIGH

## Problema
O ReverseSubstituteKeywords atual usa scanner char-by-char que nao distingue identificadores de keywords traduzidas quando tem o mesmo texto. Isso causa corrupcao no round-trip (ex: variavel "e" vira "and" em pt-br). O problema foi mitigado com traducoes mais longas (logicoe, logicoou, igual), mas a fragilidade permanece para qualquer traducao futura que coincida com um nome de variavel.

## Solucao: mapeamento posicional

### Fluxo forward (ja existente, adaptar)
1. `TranslateAstForward` traduz KeywordNodes no AST clonado
2. `Generate` aplica substituicoes no source code (sort descendente por posicao)

### Novo: gravar posicoes de keywords traduzidas
Durante o `Generate`, ao aplicar cada replacement de KeywordNode, gravar a posicao resultante (start, end) no codigo traduzido. Retornar esse set de ranges junto com o codigo traduzido.

Opcoes de implementacao:
a) `Generate` retorna tupla `(string code, List<(int Start, int End)> keywordRanges)` — quebra interface
b) Gravar ranges num campo do adapter ou do orchestrator apos Generate
c) Criar modelo `TranslationResult { Code, KeywordRanges }` retornado pelo orchestrator

Recomendacao: opcao (c) — encapsula resultado sem quebrar interfaces existentes.

### Novo: reverse usa ranges em vez de scanner
`ReverseSubstituteKeywords` recebe os ranges de keywords. Para cada range no codigo traduzido:
1. Extrair o texto no range
2. Fazer lookup na tabela de traducao reversa
3. Se encontrou, substituir pelo keyword original

Textos fora dos ranges nao sao tocados — identificadores ficam intactos independente do texto.

### Calculo de posicoes no Generate
O `Generate` aplica replacements em ordem reversa (posicao descendente). Ao aplicar cada replacement de KeywordNode:
- O start da posicao no output e conhecido (e o mesmo start do replacement, pois replacements anteriores nao afetam posicoes anteriores quando aplicados em ordem reversa)
- O end e start + length do texto traduzido

### Impacto na performance
Positivo — elimina o scanner char-by-char que percorre todos os caracteres. O novo approach percorre apenas os N keywords (~20-50 por arquivo tipico). Memoria adicional e uma lista de tuplas de inteiros.

## Escopo
- Criar modelo TranslationResult (ou similar) com code + keywordRanges
- Modificar Generate nos adapters para produzir ranges de keywords
- Modificar ReverseSubstituteKeywords para usar ranges
- Modificar TranslationOrchestrator para passar ranges entre forward e reverse
- Remover scanner char-by-char dos adapters (ou manter como fallback)
- Atualizar testes existentes de ReverseSubstituteKeywords
- Adicionar teste: variavel "e" nao e corrompida no round-trip (sem depender da traducao ser "logicoe")
