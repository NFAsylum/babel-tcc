# Tarefa 042 - CI/CD completo

## Fase
6 - Polimento e Deploy

## Objetivo
Configurar pipeline CI/CD completo com build, testes, geracao de artefatos e publicacao automatica via tags.

## Escopo

### Pre-requisito: testes da extensao TypeScript
O CI precisa de testes reais para executar. Atualmente `npm test` imprime
"No tests configured yet" e passa verde — falsa sensacao de seguranca.

- Configurar test framework (Vitest ou Mocha + @vscode/test-electron)
- Estrutura de testes espelhando o codigo (mesma convencao do Core C#):
  ```
  src/providers/keywordMap.ts              -> test/providers/keywordMap.test.ts
  src/providers/completionProvider.ts      -> test/providers/completionProvider.test.ts
  src/providers/hoverProvider.ts           -> test/providers/hoverProvider.test.ts
  src/providers/autoTranslateManager.ts    -> test/providers/autoTranslateManager.test.ts
  src/providers/translatedContentProvider  -> test/providers/translatedContentProvider.test.ts
  src/services/languageDetector.ts         -> test/services/languageDetector.test.ts
  src/services/configurationService.ts     -> test/services/configurationService.test.ts
  src/services/coreBridge.ts               -> test/services/coreBridge.test.ts
  src/ui/statusBar.ts                      -> test/ui/statusBar.test.ts
  src/extension.ts                         -> test/extension.test.ts
  ```
- Prioridade 1 (logica pura, sem mock de vscode API):
  - KeywordMapService: carregamento, cache, invalidacao, retry apos erro
  - LanguageDetector: deteccao por extensao
- Prioridade 2 (requer mock de vscode):
  - CompletionProvider, HoverProvider
  - ConfigurationService
  - StatusBar
- Prioridade 3 (requer mock de vscode + child_process/filesystem):
  - CoreBridge
  - TranslatedContentProvider
  - AutoTranslateManager
- Prioridade 4 (integracao — requer todos os servicos):
  - extension.ts (activate/deactivate)
- Garantir que `npm test` falha se nenhum teste real for encontrado

### GitHub Actions - CI (em push e PR)
- Build do Core C# (`dotnet build`)
- Build da Extension TypeScript (`npm run build`)
- Rodar testes unitarios do Core (`dotnet test`)
- Rodar testes unitarios da Extension (`npm test`)
- Rodar testes de integracao
- Gerar relatorio de cobertura (coverlet para C#, c8/istanbul para TS)
  - Cobertura visivel como comentario no PR ou via badge
- Lint check (dotnet format --verify-no-changes, eslint)
- Matrix strategy: Windows + Ubuntu (minimo)
- Validacao de traducoes: rodar `python scripts/validate.py` contra os
  JSONs embeddados ou contra o repo babel-tcc-translations como submodule/checkout

### GitHub Actions - Release (em tag v*)
- Build otimizado (Release configuration)
- Rodar todos os testes
- Gerar pacote .vsix (`vsce package`)
- Publicar no VS Code Marketplace (`vsce publish`)
- Criar GitHub Release com .vsix como asset
- Gerar release notes automaticas (changelog ou git log)

### Versionamento semantico
- Usar SemVer (MAJOR.MINOR.PATCH)
- Versao definida em package.json e sincronizada com .csproj
- Tag git como trigger de release (v1.0.0, v1.0.1, etc.)

### Release notes automaticas
- Gerar a partir de commits desde ultima tag
- Ou usar GitHub auto-generated release notes
- Incluir secoes: Features, Bug Fixes, Breaking Changes

### Configurar branch protection
- Require PR reviews antes de merge em main
- Require status checks passing (build + tests)
- Nao permitir push direto em main
