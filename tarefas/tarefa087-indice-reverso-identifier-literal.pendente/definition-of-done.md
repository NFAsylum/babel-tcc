# Definition of Done - Tarefa 087

- [ ] IdentifierMapper tem indice reverso para identifiers (traducao -> original)
- [ ] IdentifierMapper tem indice reverso para literals (traducao -> original)
- [ ] GetOriginal() usa indice reverso (O(1) em vez de O(n))
- [ ] TranslateAstReverse para literais usa indice reverso (O(1) em vez de O(n))
- [ ] Indices reconstruidos automaticamente apos LoadMap, SetTranslation, SetLiteralTranslation
- [ ] Testes existentes continuam passando
- [ ] Teste novo: reverse lookup com mapa grande (100+ entries) retorna em tempo constante
