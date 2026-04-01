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
- 4 soft keywords (`match`, `case`, `type`, `_`) NAO serao incluidas nesta versao pois dependem de contexto sintatico para determinar se sao keywords.

## Decisao: schema vs keywords com maiuscula
O schema `keyword-table.schema.json` em babel-tcc-translations exige `^[a-z]+$` para nomes de keywords, mas Python tem `True`, `False`, `None` com maiuscula.
**Decisao**: Atualizar o schema para `^[a-zA-Z_]+$` (aceitar maiusculas e underscore). Justificativa:
- Armazenar em lowercase no JSON e converter no adapter criaria uma divergencia entre o JSON e a linguagem real, violando o principio de que o keywords-base.json reflete as keywords originais.
- O schema deve ser generico o suficiente para qualquer linguagem de programacao.
- Esta mudanca no schema faz parte da tarefa 057 (tabelas de traducao), que ja inclui babel-tcc-translations.
