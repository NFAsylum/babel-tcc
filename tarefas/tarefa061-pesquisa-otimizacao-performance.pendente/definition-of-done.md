# Definition of Done - Tarefa 061

## Pesquisa
- [ ] Traducao incremental: prototipo ou benchmark do Roslyn WithChangedText
- [ ] Substituicao por texto: prototipo de scanner linear com skip de strings/comentarios
- [ ] Cache por metodo: prototipo de hashing por bloco sintatico
- [ ] Traducao lazy: avaliacao de viabilidade com FileSystemProvider do VS Code

## Benchmarks
- [ ] Cada metodo testado com arquivos de 1.700, 17.000 e 85.000 linhas
- [ ] Cenario de edicao incremental (1 linha alterada) medido
- [ ] Cenario de cache hit (troca de tab) medido
- [ ] Resultados em tabela comparativa

## Relatorio
- [ ] Comparacao nos criterios de performance (tempos vs metas)
- [ ] Comparacao nos criterios de UX (delay, flicker, consistencia)
- [ ] Comparacao de viabilidade (complexidade, risco, manutencao)
- [ ] Recomendacao fundamentada de qual metodo implementar
- [ ] Riscos e limitacoes documentados
