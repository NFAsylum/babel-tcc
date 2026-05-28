# Tarefa 045 - Deploy v1.0.0

## Fase
6 - Polimento e Deploy

## Objetivo
Realizar o deploy final da versao 1.0.0 da extensao no VS Code Marketplace, validando a instalacao e publicando pacotes complementares.

## Escopo
- Pre-deploy checklist:
  - Todos os testes passam (unitarios, integracao, e2e)
  - Code cleanup completo (Tarefa 041)
  - CI/CD pipeline funcional (Tarefa 042)
  - Marketplace assets prontos (Tarefa 043)
  - LICENSE e CHANGELOG presentes (Tarefa 044)
  - README.md finalizado (Tarefa 031)
  - Documentacao completa (Tarefas 032-033)
  - Security review limpo (Tarefa 040)
- Criar tag v1.0.0:
  - Atualizar versao para 1.0.0 em package.json
  - Atualizar versao para 1.0.0 em .csproj / Directory.Build.props
  - Commit de versao: "chore: bump version to 1.0.0"
  - Criar tag git: `git tag v1.0.0`
  - Push da tag: `git push origin v1.0.0`
- Build release:
  - CI/CD gera .vsix automaticamente via tag (ou manualmente com `vsce package`)
  - Verificar que .vsix contem apenas arquivos necessarios
  - Verificar tamanho do .vsix (ideal < 5MB)
- Publicar no VS Code Marketplace:
  - Via CI/CD automatico (preferido) ou `vsce publish`
  - Verificar pagina da extensao no Marketplace
  - Verificar que instalacao funciona via Marketplace
  - Verificar que features funcionam apos instalacao
- Verificacao pos-deploy:
  - Instalar extensao em VS Code limpo via Marketplace
  - Testar traducao basica end-to-end
  - Verificar que README, CHANGELOG, LICENSE aparecem no Marketplace
  - Verificar que screenshots e icone aparecem corretamente
- Publicacoes complementares (se aplicavel):
  - NuGet package do Core (para uso programatico)
  - NPM package do bridge (se aplicavel)
  - GitHub Release com binarios e release notes
