# Definition of Done - Tarefa 085

- [ ] Diff de 3 vias implementado no Core (nao na extensao)
- [ ] Round-trip nao corrompe identificadores que coincidem com keywords traduzidas
- [ ] Escrita nativa (arquivo novo) funciona via diff contra vazio
- [ ] Teste: variavel "e" sobrevive round-trip com traducao "e" para "and"
- [ ] Teste: variavel "si" sobrevive round-trip com traducao "si" para "if" em es-es
- [ ] Teste: linhas iguais copiadas do original sem reverse
- [ ] Teste: linhas adicionadas recebem reverse translate
- [ ] Teste: linhas removidas sao removidas do original
- [ ] CSharpAdapter e PythonAdapter ambos suportam o novo fluxo
- [ ] ReverseSubstituteKeywords mantido como fallback
- [ ] Todos os testes existentes continuam passando
- [ ] Zero regressoes nos testes C# e TS
