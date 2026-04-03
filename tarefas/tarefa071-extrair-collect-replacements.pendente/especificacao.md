# Tarefa 071 - Extrair CollectReplacements para classe compartilhada e corrigir logica de literais

## Prioridade: HIGH

## Problema
`CSharpAdapter.CollectReplacements()` e um metodo estatico que o `PythonAdapter.Generate()` chama diretamente. Este metodo tem logica especifica de C# para literais string:

```csharp
case LiteralNode literal when literal.Type == LiteralType.String:
    string quotedValue = "\"" + literal.Value + "\"";
```

Isto forca aspas duplas em todas as strings. Se o codigo Python original era `'hello'`, o Parse extrai `hello` e o Generate reconstroi como `"hello"` — mudando o tipo de aspas silenciosamente. O mesmo problema afetaria qualquer linguagem futura com sintaxe de string diferente.

## Escopo

### 1. Extrair para helper compartilhado
Mover `CollectReplacements` de `CSharpAdapter` para uma classe como `AdapterHelpers` ou `ASTHelpers` em `LanguageAdapters/`. Remover a logica de quoting de literais — cada adapter deve ser responsavel por reconstruir seus literais.

### 2. Corrigir reconstrucao de literais no PythonAdapter
O `PythonAdapter.Parse()` deve preservar o texto original do literal (incluindo aspas e prefixos) no `LiteralNode`, para que `Generate()` reconstroi corretamente. Opcoes:
- Guardar o raw token text no LiteralNode (ex: `'hello'` ou `f"world"` completo)
- Ou adicionar campo de metadata no LiteralNode para o tipo de aspas

### 3. Manter compatibilidade com CSharpAdapter
O CSharpAdapter deve continuar funcionando como antes. A logica de `"\"" + literal.Value + "\""` pode permanecer no CSharpAdapter ou num override do helper.

## Impacto
Bug silencioso: aspas simples do Python viram duplas no round-trip.
