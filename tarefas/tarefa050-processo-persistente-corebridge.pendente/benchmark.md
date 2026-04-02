# Benchmark: Spawn-por-request vs Processo Persistente

Data: 2026-04-02
Maquina: Windows 10, .NET 8, Release build
Codigo de teste: arquivo C# com 1 classe, 1 metodo, if/for/Console.WriteLine (~10 linhas)

## Resultados

### 20 requests sequenciais (mesmo codigo traduzido 20x)

| Modo | Total | Media/request | Speedup |
|---|---|---|---|
| Spawn-por-request (main) | 2551ms | 127ms | 1x |
| Processo persistente (PR #58) | 168ms | 8ms | **15x** |

### 1 request (cold start incluso)

| Modo | Tempo |
|---|---|
| Spawn-por-request | 140ms |
| Processo persistente | 152ms |

## Analise

- **Cold start**: ambos os modos tem custo similar (~140-150ms) para o primeiro
  request, pois o runtime .NET precisa ser carregado em ambos os casos.

- **Requests subsequentes**: o processo persistente elimina completamente o
  cold start de ~127ms por request, reduzindo para ~8ms (15x mais rapido).

- **Na pratica**: o usuario paga o cold start 1 vez ao ativar a extensao.
  Depois disso, cada troca de tab traduz em ~8ms — imperceptivel.

- **20 requests**: spawn mode leva 2.5s (usuario percebe delay).
  Persistente leva 168ms (instantaneo).

## Overhead de memoria

O processo persistente fica residente (~40MB RAM) enquanto a extensao esta ativa.
No modo spawn, o .NET e carregado e descarregado a cada request (~40MB alocados
e liberados repetidamente, causando pressao no GC do sistema).
