# Contexto - Tarefa 055

## Dependencias
- Nenhuma (refatoracao do codigo existente)

## Bloqueia
- Tarefa 056 (PythonAdapter precisa da interface refatorada para implementar ExtractTrailingComments)

## Arquivos afetados
- packages/core/MultiLingualCode.Core/LanguageAdapters/TraduAnnotationParser.cs (refatorar)
- packages/core/MultiLingualCode.Core/Interfaces/ILanguageAdapter.cs (adicionar metodo)
- packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs (adaptar chamadas)
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (implementar novo metodo)
- packages/core/MultiLingualCode.Core/Models/ (novo modelo TrailingComment se necessario)
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/TraduAnnotationParserTests.cs (adaptar testes)

## Notas
- Decisao tomada com o usuario: Opcao A (refatorar para generico) — maximiza modularidade e expansibilidade
- O TraduAnnotationParser tambem usa `RoslynWrapper.GetAllTokensOnLine()`, `RoslynWrapper.GetMethodRange()`, `RoslynWrapper.GetIdentifierTokensOnLine()` para associar anotacoes a identifiers e calcular escopo de metodos. Essas dependencias tambem precisam ser abstraidas ou movidas para o adapter.
- Abordagem alternativa: se a abstracao completa for muito complexa, considerar passar o adapter ao parser e deixar o parser chamar metodos do adapter conforme necessario.
