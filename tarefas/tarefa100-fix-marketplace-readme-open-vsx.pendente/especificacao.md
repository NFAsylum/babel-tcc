# Tarefa 100 - Corrigir overview vazio do Marketplace e publicar no Open VSX

## Fase
6 - Polimento e Deploy

## Objetivo
Corrigir a listagem vazia no VS Code Marketplace (README ausente do pacote) e adicionar publicacao automatica no Open VSX ao pipeline de release.

## Escopo
- Adicionar passo Copy README no release.yml antes do empacotamento, espelhando o passo Copy translations.
- Adicionar passo de publicacao no Open VSX (ovsx) reusando a mesma .vsix empacotada, condicional ao secret OVSX_PAT.
- Instalar o ovsx junto do vsce no pipeline.
- Bump de versao 0.9.0 -> 0.9.1 em package.json e Directory.Build.props.
- Nova secao [0.9.1] no CHANGELOG (Fixed: README ausente; Added: publicacao no Open VSX).
- Criar o secret OVSX_PAT no repositorio.
- Confirmar o claim do namespace NFAsylum no Open VSX.
- Criar a tag v0.9.1 apos o merge para disparar o release.
