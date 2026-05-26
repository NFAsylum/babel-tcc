# Contexto - Tarefa 100

## Dependencias
- Tarefa 045 (deploy v1 - esta tarefa corrige a publicacao feita nela)

## Bloqueia
Nenhuma.

## Arquivos relevantes
- .github/workflows/release.yml (pipeline de release)
- packages/ide-adapters/vscode/package.json (versao)
- Directory.Build.props (versao validada pelo pipeline)
- packages/ide-adapters/vscode/CHANGELOG.md
- scripts/copy-readme.js (copia o README para a extensao no empacotamento)

## Notas
- O overview do Marketplace ficou vazio porque o CI roda o binario vsce package direto, que nao dispara o hook prepackage do npm; o copy-readme.js nao rodava e o README ficava fora do .vsix.
- O README da pasta da extensao e gitignored, entao nao existe no checkout limpo do CI.
- O Marketplace nao permite republicar a mesma versao; por isso o bump para 0.9.1.
- O Open VSX foi publicado manualmente no 0.9.0; o pipeline passa a publicar automaticamente, condicional ao secret OVSX_PAT.
- Implementado no PR #141.
