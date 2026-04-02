# Benchmark: Spawn-por-request vs Processo Persistente

Data: 2026-04-02
Maquina: Windows 10, x86_64, .NET 9.0.302, Release build
Metodologia: 5 rodadas x 20 requests, medias calculadas
Reproduzir: `bash scripts/benchmark_persistent.sh [dll_spawn] [dll_persistent]`

## Cold start (1 request, media de 5 rodadas)

| Modo | Tempo |
|---|---|
| Spawn-por-request | 136ms |
| Processo persistente | 147ms |
| Diferenca | +11ms |

O modo persistente e ~11ms mais lento no primeiro request porque carrega
tabelas de traducao no startup (--translations). No modo spawn, as tabelas
sao carregadas sob demanda dentro do request. Essa diferenca e paga uma
unica vez ao ativar a extensao.

## 20 requests sequenciais por tamanho de arquivo (media de 5 rodadas)

| Tamanho | Spawn (ms/req) | Persistente (ms/req) | Speedup | Traducao pura (ms/req) |
|---|---|---|---|---|
| ~10 linhas (5 metodos) | 126 | 8 | **14.3x** | 1 |
| ~425 linhas (25 metodos) | 131 | 12 | **10.2x** | 4 |
| ~1700 linhas (100 metodos) | 160 | 25 | **6.2x** | 16 |
| ~3400 linhas (200 metodos) | 204 | 57 | **3.5x** | 39 |

## Decomposicao do tempo

Para um arquivo de ~10 linhas:
- **Spawn**: 126ms = ~120ms cold start .NET + ~1ms traducao + ~5ms overhead
- **Persistente**: 8ms = ~1ms traducao + ~7ms IPC (stdin/stdout/JSON parse)

Para um arquivo de ~3400 linhas:
- **Spawn**: 204ms = ~120ms cold start .NET + ~39ms traducao + ~45ms overhead
- **Persistente**: 57ms = ~39ms traducao + ~18ms IPC

O speedup diminui com arquivos maiores porque o tempo de traducao (Roslyn
parse + substituicao) passa a dominar em relacao ao cold start eliminado.
Em arquivos pequenos (~10 linhas), o cold start era 99% do tempo.

## Overhead de memoria

O processo persistente fica residente (~40MB RAM) enquanto a extensao
esta ativa. No modo spawn, ~40MB sao alocados e liberados a cada request.

## Conclusao

O processo persistente elimina o cold start do .NET (~120ms). O ganho
e maior em arquivos pequenos (14x) e diminui em arquivos grandes (3.5x)
onde a traducao domina. Na pratica, a maioria dos arquivos tem <500 linhas,
resultando em speedup de 10-14x.
