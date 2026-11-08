# Definition of Done - Tarefa 121

- [x] `[]`, `5`, `"texto"`, `null` e `true` recebem resposta de erro e o processo continua
      (2026-10-12: para cada entrada seguida de uma requisicao valida, exit 0, stderr vazio,
      primeira resposta `Invalid request: expected a JSON object`, segunda com `ok: true`)
- [x] Teste de regressao falha contra o codigo antigo e passa com a correcao
      (2026-10-12: `dotnet test --filter "FullyQualifiedName~RawRequest_ValidJson"` - 1 falha
      com o script da `main`, 1 de 1 aprovado com a correcao)
- [x] Requisicoes que ja funcionavam respondem igual
      (2026-10-12: 165 requisicoes - 150 modulos da biblioteca padrao, exemplos, JSON invalido,
      `quit` - com stdout byte a byte identico entre o script antigo e o novo)
- [x] Testes do Core passam (`dotnet test --filter "Category!=Research"`)
      (2026-10-12: 703 de 703, com Python 3.14 instalado e nenhum teste pulado)
- [x] `dotnet format --verify-no-changes` limpo
- [x] Script continua compativel com a sintaxe do Python 3.8
      (2026-10-12: `ast.parse(..., feature_version=(3, 8))` aceita. Executado em 3.12 e 3.14;
      nao executado em um interpretador 3.8)
- [x] Tarefa marcada como .finalizada no mesmo PR
