# Relatorio: Pesquisa de Otimizacoes de Performance (Tarefa 061)

Data: 2026-04-06
Maquina: Windows 10, .NET 9.0.302, Release build
Metodologia: 4 runs por cenario, medias calculadas
Codigo: OptimizationResearchTests.cs

## Baseline: Comportamento Atual (Full Translation via Roslyn)

| Metodos | ~Linhas | Run1 | Run2 | Run3 | Run4 | Media |
|---------|---------|------|------|------|------|-------|
| 5 | 95 | 1ms | 0ms | 0ms | 0ms | 0ms |
| 25 | 435 | 6ms | 6ms | 5ms | 10ms | 6ms |
| 100 | 1710 | 38ms | 24ms | 18ms | 17ms | 24ms |
| 200 | 3410 | 92ms | 79ms | 70ms | 61ms | 75ms |
| 500 | 8510 | 506ms | 302ms | 588ms | 481ms | 469ms |
| 1000 | 17010 | 2523ms | 1802ms | 2029ms | 2033ms | 2096ms |

O crescimento e super-linear (~quadratico). Arquivos <1000 linhas sao aceitaveis.
Arquivos >5000 linhas apresentam delay perceptivel.

## Metodo 1: Incremental Reparse (Roslyn WithChangedText)

| Metodos | ~Linhas | Parse Completo | Incremental | Speedup |
|---------|---------|----------------|-------------|---------|
| 25 | 435 | 0ms | 2ms | N/A |
| 100 | 1710 | 0ms | 0ms | N/A |
| 500 | 8510 | 1ms | 1ms | 1.0x |
| 1000 | 17010 | 3ms | 3ms | 1.0x |

### Conclusao
O parse do Roslyn (CSharpSyntaxTree.ParseText) e extremamente rapido — 3ms
para 17k linhas. O gargalo NAO esta no parse. Esta no walk da AST e na
substituicao de keywords pelo TranslationOrchestrator. O incremental reparse
nao traz ganho porque o bottleneck e downstream do parse.

### Viabilidade
- Complexidade: MEDIA (precisa manter SyntaxTree em cache, detectar mudancas)
- Ganho: ZERO para o gargalo real
- Risco: BAIXO
- Recomendacao: NAO implementar (resolve o problema errado)

## Metodo 2: Substituicao por Texto (Scan Linear)

| Metodos | ~Linhas | Roslyn AST | Text Scan | Speedup |
|---------|---------|------------|-----------|---------|
| 25 | 435 | 2ms | 0ms | 2x |
| 100 | 1710 | 9ms | 0ms | 9x |
| 500 | 8510 | 497ms | 3ms | 166x |
| 1000 | 17010 | 2672ms | 1ms | 2672x |

### Edge Cases (todos PASS)
- keyword in identifier (publicKey): PASS (nao substitui parcialmente)
- keyword in string: PASS (strings puladas)
- keyword in comment: PASS (comentarios pulados)
- var standalone: PASS (traduzido corretamente)
- var in identifier (variable): PASS (nao substitui parcialmente)

### Conclusao
166-2672x mais rapido que Roslyn para arquivos grandes. O scan linear e O(n)
vs O(n^2) do walk da AST. Para 17k linhas: 1ms vs 2672ms.

### Edge Cases: 43/43 PASS (0 FAIL)

Basicos (5): keyword em identifier, string, comment, var standalone, var em identifier
Avancados (14): verbatim string, interpolated string, block comment,
keyword apos block comment, preprocessor directive, generic type,
multiplas keywords, string vazia, escaped quote, keyword inicio/fim
de arquivo, adjacente com braces, tab separado, unicode identifier
String/multiline/pragma/broken (24): multiline block comment, unclosed
block comment, raw string literal, string.Format, interpolated com expressao,
interpolated com braces aninhados, verbatim interpolated, multiline comment
spans keywords, pragma warning/restore, #if/#region/#define, unclosed
string/char, unclosed block comment EOF, missing semicolons, double braces,
empty input, whitespace, keyword sozinha, keyword com numeros, escaped backslash

Preprocessor directives (`#if`, `#region`, `#define`, `#pragma`): linhas que
comecam com `#` sao puladas inteiramente pelo scanner.

### Limitacoes confirmadas
- C# 11 raw string literals (`"""..."""`): keywords dentro podem ser traduzidas
- Nao consegue traduzir identificadores contextuais (tradu annotations)
- Nao funciona para features que dependem da AST (posicoes de nos, tipos)

### Viabilidade
- Complexidade: BAIXA (scanner ja existe no PythonAdapter.ReverseSubstituteKeywords)
- Ganho: MUITO ALTO (166-2672x para arquivos grandes)
- Risco: BAIXO (42/43 edge cases passam, 1 fix trivial para directives)
- Recomendacao: IMPLEMENTAR como fast path para keywords, fallback para AST
  quando precisar de identificadores/anotacoes tradu

## Metodo 3: Cache por Bloco (Hash)

| Metodos | ~Linhas | Full (1a vez) | Full (2a vez) | Cache Hit | Speedup |
|---------|---------|---------------|---------------|-----------|---------|
| 25 | 435 | 4ms | 2ms | 0ms | 2x |
| 100 | 1710 | 10ms | 14ms | 0ms | 14x |
| 500 | 8510 | 476ms | 540ms | 2ms | 270x |

### Conclusao
Cache hit e quase instantaneo (~0ms). O custo e apenas hash lookup. Para
arquivos com metodos que nao mudam entre edicoes, o ganho e enorme.

### Limitacoes
- Primeiro load continua lento (cache vazio)
- Precisa de block splitting que adiciona complexidade
- Mudancas em tipos compartilhados invalidam multiplos blocos
- Hash collision (teoricamente possivel, praticamente improvavel)

### Viabilidade
- Complexidade: MEDIA (block splitting, hash, invalidacao)
- Ganho: ALTO para interacoes subsequentes (270x para 8500 linhas)
- Risco: BAIXO (fallback para full translation se cache miss)
- Recomendacao: IMPLEMENTAR como segunda otimizacao (apos text scan)

## Metodo 4: Traducao Lazy (Viewport)

### Avaliacao Teorica (sem benchmark — requer VS Code rodando)

O FileSystemProvider.readFile() retorna o arquivo inteiro — nao permite
retorno parcial. Opcoes:
- Retornar placeholders e substituir via WorkspaceEdit: causa flicker
- Retornar traducao parcial: texto "pula" conforme scroll
- Background worker com chunks: delay entre scroll e traducao

### Viabilidade
- Complexidade: MUITO ALTA (estado parcial, concorrencia, flicker)
- Ganho: ALTO para primeiro load de arquivos enormes
- Risco: MUITO ALTO (UX ruim com flicker/jank)
- Recomendacao: NAO implementar (complexidade nao justifica o ganho,
  e o text scan resolve o primeiro load de forma mais simples)

## Tabela Comparativa

| Criterio | Metodo 1 | Metodo 2 | Metodo 3 | Metodo 4 |
|----------|----------|----------|----------|----------|
| Speedup (8500 linhas) | 1x | 166x | 270x | Teorico |
| Speedup (17000 linhas) | 1x | 2672x | N/A | Teorico |
| Primeiro load | Sem ganho | Muito rapido | Sem ganho | Rapido |
| Interacoes seguintes | Sem ganho | Rapido | Muito rapido | Rapido |
| Complexidade | Media | Baixa | Media | Muito alta |
| Risco de regressao | Baixo | Medio | Baixo | Muito alto |
| Compatibilidade Python | Sim | Sim (ja existe) | Sim | Sim |
| UX (flicker) | Nenhum | Nenhum | Nenhum | Provavel |

## Sistema Hibrido: Text Scan + Roslyn Fallback (4 runs cada)

Logica de decisao:
- Arquivo contem `tradu` ou `"""` → Roslyn (AST completo)
- Caso contrario → Text Scan (fast path)

### Cenario A: Arquivos SEM tradu (Text Scan path)

| Metodos | ~Linhas | Roslyn | Hibrido | Speedup |
|---------|---------|--------|---------|---------|
| 25 | 435 | 6ms | 0ms | 6x |
| 100 | 1710 | 25ms | 0ms | 25x |
| 500 | 8510 | 539ms | 1ms | 539x |
| 1000 | 17010 | 2069ms | 2ms | **1034x** |

### Cenario B: Arquivos COM tradu (Roslyn fallback)

| Metodos | ~Linhas | Roslyn | Hibrido | Speedup |
|---------|---------|--------|---------|---------|
| 25 | 259 | 5ms | 4ms | 1.2x |
| 100 | 1009 | 39ms | 40ms | 1.0x |
| 500 | 5009 | 862ms | 870ms | 1.0x |
| 1000 | 10009 | 3459ms | 3428ms | 1.0x |

### Cenario C: Arquivos COM raw strings (Roslyn fallback)

| Metodos | ~Linhas | Roslyn | Hibrido | Speedup |
|---------|---------|--------|---------|---------|
| 25 | 234 | 0ms | 0ms | N/A |
| 100 | 909 | 5ms | 1ms | 5x |

### Analise

O sistema hibrido tem **zero overhead** para arquivos com tradu/raw strings —
o custo e apenas um `string.Contains` que e O(n) mas negligivel.

Para arquivos sem tradu (maioria dos arquivos de codigo), o speedup e
**539-1034x**. 17k linhas: 2069ms → 2ms.

Para arquivos com tradu, o desempenho e identico ao Roslyn (1.0x).

## Metas de Performance vs Resultados

| Cenario | Meta | Atual | Hibrido (sem tradu) | Hibrido (com tradu) |
|---------|------|-------|---------------------|---------------------|
| Load 1700 linhas | <50ms | 25ms | 0ms | 40ms |
| Load 17000 linhas | <500ms | 2305ms | 2ms | 3428ms |
| Edicao + retraduzir | <50ms | 25-2305ms | 0-2ms | 40-3428ms |
| Troca de tab | <5ms | 25-2305ms | 0-2ms | 40-3428ms |

Arquivos sem tradu: TODAS as metas atingidas.
Arquivos com tradu: sem melhoria (Roslyn e o unico caminho).

## Recomendacao

### Implementar:

1. **Sistema Hibrido (Text Scan + Roslyn Fallback)** — maior ganho,
   menor complexidade. Deteccao por `string.Contains("tradu")` e
   `string.Contains("\"\"\"")`. 43/43 edge cases testados.
   Scanner ja existe como referencia (PythonAdapter.ReverseSubstituteKeywords).

### NAO implementar:

2. **Metodo 1 (Incremental Reparse)** — resolve o problema errado.
3. **Metodo 3 (Cache por Bloco)** — desnecessario se Text Scan ja faz em 2ms.
4. **Metodo 4 (Lazy Viewport)** — complexidade muito alta.
