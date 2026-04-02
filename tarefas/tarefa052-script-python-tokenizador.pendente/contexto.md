# Contexto - Tarefa 052

## Dependencias
- Nenhuma

## Bloqueia
- Tarefa 053 (PythonTokenizerService C# depende do script)

## Arquivos relevantes
- docs/decisoes-tecnicas.md (DT-005 sistema de IDs)
- Documentacao CPython: https://docs.python.org/3/library/tokenize.html
- Documentacao CPython: https://docs.python.org/3/library/keyword.html

## Decisao tecnica
Usar o tokenizador do proprio CPython via subprocesso em vez de reimplementar em C#.
Justificativa:
- 100% de confiabilidade (e o tokenizador de referencia)
- Zero manutencao (evolui com o Python do usuario)
- Desempenho adequado: ~0.13ms por request com processo persistente
- Compatibilidade automatica com qualquer versao do Python instalada
- Alternativas descartadas: IronPython (preso no Python 3.4), ANTLR4 (bugs INDENT/DEDENT no C#), custom tokenizer (edge cases complexos)

## Notas
- O modulo `tokenize` retorna NAME (type=1) para keywords E identifiers. `keyword.iskeyword()` distingue.
- O script deve ser leve e sem dependencias externas (apenas stdlib).
- Padrao inspirado no Language Server Protocol (JSON Lines via stdio).
