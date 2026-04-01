# Tarefa 058 - Registrar PythonAdapter no Program.cs e corrigir hardcodes

## Fase
7 - Suporte a Python

## Objetivo
Atualizar o host CLI para registrar o PythonAdapter e remover referencias hardcoded ao CSharpAdapter.

## Escopo

### 1. CreateOrchestrator() (linhas ~153-168)
Atualmente:
```csharp
CSharpAdapter adapter = new CSharpAdapter();
LanguageRegistry registry = new LanguageRegistry();
registry.RegisterAdapter(adapter);
```

Adicionar registro do PythonAdapter:
```csharp
LanguageRegistry registry = new LanguageRegistry();
registry.RegisterAdapter(new CSharpAdapter());
registry.RegisterAdapter(new PythonAdapter());
```

### 2. HandleValidateSyntax() (linhas ~215-225)
Atualmente cria `new CSharpAdapter()` hardcoded para validacao:
```csharp
CSharpAdapter adapter = new CSharpAdapter();
ValidationResult validation = adapter.ValidateSyntax(request.SourceCode);
```

Refatorar para usar o LanguageRegistry:
- O request precisa incluir a extensao do arquivo para que o registry resolva o adapter correto
- Verificar se `ValidateRequest` ja tem campo de extensao/linguagem; se nao, adicionar
- Usar `registry.GetAdapter(extension)` para obter o adapter correto

### 3. Verificar outros metodos em Program.cs
- Verificar se `HandleTranslateToNaturalLanguage` e `HandleTranslateFromNaturalLanguage` ja usam o registry corretamente (via TranslationOrchestrator)
- Verificar se `HandleGetSupportedLanguages` precisa ser atualizado
