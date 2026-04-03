# Contexto - Tarefa 076

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/PythonTokenizerServiceTests.cs (novos testes)
- packages/core/MultiLingualCode.Core/Services/LanguageRegistry.cs (possivelmente IDisposable)

## Notas
- PythonAdapter implementa IDisposable, delega ao PythonTokenizerService
- PythonTokenizerService envia quit + kill no Dispose
- Testes devem usar [RequiresPythonFact] (dependem de Python)
- No futuro com Host persistente (tarefa 050), este problema se torna critico
