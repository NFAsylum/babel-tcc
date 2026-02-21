# Contexto - Tarefa 042

## Dependencias
- Tarefa 041 (codebase limpa para pipeline final)
- Tarefa 003 (CI/CD basico como ponto de partida)
- Tarefa 036 (cobertura de testes para integrar no CI)

## Bloqueia
- Tarefa 045 (deploy usa o pipeline de release)

## Arquivos relevantes
- .github/workflows/ (pipelines existentes e novos)
- package.json (versao, scripts de build)
- packages/core/MultiLingualCode.Core.csproj (versao .NET)
- packages/core/Directory.Build.props (versao centralizada)
- CHANGELOG.md (referencia para release notes)

## Notas
- O VSCE_PAT (Personal Access Token) deve ser criado no Azure DevOps e adicionado como secret no GitHub.
- Testar o pipeline de release com uma tag de pre-release (v0.9.0-beta) antes da v1.0.0.
- Considerar usar GitHub Environments para separar staging de production.
- Release notes automaticas do GitHub sao uma opcao simples e eficaz.
- O .vsix gerado deve ser testado manualmente antes de publicar no Marketplace.
- Manter o pipeline rapido: cache de dependencies (.NET, npm) reduz tempo significativamente.
