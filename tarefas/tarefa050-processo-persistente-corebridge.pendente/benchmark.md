# Benchmark: Spawn-por-request vs Processo Persistente

Data: 2026-04-02
Maquina: Windows 10, .NET 8, Release build
Codigo de teste: arquivo C# com 1 classe, 1 metodo, if/for/Console.WriteLine (~10 linhas)
Metodologia: 5 rodadas de 20 requests cada, medias calculadas

## Resultados: 20 requests sequenciais (5 rodadas)

### Spawn-por-request (main)

| Rodada | Total | Media/request |
|--------|-------|---------------|
| 1 | 2550ms | 127ms |
| 2 | 2522ms | 126ms |
| 3 | 2525ms | 126ms |
| 4 | 2525ms | 126ms |
| 5 | 2524ms | 126ms |
| **Media** | **2529ms** | **126ms** |

### Processo persistente (PR #58)

| Rodada | Total | Media/request |
|--------|-------|---------------|
| 1 | 165ms | 8ms |
| 2 | 166ms | 8ms |
| 3 | 163ms | 8ms |
| 4 | 165ms | 8ms |
| 5 | 164ms | 8ms |
| **Media** | **165ms** | **8ms** |

### Comparacao

| Metrica | Spawn | Persistente | Speedup |
|---------|-------|-------------|---------|
| Media 20 requests | 2529ms | 165ms | **15.3x** |
| Media por request | 126ms | 8ms | **15.8x** |
| Desvio padrao total | ~12ms | ~1ms | — |

## Cold start (1 request)

| Modo | Tempo |
|---|---|
| Spawn-por-request | 140ms |
| Processo persistente | 152ms |

Ambos incluem carregamento do runtime .NET. Custo similar — a diferenca
aparece apenas nos requests subsequentes.

## Analise

- Resultados extremamente consistentes: desvio padrao < 1% em ambos os modos
- O processo persistente elimina o cold start de ~126ms repetido a cada request
- Na pratica: o usuario paga ~150ms no primeiro request (startup da extensao),
  depois cada traducao leva ~8ms — imperceptivel
- 20 tabs abertas: spawn leva 2.5s (perceptivel), persistente leva 165ms (instantaneo)

## Overhead de memoria

O processo persistente fica residente (~40MB RAM) enquanto a extensao esta ativa.
No modo spawn, o .NET e carregado e descarregado a cada request (~40MB alocados
e liberados repetidamente, causando pressao no GC do sistema).
