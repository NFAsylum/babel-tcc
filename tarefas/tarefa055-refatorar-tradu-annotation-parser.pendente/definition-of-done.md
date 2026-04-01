# Definition of Done - Tarefa 055

- [ ] TraduAnnotationParser nao depende mais de RoslynWrapper nem de Microsoft.CodeAnalysis
- [ ] ILanguageAdapter (ou interface separada) fornece: ExtractTrailingComments, GetIdentifierNamesOnLine, GetFirstStringLiteralOnLine, GetContainingMethodRange
- [ ] CSharpAdapter implementa todos os novos metodos delegando ao RoslynWrapper
- [ ] TranslationOrchestrator passa o adapter ao TraduAnnotationParser
- [ ] AssociateIdentifierOnLine usa adapter em vez de RoslynWrapper.GetIdentifierTokensOnLine
- [ ] AssociateLiteralOnLine usa adapter em vez de RoslynWrapper.GetAllTokensOnLine
- [ ] Calculo de escopo de metodo (ParameterMappings) usa adapter em vez de RoslynWrapper.GetMethodRange
- [ ] Formato tradu: continua funcionando identicamente para C#
- [ ] Todos os testes existentes de TraduAnnotationParser continuam passando
- [ ] Todos os testes existentes de CSharpAdapter continuam passando
- [ ] Todos os testes de integracao continuam passando
- [ ] Nenhuma regressao na suite de testes completa
