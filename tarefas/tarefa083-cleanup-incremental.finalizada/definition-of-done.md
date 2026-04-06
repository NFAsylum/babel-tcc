# Definition of Done - Tarefa 083

- [x] LOW-001: Docs dizem Node.js 20+ (corrigido em tarefa 064/082)
- [x] LOW-002: Exemplos seguem convencoes (atualizados pelo outro agente)
- [x] LOW-003: tarefa050 renomeada para .finalizada (feito anteriormente)
- [x] LOW-004: Campos TS public — aceitar como padrao do projeto
  (testes acessam campos diretamente, mudar para private quebraria testes)
- [x] LOW-005: ContainsKey+indexer — aceitar como padrao
  (TryGetValue requer out nullable que viola CONTRIBUTING.md. Pattern
  atual e seguro sem concorrencia multi-thread.)
- [x] LOW-006: README padronizado para formato tradu[pt-br]:
- [x] LOW-007: Titulos de commands padronizados para ingles.
  stdout!: spawn('dotnet', args) usa stdio default 'pipe', stdout
  garantido nao-null pelo runtime Node.js.
  resolveTranslationsPath public: coreBridge.test.ts acessa
  directamente nos testes (mesmo motivo que LOW-004).
