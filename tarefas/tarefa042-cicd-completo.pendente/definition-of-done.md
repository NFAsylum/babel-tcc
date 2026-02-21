# Definition of Done - Tarefa 042

- [ ] CI roda automaticamente em todo push e PR para main
- [ ] CI executa build de Core (C#) e Extension (TypeScript)
- [ ] CI executa testes unitarios e de integracao
- [ ] CI gera relatorio de cobertura visivel no PR
- [ ] CI roda em pelo menos 2 plataformas (matrix strategy)
- [ ] Lint check integrado ao CI (falha build se houver problemas)
- [ ] Release pipeline gera .vsix em tag v*
- [ ] Release pipeline publica no VS Code Marketplace automaticamente
- [ ] GitHub Release criado automaticamente com .vsix como asset
- [ ] Release notes geradas automaticamente
- [ ] Versionamento semantico implementado e sincronizado
- [ ] Branch protection configurada para main
- [ ] Todo o pipeline testado com pelo menos uma release de teste
- [ ] Secrets configurados no repositorio (VSCE_PAT para publish)
