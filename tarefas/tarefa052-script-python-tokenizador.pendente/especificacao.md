# Tarefa 052 - Script Python do tokenizador (tokenizer_service.py)

## Fase
7 - Suporte a Python

## Objetivo
Criar um script Python que usa o modulo `tokenize` da stdlib e roda como processo persistente, comunicando via JSON Lines (stdin/stdout).

## Escopo
- Criar script em `packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py`
- Usar `tokenize.generate_tokens()` para tokenizar codigo-fonte Python
- Usar `keyword.iskeyword()` para distinguir keywords de identifiers (ambos sao NAME)

### Protocolo de comunicacao
- Recebe via stdin: `{"source": "def foo():\n    pass"}` (uma linha JSON por request)
- Responde via stdout: JSON com lista de tokens
- Comando `{"cmd": "quit"}` encerra o processo
- `sys.stdout.flush()` apos cada resposta

### Formato de cada token na resposta
```json
{
  "type": 1,
  "typeName": "NAME",
  "string": "def",
  "startLine": 1,
  "startCol": 0,
  "endLine": 1,
  "endCol": 3,
  "isKeyword": true
}
```

### Tipos de token relevantes
- NAME (1): identifiers e keywords
- NUMBER (2): literais numericos
- STRING (3): literais string (todos os tipos)
- OP (55): operadores e delimitadores
- COMMENT (62): comentarios `#`
- FSTRING_START (59), FSTRING_MIDDLE (60), FSTRING_END (61): f-strings (Python 3.12+)
- INDENT (5), DEDENT (6): indentacao

### Tratamento de erros
- Erros de tokenizacao (ex: string nao terminada) retornam `{"ok": false, "error": "mensagem", "tokens": [...]}`
- Excecoes genericas nao devem matar o processo — capturar e retornar erro no JSON
- Requests com JSON invalido retornam erro sem crashar
