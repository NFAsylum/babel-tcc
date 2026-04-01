# Definition of Done - Tarefa 052

- [ ] Script `tokenizer_service.py` criado em `LanguageAdapters/Python/`
- [ ] Usa apenas modulos da stdlib (tokenize, keyword, json, sys, io, token)
- [ ] Recebe requests JSON via stdin e responde via stdout
- [ ] Cada token inclui: type, typeName, string, startLine, startCol, endLine, endCol, isKeyword
- [ ] Classifica corretamente keywords vs identifiers via `keyword.iskeyword()`
- [ ] Trata todos os tipos de string Python (single, double, triple, f-string, raw, byte)
- [ ] Trata erros de tokenizacao sem crashar o processo
- [ ] Trata JSON invalido no request sem crashar o processo
- [ ] Comando `{"cmd": "quit"}` encerra o processo normalmente
- [ ] MultiLingualCode.Core.csproj configurado para copiar tokenizer_service.py para o output directory
- [ ] Testavel manualmente via `echo '{"source": "def foo(): pass"}' | python tokenizer_service.py`
- [ ] Funciona em Python 3.8+ (sem depender de constantes exclusivas do 3.12)
