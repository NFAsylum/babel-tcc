# Definition of Done - Tarefa 050

## Host C# (Program.cs)
- [ ] Main executa loop persistente lendo stdin linha por linha
- [ ] Cada linha e parseada como JSON com method e params
- [ ] Resposta escrita no stdout como uma linha JSON com success, result, error
- [ ] Estado mantido entre chamadas (translations carregadas, registry, adapters)
- [ ] Comando quit encerra o processo gracefully
- [ ] Erros em um request nao derrubam o processo

## CoreBridge TypeScript
- [ ] startProcess() inicia processo uma vez no activate
- [ ] invokeCore() envia JSON no stdin e aguarda resposta no stdout
- [ ] Fila serial: requests enfileirados, processados um por vez
- [ ] Timeout por request: rejeita promise, nao mata processo
- [ ] Timeout consecutivo (2x): mata processo e faz restart
- [ ] Crash recovery: detecta processo morto, reinicia automaticamente
- [ ] Crash repetido (3+): mostra warning ao usuario, para de tentar restart
- [ ] dispose() envia quit e mata o processo

## Experiencia do usuario
- [ ] Crash com restart bem-sucedido: delay de ~500ms, sem notificacao visivel
- [ ] Crash com restart falho: codigo original exibido como fallback, sem popup
- [ ] Crashes repetidos: warning visivel via showWarningMessage
- [ ] Timeout: codigo original exibido como fallback, sem popup
- [ ] Detalhes sempre disponiveis no output channel para debug

## Testes
- [ ] Testes de coreBridge.test.ts reescritos para modelo persistente
- [ ] npm test passa com todos os testes (sem regressao nos outros 102 testes)

### Cenarios reescritos (modelo persistente)
- [ ] invokeCore sucesso: envia JSON no stdin, recebe resposta no stdout
- [ ] invokeCore output vazio: rejeita
- [ ] invokeCore response.success false: rejeita
- [ ] invokeCore timeout: rejeita promise, processo continua vivo
- [ ] invokeCore process error: rejeita
- [ ] resolveTranslationsPath: 3 niveis de fallback
- [ ] translateToNaturalLanguage/translateFromNaturalLanguage: params corretos
- [ ] validateSyntax: parse do resultado
- [ ] getSupportedLanguages: parse do array
- [ ] JSON invalido no stdout: lanca erro

### Cenarios novos (processo persistente)
- [ ] startProcess inicia processo uma vez
- [ ] Segundo request reutiliza mesmo processo (spawn chamado 1x)
- [ ] Fila serial: segundo request espera primeiro terminar
- [ ] Crash recovery: processo morre, proximo request reinicia
- [ ] Crash repetido (3+): showWarningMessage, para de reiniciar
- [ ] Timeout nao mata processo: proximo request funciona normalmente
- [ ] Timeout consecutivo (2x): mata processo e reinicia
- [ ] dispose envia quit no stdin
- [ ] dispose mata processo se quit nao responder

## Verificacao
- [ ] Traducao funciona no VS Code apos mudanca
- [ ] Performance visivelmente melhor (sem delay perceptivel ao trocar tabs)
- [ ] Processo .NET visivel no task manager enquanto extensao ativa
- [ ] Processo encerra quando extensao desativa
