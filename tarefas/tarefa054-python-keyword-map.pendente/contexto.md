# Contexto - Tarefa 054

## Dependencias
- Nenhuma

## Bloqueia
- Tarefa 056 (PythonAdapter usa PythonKeywordMap)
- Tarefa 057 (tabelas de traducao devem usar os mesmos IDs)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpKeywordMap.cs (modelo de estrutura)
- docs/decisoes-tecnicas.md (DT-005 sistema de IDs numericos)

## Notas
- Python e case-sensitive: `True`, `False`, `None` sao keywords com maiuscula.
- O schema `keyword-table.schema.json` no babel-tcc-translations exige `^[a-z]+$` para nomes de keywords. Verificar se precisa ser atualizado para aceitar `True`, `False`, `None` — ou se armazenamos em lowercase no JSON e fazemos a conversao no adapter.
- 4 soft keywords (`match`, `case`, `type`, `_`) NAO serao incluidas nesta versao pois dependem de contexto sintatico para determinar se sao keywords.
