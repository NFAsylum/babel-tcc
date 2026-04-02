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

### Tipos de token relevantes (valores de referencia para Python 3.12 — usar constantes `token.*`)
- `token.NAME`: identifiers e keywords
- `token.NUMBER`: literais numericos
- `token.STRING`: literais string (todos os tipos)
- `token.OP`: operadores e delimitadores
- `token.COMMENT`: comentarios `#`
- `token.FSTRING_START`, `token.FSTRING_MIDDLE`, `token.FSTRING_END`: f-strings decompostas (Python 3.12+ apenas)
- `token.INDENT`, `token.DEDENT`: indentacao

**IMPORTANTE**: Os IDs numericos dos tipos variam entre versoes do Python. Sempre usar as constantes do modulo `token`, nunca os numeros diretamente.

### Compatibilidade entre versoes do Python
- Os tipos FSTRING_START, FSTRING_MIDDLE, FSTRING_END (PEP 701) so existem no Python 3.12+.
- No Python 3.6-3.11, f-strings sao retornadas como um unico token STRING (3).
- O script deve funcionar em ambos os casos. Usar `hasattr(token, 'exact_type')` ou verificar se as constantes existem no modulo `token` antes de referencia-las.
- Nao hardcodar os IDs numericos dos tipos de token — usar as constantes do modulo `token` (ex: `token.NAME`, `token.STRING`).

### Configuracao no .csproj
O arquivo `.py` dentro de um projeto C# precisa ser configurado no `MultiLingualCode.Core.csproj`:
```xml
<ItemGroup>
  <None Include="LanguageAdapters\Python\tokenizer_service.py">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```
Isso garante que o script e copiado para o diretorio de saida do build sem que o MSBuild tente compila-lo.

### Tratamento de erros
- Erros de tokenizacao (ex: string nao terminada) retornam `{"ok": false, "error": "mensagem", "tokens": [...]}`
- Excecoes genericas nao devem matar o processo — capturar e retornar erro no JSON
- Requests com JSON invalido retornam erro sem crashar
