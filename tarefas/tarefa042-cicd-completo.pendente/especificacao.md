# Tarefa 042 - CI/CD completo

## Fase
6 - Polimento e Deploy

## Objetivo
Configurar pipeline CI/CD completo com build, testes, geracao de artefatos e publicacao automatica via tags.

## Escopo
- GitHub Actions - CI (em push e PR):
  - Build do Core C# (`dotnet build`)
  - Build da Extension TypeScript (`npm run compile`)
  - Rodar testes unitarios do Core (`dotnet test`)
  - Rodar testes unitarios da Extension (`npm test`)
  - Rodar testes de integracao
  - Gerar relatorio de cobertura
  - Lint check (dotnet format --verify-no-changes, eslint)
  - Matrix strategy: Windows + Ubuntu (minimo)
- GitHub Actions - Release (em tag v*):
  - Build otimizado (Release configuration)
  - Rodar todos os testes
  - Gerar pacote .vsix (`vsce package`)
  - Publicar no VS Code Marketplace (`vsce publish`)
  - Criar GitHub Release com .vsix como asset
  - Gerar release notes automaticas (changelog ou git log)
- Versionamento semantico:
  - Usar SemVer (MAJOR.MINOR.PATCH)
  - Versao definida em package.json e sincronizada com .csproj
  - Tag git como trigger de release (v1.0.0, v1.0.1, etc.)
- Release notes automaticas:
  - Gerar a partir de commits desde ultima tag
  - Ou usar GitHub auto-generated release notes
  - Incluir secoes: Features, Bug Fixes, Breaking Changes
- Configurar branch protection:
  - Require PR reviews antes de merge em main
  - Require status checks passing (build + tests)
  - Nao permitir push direto em main
