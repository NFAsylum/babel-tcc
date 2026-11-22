# Tarefa 126 - Limite de reinicios do motor de traducao

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Fazer o limite de quedas do processo do Core (`MAX_CRASHES = 3`) funcionar: um motor que cai sempre
deve parar de ser reiniciado e o usuario deve ser avisado, uma vez.

## Contexto
Registrado como fora de escopo na tarefa117. O `CoreBridge` conta quedas do processo (`crashCount`,
incrementado nos handlers de `exit` e `error`) e o `ensureProcess` recusa reiniciar a partir de 3. Mas o
`startProcess` zerava o `crashCount` a cada inicio: cada queda levava o contador a 1, o reinicio o
voltava a 0, e o limite nunca era atingido. Um motor que cai ao iniciar (instalacao danificada, .NET
quebrado) era reiniciado a cada requisicao, sem fim e sem nenhuma mensagem.

O teste existente nao pegava o defeito porque forcava o contador na mao (`crashCount = 3`) em vez de
provocar quedas.

Corrigir so a linha do `startProcess` trocaria o reinicio infinito por um aviso a cada requisicao:
depois do limite, toda traducao pediria o processo de novo e mostraria a mensagem. Foi por isso que a
tarefa117 deixou a correcao para uma tarefa propria.

## Decisao
- **O contador zera quando o motor responde**, nao quando ele inicia. Assim o limite conta quedas
  seguidas: um motor que funciona e cai de vez em quando continua sendo reiniciado. "Responder" e
  mandar uma resposta valida do protocolo (JSON), mesmo de erro. A primeira versao zerava com qualquer
  linha, e o F5 mostrou o furo: sem a DLL do Core, o `dotnet` imprime "Nao foi possivel executar..."
  em texto no `stdout` antes de sair, essa linha zerava o contador e o motor foi reiniciado 6 vezes
  sem parar. Agora o contador so zera depois que a linha e lida como JSON.
- **No limite, o aviso aparece uma vez** (`instabilityReported`), com o botao "Abrir Log" (frase que ja
  existia nos bundles, da tarefa115). As requisicoes seguintes sao recusadas sem repetir o aviso.
- **Sem `throw`**: o `ensureProcess` passa a devolver se ha processo, e o `sendRequest` recusa a
  requisicao como valor. Os padroes do projeto proibem `throw`. O metodo tambem passa a `public`, como
  pedem os padroes, e o teste o usa diretamente.
- Depois do limite, a extensao so volta a usar o motor ao recarregar a janela. Isso evita um laco de
  reinicios; reiniciar sob demanda (por exemplo, ao alternar a traducao) fica como melhoria possivel.

## Escopo
- `src/services/coreBridge.ts`: `startProcess`, `ensureProcess`, `warnEngineUnstable`,
  `instabilityReported` e o zerar do contador na resposta.
- `test/services/coreBridge.test.ts`: o teste antigo do limite (contador forcado) trocado por 4 testes
  com quedas de verdade.
- `CHANGELOG.md` (Fixed).

## Fora de escopo
- A requisicao em andamento quando o processo cai so e recusada no timeout (10 s); responder na hora
  exigiria rejeitar a requisicao pendente no `exit`. Comportamento anterior, mantido.
