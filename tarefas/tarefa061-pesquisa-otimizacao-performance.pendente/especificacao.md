# Tarefa 061 - Pesquisa e avaliacao de otimizacoes de performance

## Fase
Pos-v1.0 (pesquisa)

## Objetivo
Investigar, prototipar e avaliar metodos de otimizacao para reducao do tempo
de traducao em arquivos grandes. Cada metodo deve ser avaliado com benchmarks
reais e criterios de UX definidos.

## Contexto

Desempenho atual (processo persistente, traducao completa via Roslyn):

| Linhas   | Tempo  | Percepcao     |
|----------|--------|---------------|
| 90       | 2ms    | Instantaneo   |
| 1.700    | 24ms   | Instantaneo   |
| 8.500    | 250ms  | Leve delay    |
| 17.000   | 921ms  | Perceptivel   |
| 34.000   | 3.9s   | Ruim          |
| 85.000   | 27.3s  | Inutilizavel  |

Dados para >3.400 linhas obtidos em medicao local separada (mesmo metodo
do benchmark da tarefa 050: processo persistente, 3 rodadas, Release build).

O crescimento e super-linear (~quadratico) porque o Roslyn parseia a AST
inteira e percorre todos os nos para substituir keywords.

Para arquivos tipicos (<1.000 linhas), o desempenho ja e aceitavel.
A pesquisa foca em melhorar o cenario de arquivos grandes (>5.000 linhas)
e interacoes subsequentes (editar, salvar, trocar tabs).

## Metodos a avaliar

### 1. Traducao incremental (reparse parcial)

**Descricao**: Em vez de retraduzir o arquivo inteiro, detectar a regiao
alterada e retraduzir apenas ela. O Roslyn suporta `SyntaxTree.WithChangedText()`
para reparse incremental.

**Implementacao esperada**:
- Manter a SyntaxTree anterior em cache
- Na edicao, aplicar TextChange e obter nova SyntaxTree via reparse incremental
- Retraduzir apenas os nos afetados
- Mesclar com a traducao cacheada do restante

**Avaliar**:
- Tempo de reparse incremental vs parse completo para diferentes tamanhos
- Complexidade de detectar quais nos foram afetados (um if editado pode
  afetar o bloco inteiro)
- Como lidar com mudancas estruturais (adicionar/remover metodo)

### 2. Substituicao por texto (sem AST)

**Descricao**: Para keywords (palavras reservadas), substituir via scan
linear do codigo fonte, pulando strings e comentarios. Nao precisa de AST.

**Implementacao esperada**:
- Scanner que percorre o texto caractere por caractere
- Detectar e pular: strings (simples, verbatim, interpoladas), comentarios
  (linha, bloco), diretivas de preprocessador
- Fora dessas regioes, substituir tokens que correspondem a keywords
- Manter posicoes para nao deslocar indices

**Base existente**: PythonAdapter.ReverseSubstituteKeywords ja implementa
este metodo (scan linear, skip strings/comentarios). Pode servir como
referencia para prototipagem do C#.

**Avaliar**:
- Tempo O(n) linear vs O(n log n) do Roslyn
- Corretude: quais edge cases quebram (ex: keyword dentro de identificador
  como `publicKey`, keyword em `nameof(public)`)
- Se e possivel usar como fast path para keywords e fallback para AST
  apenas para identificadores contextuais

### 3. Cache por metodo/bloco

**Descricao**: Cachear traducoes por unidade sintatica (metodo, propriedade,
classe). Na edicao, invalidar apenas o bloco afetado.

**Implementacao esperada**:
- Indexar traducao por (hash do bloco fonte, idioma)
- Na edicao, recalcular hash apenas dos blocos que mudaram
- Blocos inalterados usam cache direto

**Avaliar**:
- Overhead de hashing vs ganho de cache
- Granularidade ideal (metodo? classe? bloco arbitrario?)
- Como lidar com mudancas que afetam multiplos blocos (renomear tipo)

### 4. Traducao lazy (viewport)

**Descricao**: Traduzir apenas as linhas visiveis no editor. Traduzir o
restante em background conforme o usuario scrolla.

**Implementacao esperada**:
- VS Code informa visible ranges via API
- Traduzir primeiro o viewport (~50 linhas)
- Registrar onDidChangeVisibleTextEditors para traduzir sob demanda
- Background worker traduz o resto em chunks

**Avaliar**:
- Viabilidade com FileSystemProvider (readFile retorna o arquivo inteiro,
  nao permite retornar parcial)
- Alternativa: retornar arquivo com placeholders e substituir via
  WorkspaceEdit conforme blocos ficam prontos
- Complexidade de manter estado parcialmente traduzido
- Flicker/jank visual durante traducao progressiva

## Criterios de avaliacao

### Performance

Cada metodo deve ser avaliado com benchmarks reais nos seguintes cenarios:

| Cenario | Metrica | Meta |
|---------|---------|------|
| Primeiro load (arquivo 1.700 linhas) | Tempo total | < 50ms |
| Primeiro load (arquivo 17.000 linhas) | Tempo total | < 500ms |
| Primeiro load (arquivo 85.000 linhas) | Tempo total | < 2s |
| Edicao de 1 linha + retraduzir | Tempo incremental | < 50ms |
| Troca de tab (arquivo ja traduzido) | Tempo de cache hit | < 5ms |
| 20 requests sequenciais (arquivos mistos) | Tempo total | < 500ms |

### UX

| Criterio | Descricao | Aceitavel | Inaceitavel |
|----------|-----------|-----------|-------------|
| Delay perceptivel | Tempo entre acao e resultado visivel | < 100ms | > 500ms |
| Flicker | Conteudo muda visivelmente apos load | Nenhum | Texto piscando |
| Consistencia | Traducao parcial visivel ao usuario | Nunca | Metade traduzido |
| Fallback | Comportamento quando otimizacao falha | Mostra original | Erro/crash |
| Scroll | Delay ao scrollar em arquivo grande | Imperceptivel | Lag visivel |

### Viabilidade

| Criterio | Peso |
|----------|------|
| Complexidade de implementacao | Alto |
| Risco de regressao nos testes existentes | Alto |
| Compatibilidade com suporte a Python (tokenizer diferente) | Medio |
| Manutencao a longo prazo | Medio |

## Entregavel

Relatorio comparativo com:
- Benchmark real de cada metodo (ou prototipo)
- Tabela comparativa nos criterios acima
- Recomendacao de qual metodo (ou combinacao) implementar
- Estimativa de esforco por metodo
- Riscos identificados

## Escopo

Esta tarefa e de **pesquisa e avaliacao**, nao de implementacao. O objetivo
e produzir informacao para decisao, nao codigo de producao.
