# Contexto - Tarefa 057

## Dependencias
- Tarefa 054 (PythonKeywordMap — IDs devem ser identicos)

## Bloqueia
- Tarefa 060 (testes de integracao precisam das tabelas de traducao)

## Arquivos relevantes
- babel-tcc-translations/programming-languages/csharp/keywords-base.json (modelo)
- babel-tcc-translations/natural-languages/pt-br/csharp.json (modelo de traducao)
- babel-tcc-translations/natural-languages/template.json (template para novos idiomas)
- babel-tcc-translations/schema/keyword-table.schema.json (validacao)
- babel-tcc-translations/schema/translation.schema.json (validacao)
- babel-tcc-translations/README.md (atualizar)

## Notas
- Os IDs no keywords-base.json DEVEM ser identicos aos do PythonKeywordMap.cs. Qualquer divergencia causa falha na traducao.
- O NaturalLanguageProvider carrega arquivos do caminho `programming-languages/{langKey}/keywords-base.json` onde langKey = adapter.LanguageName.ToLowerInvariant() = "python".
- Para traducoes, carrega de `natural-languages/{languageCode}/python.json`.
- O campo `programmingLanguage` no JSON de traducao deve ser `"Python"` (PascalCase, matching adapter.LanguageName).
