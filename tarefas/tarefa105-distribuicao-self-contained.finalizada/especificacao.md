# Tarefa 105 - Distribuicao self-contained por plataforma (dual-track)

## Fase
6 - Polimento e Deploy

## Objetivo
Reduzir a barreira de instalacao da extensao embutindo o runtime .NET em pacotes por plataforma,
mantendo um pacote universal de fallback. Implementa a decisao DT-010.

## Atencao: nao e so config

A parte de pipeline (matrix + `--self-contained`) e ~20% do trabalho. O grosso e mudanca de
codigo no spawn do Core e validacao por plataforma (ver itens 1 e 6 abaixo). O risco mora no
launch: se errar, o pacote self-contained quebra para o usuario e o CI dificilmente pega sem
smoke test por SO.

## Escopo

### 1. Spawn do Core no CoreBridge (mudanca de codigo, parte critica)
Hoje `coreBridge.ts` faz `spawn('dotnet', [..., 'MultiLingualCode.Core.Host.dll'])` (linhas ~57/88),
ou seja, invoca o runtime `dotnet` do sistema para rodar a `.dll`. Um build self-contained NAO e
uma `.dll` rodada via `dotnet` — e um **executavel nativo** (`MultiLingualCode.Core.Host` no
Linux/Mac, `MultiLingualCode.Core.Host.exe` no Windows), rodado diretamente.

Como shippamos os dois formatos, o CoreBridge precisa:
- Detectar o modo: se existe o executavel nativo em `bin/`, lanca-lo direto; senao, cair para
  `spawn('dotnet', ['...Host.dll'])` (universal).
- Resolver o nome do executavel por SO (`Host` vs `Host.exe`) e dar permissao de execucao quando
  necessario (Linux/Mac).
- Manter testes do CoreBridge cobrindo os dois caminhos.

### 2. Mapeamento RID (.NET) <-> target (vsce)
Os identificadores divergem e precisam de uma tabela:
- vsce `win32-x64`   <-> dotnet `win-x64`
- vsce `win32-arm64` <-> dotnet `win-arm64`
- vsce `linux-x64`   <-> dotnet `linux-x64`
- vsce `linux-arm64` <-> dotnet `linux-arm64`
- vsce `alpine-x64`  <-> dotnet `linux-musl-x64` (opcional)
- vsce `darwin-x64`  <-> dotnet `osx-x64`
- vsce `darwin-arm64`<-> dotnet `osx-arm64`

### 3. publish-core parametrizado
Adicionar variante self-contained: `dotnet publish -r <dotnet-rid> --self-contained true`
(parametrizar o RID; hoje o script faz um publish unico framework-dependent).

### 4. release.yml com matrix
- Matrix self-contained: win32-x64, win32-arm64, linux-x64, linux-arm64, darwin-x64, darwin-arm64.
- Mais um pacote universal (sem `--target`, framework-dependent) como fallback.
- Cada alvo: publish-core do RID -> empacotar com `vsce package --target <vsce-target>`.
- Publicar cada alvo no Marketplace (`vsce publish --target`) e no Open VSX (`ovsx publish`).
- Idealmente integrar com a robustez da tarefa 106 (retry, destinos desacoplados).

### 5. Dois payloads e .vscodeignore
- Universal: DLLs framework-dependent em `bin/`.
- Por plataforma: apenas o executavel nativo daquele alvo.
- Garantir que cada `.vsix` carregue so o payload correto e que o `.vscodeignore` nao vaze
  binarios de outras plataformas (inflaria o pacote e confundiria o launch).

### 6. Validacao por plataforma
- Smoke test em maquina/container **sem .NET instalado**: o executavel nativo sobe e traduz.
- Confirmar que a resolucao do Python continua funcionando a partir do binario self-contained
  (so para `.py`).
- Suite de testes continua passando.

### 7. README (pt-br, en, es)
- Explicar que o pacote por plataforma nao exige .NET; o universal exige .NET 8.
- Deixar claro que Python e opcional, necessario apenas para arquivos `.py`.

## Fora de escopo
- NativeAOT (Roslyn nao e AOT-friendly) e WASM (rejeitado em DT-002).
