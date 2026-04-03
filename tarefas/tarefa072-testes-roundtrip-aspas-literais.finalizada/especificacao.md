# Tarefa 072 - Testes round-trip para aspas e literais por linguagem

## Prioridade: HIGH

## Problema
Nenhum teste verifica que o tipo de aspas e preservado no round-trip. O bug de CollectReplacements (tarefa 071) corrompe aspas simples do Python silenciosamente.

## Escopo
Criar testes para ambos os adapters que verificam:

### Python
- `'hello'` -> parse -> generate = `'hello'` (nao `"hello"`)
- `"hello"` -> parse -> generate = `"hello"`
- `'''multiline'''` -> parse -> generate preserva triple quotes
- `"""multiline"""` -> parse -> generate preserva triple quotes
- `f"hello {name}"` -> parse -> generate preserva f-string
- `r"raw\path"` -> parse -> generate preserva raw string
- `b"bytes"` -> parse -> generate preserva byte string

### C#
- `"hello"` -> parse -> generate = `"hello"`
- `@"verbatim"` -> parse -> generate = `@"verbatim"`
- `$"interpolated {x}"` -> parse -> generate preserva

### Round-trip completo (via orchestrator)
- Codigo Python com aspas simples -> traduzir -> reverter -> comparar exato
- Codigo Python com f-strings -> traduzir -> reverter -> comparar exato
