# Contexto - Tarefa 105

## Dependencias
- Tarefa 100 (pipeline de release com Marketplace + Open VSX ja consolidado)

## Bloqueia
Nenhuma estrita. Reforca o objetivo da Tarefa 045 (deploy) ao reduzir a barreira de instalacao.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/services/coreBridge.ts (spawn do Core — MUDANCA CRITICA)
- .github/workflows/release.yml (passa a usar matrix de plataformas)
- packages/ide-adapters/vscode/package.json (scripts publish-core / package)
- packages/ide-adapters/vscode/.vscodeignore
- docs/decisoes-tecnicas.md (DT-010 registra a decisao)
- README.md / README.en.md / README.es.md (comunicar dependencias)

## Notas
- Decisao registrada em DT-010: distribuicao dual-track (self-contained por plataforma + universal).
- Numeros do spike: framework-dependent ~5 MB comprimido; self-contained linux-x64 ~36 MB comprimido.
- RIDs comuns: win32-x64, win32-arm64, linux-x64, linux-arm64, darwin-x64, darwin-arm64.
- O VS Code/Marketplace/Open VSX escolhem o pacote por plataforma e caem no universal quando nao ha alvo.
- Python NAO precisa de mudanca de codigo: resolucao ja e lazy (so para `.py`) e degrada com OperationResult.Fail. Falta so o texto no README.
- AOT e WASM estao fora (Roslyn nao e AOT-friendly; WASM ja rejeitado em DT-002).
- PONTO CRITICO: self-contained gera executavel nativo, nao `.dll`. O CoreBridge hoje faz
  `spawn('dotnet', ['...Host.dll'])`; precisa detectar e lancar o exe nativo direto (por SO),
  com fallback para o universal. Essa e a maior parte do esforco, nao o YAML.
