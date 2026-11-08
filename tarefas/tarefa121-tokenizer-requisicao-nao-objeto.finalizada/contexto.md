# Contexto - Tarefa 121

## Dependencias
Nenhuma.

## Bloqueia
Nada.

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py
- packages/core/MultiLingualCode.Core.Tests/LanguageAdapters/PythonTokenizerServiceTests.cs
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/PythonTokenizerService.cs
  (o cliente; nao muda)
- tarefas/tarefa052-script-python-tokenizador.finalizada/especificacao.md (regra original)
- docs/padroes-codigo.md, secao Python

## Como reproduzir o defeito (antes da correcao)

```bash
printf '%s\n' '[]' '{"source":"a=1"}' | \
  python3 -B packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py
echo "exit=$?"
```

Antes: nenhuma linha no stdout, traceback com `AttributeError` no stderr, `exit=1`.
Depois: duas linhas no stdout (o erro e a tokenizacao de `a=1`), stderr vazio, `exit=0`.

## Notas
- Usar `python3 -B` ao rodar o script na arvore de fontes. Sem o `-B`, um `__pycache__/` ao
  lado do script faz `BuildDeployTests.Build_AllNonCSharpSourceFiles_CopiedToOutputDirectory`
  falhar, e o diretorio e ignorado pelo git.
- Os testes com `[RequiresPythonFact]` sao pulados se nao houver Python 3.8+ na maquina.
  Conferir na saida do `dotnet test` que o teste novo rodou.
