# Definition of Done - Tarefa 078

- [ ] ApplyTraduAnnotations nao destroi mapeamentos existentes no identifier-map
- [ ] Teste: carregar identifier-map com mapeamentos, traduzir arquivo, verificar que mapeamentos persistem
- [ ] refreshingPaths.delete executado em finally (nao apenas no happy path)
- [ ] Teste: simular falha no applyEdit e verificar que path e removido de refreshingPaths
- [ ] Todos os testes C# e TS passam
