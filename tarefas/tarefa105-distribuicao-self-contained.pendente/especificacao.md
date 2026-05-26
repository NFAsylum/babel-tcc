# Tarefa 105 - Distribuicao self-contained por plataforma (dual-track)

## Fase
6 - Polimento e Deploy

## Objetivo
Reduzir a barreira de instalacao da extensao embutindo o runtime .NET em pacotes por plataforma,
mantendo um pacote universal de fallback. Implementa a decisao DT-010.

## Escopo
- Adicionar variante self-contained ao publish do core (`dotnet publish -r <rid> --self-contained true`).
- Atualizar o release.yml para uma matrix de alvos:
  - self-contained: win32-x64, win32-arm64, linux-x64, linux-arm64, darwin-x64, darwin-arm64
  - universal (framework-dependent, sem --target) como fallback
- Empacotar cada alvo com `vsce package --target <rid>` e o universal sem --target.
- Publicar todos os alvos no Marketplace (vsce) e no Open VSX (ovsx).
- Garantir que o .vscodeignore inclui o binario self-contained correto por alvo.
- Atualizar README.md / README.en.md / README.es.md:
  - explicar que o pacote por plataforma nao exige .NET; o universal exige .NET 8
  - deixar claro que Python e opcional, necessario apenas para arquivos `.py`
- Validar a instalacao de um .vsix self-contained em maquina sem .NET instalado.
