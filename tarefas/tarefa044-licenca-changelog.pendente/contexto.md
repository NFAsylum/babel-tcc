# Contexto - Tarefa 044

## Dependencias
Nenhuma dependencia estrita. Pode ser iniciada a qualquer momento.

## Bloqueia
- Tarefa 031 (README linka para LICENSE)
- Tarefa 045 (deploy requer LICENSE e CHANGELOG)
- Tarefa 043 (Marketplace exibe LICENSE e CHANGELOG)

## Arquivos relevantes
- LICENSE (a ser criado na raiz)
- CHANGELOG.md (a ser criado na raiz)
- THIRD-PARTY-NOTICES.md (a ser criado se necessario)
- package.json (campo license)
- packages/core/MultiLingualCode.Core.csproj (referencia de licenca)
- packages/core/Directory.Build.props (metadata de licenca)

## Notas
- MIT e a licenca mais comum para extensoes VS Code e projetos open source educacionais.
- O CHANGELOG deve ser mantido atualizado a cada release futura.
- Verificar licencas de dependencias e importante para compliance legal.
- Ferramentas uteis: `license-checker` (npm), `dotnet-project-licenses` (.NET).
- O Marketplace exibe o CHANGELOG.md como aba separada - formatacao importa.
- Para o TCC, a escolha e justificativa da licenca pode ser mencionada na monografia.
