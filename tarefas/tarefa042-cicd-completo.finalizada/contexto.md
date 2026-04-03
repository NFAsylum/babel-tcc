# Contexto - Tarefa 042

## Dependencias
- Tarefa 041 (codebase limpa para pipeline final)
- Tarefa 003 (CI/CD basico como ponto de partida)
- Tarefa 036 (cobertura de testes para integrar no CI)

## Bloqueia
- Tarefa 045 (deploy usa o pipeline de release)

## Arquivos relevantes
- .github/workflows/ (pipelines existentes e novos)
- package.json (versao, scripts de build e test)
- packages/core/MultiLingualCode.Core.csproj (versao .NET)
- packages/core/Directory.Build.props (versao centralizada)
- packages/ide-adapters/vscode/src/ (codigo da extensao a ser testado)
- CHANGELOG.md (referencia para release notes)
- babel-tcc-translations/scripts/validate.py (validacao de traducoes)

## CI existente (tarefa 003)
Ja existem 3 workflows parciais em .github/workflows/ que devem ser
expandidos ou consolidados (nao recriar do zero):
- `test.yml`: roda em PR para main. Dois jobs:
  - core-tests: dotnet restore/build/test (ubuntu-only, sem matrix)
  - vscode-build: npm install/build/lint (sem `npm test` — placeholder)
- `build-core.yml`: roda em push+PR para main, filtrado por paths packages/core/**.
  dotnet restore/build/test em Release config (ubuntu-only)
- `build-vscode.yml`: roda em push+PR para main, filtrado por paths packages/ide-adapters/vscode/**.
  npm install/build/lint (sem `npm test`, com cache de node_modules)

Decisao necessaria: consolidar em 1-2 workflows ou expandir os existentes.
Nenhum workflow tem matrix strategy, cobertura, ou release pipeline.

## Estado atual dos testes
- Core C#: 388 testes em 23 arquivos, estrutura modular espelhando o codigo
  - Lacunas menores: RoslynWrapper e JsonFileReader sem teste direto
- Extension TypeScript: ZERO testes, `npm test` e um echo placeholder
  - 10 arquivos .ts sem nenhuma cobertura de testes
  - KeywordMapService, LanguageDetector e ConfigurationService sao testáveis
    sem mock de vscode API (logica pura)
- babel-tcc-translations: validate.py roda no CI do repo de traducoes,
  mas nao e executado no CI do babel-tcc principal

## Notas
- O VSCE_PAT (Personal Access Token) deve ser criado no Azure DevOps e adicionado como secret no GitHub.
- Testar o pipeline de release com uma tag de pre-release (v0.9.0-beta) antes da v1.0.0.
- Considerar usar GitHub Environments para separar staging de production.
- Release notes automaticas do GitHub sao uma opcao simples e eficaz.
- O .vsix gerado deve ser testado manualmente antes de publicar no Marketplace.
- Manter o pipeline rapido: cache de dependencies (.NET, npm) reduz tempo significativamente.
- A estrutura de testes TS deve seguir a mesma convencao modular do Core C#
  (test/providers/, test/services/ espelhando src/providers/, src/services/).
