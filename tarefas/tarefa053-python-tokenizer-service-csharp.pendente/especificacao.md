# Tarefa 053 - PythonTokenizerService em C# (comunicacao com subprocesso)

## Fase
7 - Suporte a Python

## Objetivo
Criar classe C# que gerencia o subprocesso Python e fornece tokenizacao para o PythonAdapter.

## Escopo
- Criar `packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonTokenizerService.cs`
- Criar modelo `PythonToken.cs` para representar tokens recebidos do subprocesso

### Classe PythonTokenizerService
- Iniciar processo Python com `tokenizer_service.py` via `Process.Start`
- `ProcessStartInfo`: `CreateNoWindow = true`, `UseShellExecute = false`, `RedirectStandardInput = true`, `RedirectStandardOutput = true`, `RedirectStandardError = true`
- Capturar stderr para diagnosticar erros fatais do Python (ex: SyntaxError no proprio script, modulo nao encontrado)
- Processo persistente (reutilizado entre chamadas)
- Implementar `IDisposable` para encerrar o processo no cleanup

### Metodos publicos
- `OperationResultGeneric<List<PythonToken>> Tokenize(string sourceCode)`: envia codigo, recebe tokens
- `void Dispose()`: envia `{"cmd": "quit"}` e encerra processo

### Modelo PythonToken
```csharp
public class PythonToken
{
    public int Type { get; set; }
    public string TypeName { get; set; }
    public string String { get; set; }
    public int StartLine { get; set; }
    public int StartCol { get; set; }
    public int EndLine { get; set; }
    public int EndCol { get; set; }
    public bool IsKeyword { get; set; }
}
```

### Tratamento de erros
- Python nao instalado: `OperationResult.Fail` com mensagem clara
- Processo morreu inesperadamente: tentar reiniciar automaticamente (1 tentativa)
- Timeout por request (ex: 5 segundos)
- Resposta JSON invalida: `OperationResult.Fail`

### Resolucao do caminho do script
- Resolver `tokenizer_service.py` relativo ao diretorio do assembly em execucao
- Alternativa: embutir como embedded resource
