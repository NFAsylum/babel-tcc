# Definition of Done - Tarefa 053

- [ ] Classe `PythonTokenizerService` criada em `LanguageAdapters/Python/`
- [ ] Modelo `PythonToken` criado com todos os campos do protocolo
- [ ] Resolucao do executavel Python: tenta python3, python, py -3 (nessa ordem)
- [ ] Permite override via configuracao do usuario (setting ou variavel de ambiente)
- [ ] Valida que o Python encontrado e versao >= 3.8
- [ ] Inicia processo Python com tokenizer_service.py como persistente
- [ ] Metodo `Tokenize(string sourceCode)` envia request e retorna lista de tokens
- [ ] Implementa `IDisposable` com encerramento limpo do processo
- [ ] Trata caso Python nao instalado com mensagem listando caminhos tentados
- [ ] Trata crash do processo com tentativa de restart
- [ ] Trata timeout por request
- [ ] Desserializa JSON de resposta corretamente em objetos PythonToken
