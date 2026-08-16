# Definition of Done - Tarefa 112

- [ ] Inventario recontado no inicio (os numeros da especificacao sao de 2026-08-16)
- [ ] Kotlin sem `private` e sem `internal` no codigo de producao
- [ ] Kotlin sem `throw`: erro atravessa como valor via Result pattern
- [ ] Degradacao das bordas preservada: Core quebrado mostra o original e nunca corrompe o
      arquivo em disco (DT-003)
- [ ] Kotlin sem tipos anulaveis fora dos boundaries com APIs Java, e esses documentados
      no ponto da chamada
- [ ] Tipo explicito nas declaracoes Kotlin (`val timeoutMs: Long = 10_000`)
- [ ] `ProcessTransport` movido para arquivo proprio, uma classe por arquivo
- [ ] tokenizer_service.py com anotacoes de tipo nas assinaturas
- [ ] Scripts de build reindentados para 2 espacos conforme .editorconfig
- [ ] Testes do plugin passam (`./gradlew test`, 58 testes)
- [ ] Testes do Core passam (`dotnet test --filter "Category!=Research"`)
- [ ] `dotnet format --verify-no-changes` limpo
- [ ] Tokenizacao Python validada de ponta a ponta (os testes com [RequiresPythonFact])
- [ ] Tabela de conformidade em docs/padroes-codigo.md atualizada ou removida quando o
      desvio correspondente deixar de existir
- [ ] Tarefa marcada como .finalizada no mesmo PR que faz o trabalho
