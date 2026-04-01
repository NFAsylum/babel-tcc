# Tarefa 055 - Refatorar TraduAnnotationParser para ser agnostico de linguagem

## Fase
7 - Suporte a Python

## Objetivo
Desacoplar o TraduAnnotationParser do Roslyn para que funcione com qualquer linguagem de programacao. Python usa `#` para comentarios, e nao pode ser parseado com Roslyn.

## Escopo

### Problema atual
O `TraduAnnotationParser.ExtractAnnotations(string sourceCode)` depende de **4 metodos do Roslyn**:
1. `RoslynWrapper.ParseSourceCode()` + `RoslynWrapper.GetTrailingCommentText()` — extrai comentarios `//` trailing
2. `RoslynWrapper.GetIdentifierTokensOnLine()` — associa anotacao ao identifier na mesma linha (`AssociateIdentifierOnLine`, linha 163)
3. `RoslynWrapper.GetAllTokensOnLine()` — associa anotacao ao literal string na mesma linha (`AssociateLiteralOnLine`, linha 179)
4. `RoslynWrapper.GetMethodRange()` — calcula escopo (startLine, endLine) do metodo para parameter mappings (linha 85)

Todas essas dependencias sao especificas do Roslyn/C# e precisam ser abstraidas.

### Solucao: interface de suporte a anotacoes no adapter
Cada `ILanguageAdapter` passa a fornecer as operacoes que o TraduAnnotationParser precisa. O parser recebe dados ja extraidos e foca apenas no formato `tradu:`.

### Mudancas na interface ILanguageAdapter
Adicionar metodos:
```csharp
List<TrailingComment> ExtractTrailingComments(string sourceCode);
List<string> GetIdentifierNamesOnLine(string sourceCode, int line);
string GetFirstStringLiteralOnLine(string sourceCode, int line);
(int StartLine, int EndLine) GetContainingMethodRange(string sourceCode, int line);
```

Modelo `TrailingComment`:
- `string Text` — texto do comentario (sem o prefixo `//` ou `#`)
- `int Line` — numero da linha (0-based)

Alternativa: se adicionar 4 metodos a ILanguageAdapter for excessivo, criar uma interface separada `IAnnotationSupport` que adapters podem implementar opcionalmente. Se o adapter nao implementar, anotacoes tradu nao sao suportadas para aquela linguagem.

### Mudancas no TraduAnnotationParser
- Refatorar `ExtractAnnotations()` para receber o adapter (ou a interface de suporte) em vez de parsear source code diretamente
- Substituir chamadas a RoslynWrapper por chamadas ao adapter:
  - `RoslynWrapper.GetTrailingCommentText()` -> `adapter.ExtractTrailingComments()`
  - `RoslynWrapper.GetIdentifierTokensOnLine()` -> `adapter.GetIdentifierNamesOnLine()`
  - `RoslynWrapper.GetAllTokensOnLine()` + `IsStringLiteralToken()` -> `adapter.GetFirstStringLiteralOnLine()`
  - `RoslynWrapper.GetMethodRange()` -> `adapter.GetContainingMethodRange()`
- Manter metodos internos de parsing do formato tradu (ParseAnnotationText, regex de language prefix, etc.)
- Remover todas as dependencias de RoslynWrapper e Microsoft.CodeAnalysis

### Mudancas no TranslationOrchestrator
- Em `ApplyTraduAnnotations()`: obter adapter do registry, passar ao TraduAnnotationParser
- Em `ApplyReverseTraduAnnotations()`: mesma mudanca

### Mudancas no CSharpAdapter
- Implementar os novos metodos delegando ao RoslynWrapper (mover logica existente):
  - `ExtractTrailingComments()` — usa `RoslynWrapper.GetTrailingCommentText()`
  - `GetIdentifierNamesOnLine()` — usa `RoslynWrapper.GetIdentifierTokensOnLine()`
  - `GetFirstStringLiteralOnLine()` — usa `RoslynWrapper.GetAllTokensOnLine()` + `IsStringLiteralToken()`
  - `GetContainingMethodRange()` — usa `RoslynWrapper.GetMethodRange()`

### Impacto para o PythonAdapter (tarefa 056)
- Implementar `ExtractTrailingComments()` usando tokens COMMENT do subprocesso tokenizador
- Implementar `GetIdentifierNamesOnLine()` filtrando tokens NAME na linha
- Implementar `GetFirstStringLiteralOnLine()` filtrando tokens STRING na linha
- Implementar `GetContainingMethodRange()` detectando bloco `def` por indentacao (ou busca simples por `def` acima da linha)
- Comentarios Python: `# tradu:...`

## IMPORTANTE
- Nao quebrar testes existentes do CSharpAdapter e TraduAnnotationParser
- Rodar suite de testes completa apos refatoracao
