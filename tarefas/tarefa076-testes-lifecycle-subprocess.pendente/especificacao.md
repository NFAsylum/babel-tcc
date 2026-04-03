# Tarefa 076 - Testes de lifecycle e cleanup de subprocessos

## Prioridade: MEDIUM

## Problema
PythonAdapter cria PythonTokenizerService que spawna um subprocesso Python. Se o adapter for recriado (ex: crash recovery no CoreBridge, ou futuro Host persistente), subprocessos antigos podem ficar orfaos. O LanguageRegistry nao chama Dispose nos adapters registrados.

## Escopo

### 1. Testes de lifecycle
- Criar PythonAdapter, chamar Parse, Dispose — verificar que processo Python encerrou (Process.HasExited)
- Criar PythonAdapter, NAO chamar Dispose — verificar que processo fica vivo (leak detectavel)
- Criar 3 PythonAdapters sequencialmente com Dispose entre eles — verificar que nao acumulam processos
- Tokenize apos Dispose retorna erro (ja testado, mas verificar que nao spawna novo processo)

### 2. Teste de crash recovery
- Simular crash do processo Python (Kill) e verificar que o proximo Tokenize tenta restart
- Verificar que restart nao deixa processo orfao

### 3. Avaliar LanguageRegistry Dispose
- LanguageRegistry armazena adapters mas nao implementa IDisposable
- Se adapters implementam IDisposable, o registry deveria propagar Dispose
- Avaliar se LanguageRegistry deve implementar IDisposable e iterar adapters
