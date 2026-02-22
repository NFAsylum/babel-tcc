# Definition of Done - Tarefa 047

## Criterios de aceite

- [ ] `TranslateFromNaturalLanguage` reverte TODAS as 77 keywords traduzidas para C# original
- [ ] Round-trip completo funciona: C# -> PT-BR -> C# produz codigo identico ao original
- [ ] Keywords dentro de strings NAO sao substituidas (ex: `"a classe e boa"` permanece intacta)
- [ ] Keywords dentro de comentarios NAO sao substituidas
- [ ] Substituicoes parciais NAO acontecem (ex: "classement" nao vira "classment")
- [ ] Identificadores traduzidos via `// tradu:` continuam sendo revertidos corretamente
- [ ] Testes de round-trip end-to-end passam com exemplos reais (Calculator, HelloWorld)
- [ ] Todos os testes existentes continuam passando
- [ ] Performance: reversao de arquivo 100 linhas < 500ms
- [ ] `dotnet build` sem erros
- [ ] `dotnet test` todos passam
