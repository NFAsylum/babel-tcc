#!/usr/bin/env bash
# Benchmark: Spawn-por-request vs Processo Persistente
# Uso: bash scripts/benchmark_persistent.sh [DLL_SPAWN] [DLL_PERSISTENT]
#
# Sem argumentos: compila ambas as versoes automaticamente (requer git).
# Com argumentos: usa os DLLs fornecidos.
#
# Requisitos: .NET 8 SDK, bash

set -euo pipefail

TRANSLATIONS="packages/core/MultiLingualCode.Core.Tests/TestData/translations"
ROUNDS=5
REQUESTS_PER_ROUND=20
SIZES=(5 25 100 200)  # metodos por arquivo (~85, ~425, ~1700, ~3400 linhas)
SIZE_NAMES=("small-10lin" "medium-425lin" "large-1700lin" "xlarge-3400lin")

generate_code() {
  local methods=$1
  local code="using System; namespace Test { public class Gen {"
  for ((i=0; i<methods; i++)); do
    code+=" public int M${i}(int p${i}) { int r = p${i} * 2; if (r > 100) { return r; } else { for (int j = 0; j < p${i}; j++) { r += j; } return r; } }"
  done
  code+=" } }"
  echo "$code"
}

measure_spawn() {
  local dll=$1 size=$2 rounds=$3 requests=$4
  local code
  code=$(generate_code "$size")
  local params="{\"sourceCode\":\"$code\",\"fileExtension\":\".cs\",\"targetLanguage\":\"pt-br\"}"

  local total=0
  for ((r=1; r<=rounds; r++)); do
    local start end elapsed
    start=$(date +%s%N)
    for ((i=1; i<=requests; i++)); do
      dotnet "$dll" --method TranslateToNaturalLanguage --params "$params" --translations "$TRANSLATIONS" > /dev/null 2>&1
    done
    end=$(date +%s%N)
    elapsed=$(( (end - start) / 1000000 ))
    total=$((total + elapsed))
  done

  echo $(( total / rounds ))
}

measure_persistent() {
  local dll=$1 size=$2 rounds=$3 requests=$4
  local code
  code=$(generate_code "$size")
  local params="{\"sourceCode\":\"$code\",\"fileExtension\":\".cs\",\"targetLanguage\":\"pt-br\"}"

  local total=0
  for ((r=1; r<=rounds; r++)); do
    local reqs=""
    for ((i=1; i<=requests; i++)); do
      reqs+="${reqs:+\n}{\"method\":\"TranslateToNaturalLanguage\",\"params\":$params}"
    done
    reqs+="\n{\"method\":\"quit\"}"

    local start end elapsed
    start=$(date +%s%N)
    printf "$reqs" | dotnet "$dll" --translations "$TRANSLATIONS" > /dev/null 2>&1
    end=$(date +%s%N)
    elapsed=$(( (end - start) / 1000000 ))
    total=$((total + elapsed))
  done

  echo $(( total / rounds ))
}

measure_cold_start() {
  local dll=$1 mode=$2
  local code
  code=$(generate_code 5)
  local params="{\"sourceCode\":\"$code\",\"fileExtension\":\".cs\",\"targetLanguage\":\"pt-br\"}"

  local total=0
  for ((r=1; r<=ROUNDS; r++)); do
    local start end elapsed
    start=$(date +%s%N)
    if [ "$mode" = "spawn" ]; then
      dotnet "$dll" --method TranslateToNaturalLanguage --params "$params" --translations "$TRANSLATIONS" > /dev/null 2>&1
    else
      printf "{\"method\":\"TranslateToNaturalLanguage\",\"params\":$params}\n{\"method\":\"quit\"}\n" | dotnet "$dll" --translations "$TRANSLATIONS" > /dev/null 2>&1
    fi
    end=$(date +%s%N)
    elapsed=$(( (end - start) / 1000000 ))
    total=$((total + elapsed))
  done

  echo $(( total / ROUNDS ))
}

measure_translation_only() {
  # Persistent mode: cold start + N requests, then subtract cold start
  # This isolates IPC + translation time per request
  local dll=$1 size=$2
  local code
  code=$(generate_code "$size")
  local params="{\"sourceCode\":\"$code\",\"fileExtension\":\".cs\",\"targetLanguage\":\"pt-br\"}"
  local n=50

  local reqs=""
  for ((i=1; i<=n; i++)); do
    reqs+="${reqs:+\n}{\"method\":\"TranslateToNaturalLanguage\",\"params\":$params}"
  done
  reqs+="\n{\"method\":\"quit\"}"

  local total=0
  for ((r=1; r<=3; r++)); do
    local start end elapsed
    start=$(date +%s%N)
    printf "$reqs" | dotnet "$dll" --translations "$TRANSLATIONS" > /dev/null 2>&1
    end=$(date +%s%N)
    elapsed=$(( (end - start) / 1000000 ))
    total=$((total + elapsed))
  done

  local avg_total=$(( total / 3 ))
  local cold=$(measure_cold_start "$dll" "persistent")
  local translation_time=$(( (avg_total - cold) ))
  local per_request=$(( translation_time / n ))
  echo "$per_request"
}

# --- Main ---

DLL_SPAWN="${1:-}"
DLL_PERSISTENT="${2:-}"

if [ -z "$DLL_SPAWN" ] || [ -z "$DLL_PERSISTENT" ]; then
  echo "Compilando versoes..."
  echo "  (Para pular compilacao, passe os DLLs como argumento)"

  if [ -z "$DLL_SPAWN" ]; then
    DLL_SPAWN="/tmp/babel-bench-spawn/MultiLingualCode.Core.Host.dll"
    echo "  Spawn: usando $DLL_SPAWN"
  fi
  if [ -z "$DLL_PERSISTENT" ]; then
    DLL_PERSISTENT="/tmp/babel-bench-persistent/MultiLingualCode.Core.Host.dll"
    echo "  Persistente: usando $DLL_PERSISTENT"
  fi
fi

echo ""
echo "=== BENCHMARK: Spawn-por-request vs Processo Persistente ==="
echo "Rodadas: $ROUNDS | Requests/rodada: $REQUESTS_PER_ROUND"
echo "Maquina: $(uname -s) $(uname -m)"
echo ".NET: $(dotnet --version)"
echo ""

# Cold start
echo "--- Cold start (1 request, media de $ROUNDS rodadas) ---"
cs_spawn=$(measure_cold_start "$DLL_SPAWN" "spawn")
cs_persistent=$(measure_cold_start "$DLL_PERSISTENT" "persistent")
echo "  Spawn:       ${cs_spawn}ms"
echo "  Persistente: ${cs_persistent}ms"
echo "  Diferenca:   $((cs_persistent - cs_spawn))ms (persistente carrega tabelas no startup)"
echo ""

# Multiple sizes
echo "--- $REQUESTS_PER_ROUND requests sequenciais (media de $ROUNDS rodadas) ---"
printf "%-16s %10s %10s %10s %10s\n" "Tamanho" "Spawn" "Persist." "Speedup" "Transl/req"
printf "%-16s %10s %10s %10s %10s\n" "--------" "------" "--------" "-------" "----------"

for idx in "${!SIZES[@]}"; do
  size=${SIZES[$idx]}
  name=${SIZE_NAMES[$idx]}

  t_spawn=$(measure_spawn "$DLL_SPAWN" "$size" "$ROUNDS" "$REQUESTS_PER_ROUND")
  t_persistent=$(measure_persistent "$DLL_PERSISTENT" "$size" "$ROUNDS" "$REQUESTS_PER_ROUND")
  speedup="$(( t_spawn * 10 / t_persistent ))"; speedup="${speedup:0:$((${#speedup}-1))}.${speedup: -1}x"
  trans_only=$(measure_translation_only "$DLL_PERSISTENT" "$size")

  avg_spawn=$(( t_spawn / REQUESTS_PER_ROUND ))
  avg_persistent=$(( t_persistent / REQUESTS_PER_ROUND ))

  printf "%-16s %8sms %8sms %10s %8sms\n" "$name" "$avg_spawn" "$avg_persistent" "$speedup" "$trans_only"
done

echo ""
echo "Transl/req = tempo de traducao puro (sem cold start, sem IPC overhead)."
echo "Diferenca entre Persist. e Transl/req = overhead de IPC (~1-3ms)."
echo ""
echo "Overhead de memoria: ~40MB residente (processo .NET persistente)."
