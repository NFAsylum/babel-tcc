# Definition of Done - Tarefa 059

- [ ] package.json: activationEvents inclui `onLanguage:python`
- [ ] package.json: languages inclui `mlc-python` com aliases
- [ ] package.json: grammars inclui entrada para `mlc-python` com TextMate grammar
- [ ] syntaxes/mlc-python.tmLanguage.json criado com syntax highlighting para Python traduzido
- [ ] languageDetector.ts reconhece `.py` como Python
- [ ] File watcher monitora arquivos `.py` alem de `.cs`
- [ ] hoverProvider.ts usa language hint dinamico (nao hardcoded 'csharp')
- [ ] autoTranslateManager.ts funciona com arquivos Python (sem logica hardcoded para .cs)
- [ ] coreBridge.ts passa extensao do arquivo ao Core para resolucao correta do adapter
- [ ] Extensao compila sem erros
- [ ] Testes existentes da extensao continuam passando
