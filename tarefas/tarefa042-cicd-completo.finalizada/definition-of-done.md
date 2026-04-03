# Definition of Done - Tarefa 042

## Testes da extensao (pre-requisito)
- [ ] Test framework configurado (Vitest ou Mocha)
- [ ] Testes unitarios para KeywordMapService (carregamento, cache, retry, erro)
- [ ] Testes unitarios para LanguageDetector
- [ ] Testes unitarios para CompletionProvider e HoverProvider (com mock)
- [ ] `npm test` executa testes reais e falha se nenhum teste for encontrado
- [ ] Estrutura de testes espelha a estrutura do codigo (test/providers/, test/services/)

## CI pipeline
- [ ] CI roda automaticamente em todo push e PR para main
- [ ] CI executa build de Core (C#) e Extension (TypeScript)
- [ ] CI executa testes unitarios e de integracao
- [ ] CI gera relatorio de cobertura visivel no PR
- [ ] CI roda em pelo menos 2 plataformas (matrix strategy)
- [ ] Lint check integrado ao CI (falha build se houver problemas)
- [ ] Validacao de traducoes (validate.py) integrada ao CI

## Release pipeline
- [ ] Release pipeline gera .vsix em tag v*
- [ ] Release pipeline publica no VS Code Marketplace automaticamente
- [ ] GitHub Release criado automaticamente com .vsix como asset
- [ ] Release notes geradas automaticamente

## Infra
- [ ] Versionamento semantico implementado e sincronizado
- [ ] Branch protection configurada para main
- [ ] Todo o pipeline testado com pelo menos uma release de teste
- [ ] Secrets configurados no repositorio (VSCE_PAT para publish)
