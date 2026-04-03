# Definition of Done - Tarefa 076

- [x] Teste verifica que Dispose encerra o processo Python
- [x] Teste verifica que processos nao acumulam em criacao/dispose sequencial
- [x] Teste verifica crash recovery (kill + re-tokenize)
- [x] Teste verifica que sem Dispose o processo continua vivo (leak detectavel)
- [x] Avaliacao documentada: LanguageRegistry deve implementar IDisposable?
- [x] Todos os testes passam

## Avaliacao: LanguageRegistry e IDisposable

LanguageRegistry NAO deve implementar IDisposable no momento. Motivos:

1. O registry armazena adapters mas nao e responsavel pelo lifecycle deles.
   Quem cria o adapter (Program.cs/CreateOrchestrator) e responsavel por
   cleanup. Transferir responsabilidade para o registry violaria o principio
   de ownership.

2. No modelo atual (processo persistente via tarefa 050), os adapters sao
   criados uma vez e vivem ate o processo encerrar. O .NET runtime limpa
   tudo no exit. Dispose explicito nao traz beneficio pratico.

3. Se no futuro adapters precisarem de cleanup durante a vida do processo
   (ex: hot-reload de adapters), ai sim o registry deveria propagar Dispose.
   Ate la, adicionar IDisposable e complexidade sem ganho.
