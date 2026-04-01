# Definition of Done - Tarefa 053

- [ ] Classe `PythonTokenizerService` criada em `LanguageAdapters/Python/`
- [ ] Modelo `PythonToken` criado com todos os campos do protocolo
- [ ] Inicia processo Python com tokenizer_service.py como persistente
- [ ] Metodo `Tokenize(string sourceCode)` envia request e retorna lista de tokens
- [ ] Implementa `IDisposable` com encerramento limpo do processo
- [ ] Trata caso Python nao instalado com mensagem de erro clara
- [ ] Trata crash do processo com tentativa de restart
- [ ] Trata timeout por request
- [ ] Desserializa JSON de resposta corretamente em objetos PythonToken
