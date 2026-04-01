# Tarefa 055 - Refatorar TraduAnnotationParser para ser agnostico de linguagem

## Fase
7 - Suporte a Python

## Objetivo
Desacoplar o TraduAnnotationParser do Roslyn para que funcione com qualquer linguagem de programacao. Atualmente ele usa `RoslynWrapper.ParseSourceCode()` e `RoslynWrapper.GetTrailingCommentText()` internamente, o que o torna exclusivo para C#. Python usa `#` para comentarios.

## Escopo

### Problema atual
O `TraduAnnotationParser.ExtractAnnotations(string sourceCode)` mistura duas responsabilidades:
1. **Extrai comentarios** do codigo-fonte (acoplado ao Roslyn — comentarios `//`)
2. **Parseia o formato `tradu:`** do texto do comentario (logica generica)

### Solucao: separar as responsabilidades
Cada `ILanguageAdapter` passa a ser responsavel por extrair comentarios do codigo-fonte. O `TraduAnnotationParser` recebe comentarios ja extraidos e foca apenas em parsear o formato `tradu:`.

### Mudancas na interface ILanguageAdapter
Adicionar metodo:
```csharp
List<TrailingComment> ExtractTrailingComments(string sourceCode);
```
Onde `TrailingComment` e um novo modelo com:
- `string Text` — texto do comentario (sem o prefixo `//` ou `#`)
- `int Line` — numero da linha (0-based)

### Mudancas no TraduAnnotationParser
- Novo metodo: `ExtractAnnotations(string sourceCode, List<TrailingComment> comments)` ou refatorar o existente para receber comentarios em vez de source code
- Manter os metodos internos de parsing do formato tradu (ParseAnnotationText, regex de language prefix, etc.)
- Remover dependencias de RoslynWrapper

### Mudancas no TranslationOrchestrator
- Em `ApplyTraduAnnotations()`: obter adapter do registry, chamar `adapter.ExtractTrailingComments(sourceCode)`, passar resultado ao parser
- Em `ApplyReverseTraduAnnotations()`: mesma mudanca

### Mudancas no CSharpAdapter
- Implementar `ExtractTrailingComments()` movendo a logica atual do TraduAnnotationParser que usa Roslyn
- Extrair comentarios `//` trailing usando `RoslynWrapper.GetTrailingCommentText()`

### Impacto para o PythonAdapter (tarefa 056)
- Implementar `ExtractTrailingComments()` usando tokens COMMENT do subprocesso tokenizador
- Comentarios Python: `# tradu:...`

## IMPORTANTE
- Nao quebrar testes existentes do CSharpAdapter e TraduAnnotationParser
- Rodar suite de testes completa apos refatoracao
