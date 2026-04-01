# Tarefa 057 - Tabelas de traducao para Python no babel-tcc-translations

## Fase
7 - Suporte a Python

## Objetivo
Criar todos os arquivos de traducao necessarios para Python no repositorio `babel-tcc-translations`.

## Escopo

### Arquivo base de keywords
Criar `programming-languages/python/keywords-base.json`:
```json
{
  "keywords": {
    "keyword": id,
    ...
  }
}
```
- Deve conter as 35 keywords do Python com os mesmos IDs definidos no `PythonKeywordMap.cs` (tarefa 054)
- Deve validar contra `schema/keyword-table.schema.json`

### Atualizar schema/keyword-table.schema.json
O schema atual exige `^[a-z]+$` para nomes de keywords, mas Python tem `True`, `False`, `None` com maiuscula.
**Decisao (ver tarefa 054 contexto.md)**: Atualizar o pattern para `^[a-zA-Z_]+$` para suportar qualquer linguagem.
```json
// Atual:
"^[a-z]+$": { "type": "integer", "minimum": 0 }

// Novo:
"^[a-zA-Z_]+$": { "type": "integer", "minimum": 0 }
```

### Traducoes para cada idioma natural (8 arquivos)
Criar um arquivo `python.json` em cada diretorio de idioma:
- `natural-languages/pt-br/python.json`
- `natural-languages/pt-br-ascii/python.json`
- `natural-languages/en-us/python.json`
- `natural-languages/es-es/python.json`
- `natural-languages/fr-fr/python.json`
- `natural-languages/de-de/python.json`
- `natural-languages/it-it/python.json`
- `natural-languages/ja-jp-romaji/python.json`

Formato:
```json
{
  "version": "1.0.0",
  "languageCode": "<code>",
  "languageName": "<nome nativo>",
  "programmingLanguage": "Python",
  "translations": {
    "<id>": "<traducao>",
    ...
  }
}
```

### Regras de traducao (do README)
- Caracteres nativos com acentos permitidos
- Tudo em lowercase
- Sem espacos em palavras compostas
- Todas as keyword IDs devem ter traducao (completude)
- Sem traducoes duplicadas (unicidade) dentro do mesmo arquivo
- Deve validar contra `schema/translation.schema.json`

### Keywords exclusivas do Python que precisam traducao cuidadosa
- `def` — definir? funcao?
- `elif` — senaose?
- `del` — deletar? apagar?
- `lambda` — manter ou traduzir?
- `nonlocal` — naolocal?
- `pass` — passar?
- `raise` — levantar? lancar?
- `yield` — produzir?
- `with` — com?
- `assert` — afirmar? assegurar?
- `not`, `and`, `or` — nao, e, ou?
- `True`, `False`, `None` — verdadeiro, falso, nulo?

Referencia: usar traducoes equivalentes do C# onde aplicavel (ex: `true`->`verdadeiro`, `false`->`falso`, `null`->`nulo`).

### Atualizar README.md
Atualizar o README do babel-tcc-translations para listar Python como linguagem suportada.
