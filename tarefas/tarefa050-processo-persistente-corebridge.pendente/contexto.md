# Contexto - Tarefa 050

## Dependencias
- Nenhuma. A tarefa 049 (thread safety) NAO e necessaria — decidimos por fila serial.

## Bloqueia
- Nenhuma diretamente (melhoria de performance)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core.Host/Program.cs (reescrever Main para loop persistente)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (reescrever invokeCore para stdin/stdout)
- packages/ide-adapters/vscode/test/services/coreBridge.test.ts (reescrever testes)

## Decisoes tomadas

### 1. Fila serial (NAO paralelo)
O VS Code so traduz a tab ativa. Mesmo ao ligar traducao com multiplas tabs
abertas, sao tipicamente 3-5 arquivos sequenciais. Cada traducao leva ~5-50ms.
Serial: ~250ms total. Imperceptivel para o usuario.

Paralelismo adicionaria complexidade (request IDs, race conditions,
ConcurrentDictionary) sem ganho perceptivel. Descartado.

### 2. Protocolo JSON Lines (uma linha por request/response)
- Request: `{"method": "...", "params": {...}}\n`
- Response: `{"success": true, "result": "...", "error": ""}\n`
- Sem IDs — fila serial garante que a proxima resposta e sempre do request atual
- `{"method": "quit"}\n` encerra o processo

### 3. Lifecycle do processo
- Inicia no activate() da extensao
- Encerra no dispose() do CoreBridge

### 4. Crash recovery
Se o processo .NET morrer (bug, out of memory, kill externo):
- O proximo invokeCore() detecta que o processo nao esta vivo
- Spawna um novo processo automaticamente
- Executa o request normalmente
- Log no output channel: "CoreBridge: processo reiniciado apos crash"

Experiencia do usuario:
- Restart bem-sucedido: delay de ~500ms naquele request. Nenhuma notificacao
  visivel — o usuario ve a traducao aparecer um pouco mais lenta, so isso.
  Nao faz sentido alarmar o usuario por algo que se resolveu sozinho.
- Restart falha (ex: DLL corrompida, .NET nao instalado): a traducao falha.
  TranslatedContentProvider mostra o codigo original (sem traducao) como
  fallback. StatusBar continua mostrando o idioma ativo. O output channel
  registra o erro detalhado para debug.
- Falhas repetidas (3+ crashes seguidos): mostrar warning ao usuario via
  vscode.window.showWarningMessage com mensagem como "Babel TCC: o motor
  de traducao esta instavel. Verifique o painel Output para detalhes."
  Evita restart infinito.

### 5. Timeout por request
Se um request nao responde em X segundos (default 10s):
- A promise daquele request e rejeitada
- O processo NAO e morto — pode estar apenas lento, nao travado
- TranslatedContentProvider captura o erro e mostra o codigo original
  (sem traducao) como fallback
- Log no output channel: "CoreBridge: timeout apos 10s para TranslateToNaturalLanguage"

Experiencia do usuario:
- A tab traduzida mostra o codigo original (sem traducao) para aquele arquivo
- Nenhum popup de erro — seria intrusivo para algo que pode ser transitorio
- Se o proximo request tambem der timeout, o processo provavelmente esta
  travado. Nesse caso, mata o processo e faz restart (mesma logica do
  crash recovery)
- O usuario pode verificar o output channel se quiser entender o que aconteceu

### 6. Impacto em testes
- Os 17 testes de coreBridge.test.ts precisam ser reescritos
- Mock muda de spawn-per-request para processo persistente com stdin/stdout streams
- Total de testes deve se manter ou aumentar

## Notas
- O custo fixo de RAM (~40MB) e aceitavel — hoje paga-se esse custo a cada request
  e libera. Com processo persistente, paga uma vez e fica.
- Nao depende da tarefa 049 (thread safety) pois nao usaremos paralelismo.
- Prioridade foi BAIXA (pos-v1.0) mas o ganho de UX justifica implementacao agora.
