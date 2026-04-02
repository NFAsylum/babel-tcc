# Contexto - Tarefa 056

## Dependencias
- Tarefa 053 (PythonTokenizerService)
- Tarefa 054 (PythonKeywordMap)
- Tarefa 055 (TraduAnnotationParser refatorado — interface ExtractTrailingComments)

## Bloqueia
- Tarefa 058 (registro no Program.cs)
- Tarefa 060 (testes)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/CSharpAdapter.cs (modelo de implementacao)
- packages/core/MultiLingualCode.Core/Interfaces/ILanguageAdapter.cs (interface a implementar)
- packages/core/MultiLingualCode.Core/Models/AST/ (modelos de nodes)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/ (diretorio criado nas tarefas anteriores)

## Notas
- O `PythonAdapter` sera o primeiro adapter alem do C#, servindo como validacao da extensibilidade da arquitetura.
- `CSharpAdapter.CollectReplacements()` e estatico — pode ser reutilizado diretamente ou extraido para um helper.
- Atencao a conversao de posicoes: Python tokenize retorna (line 1-based, col 0-based). O Generate precisa de offsets absolutos (caractere no source code). Calcular offset a partir de line/col contando newlines.
