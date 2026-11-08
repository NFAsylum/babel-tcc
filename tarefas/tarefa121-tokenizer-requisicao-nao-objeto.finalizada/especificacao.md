# Tarefa 121 - Tokenizer Python: requisicao JSON que nao e objeto encerra o processo

## Fase
Correcao de defeito (encontrado durante a verificacao da tarefa112)

## Objetivo
Fazer o tokenizer Python responder com erro, e continuar vivo, quando recebe uma linha que e
JSON valido mas nao e um objeto.

## Contexto
`tokenizer_service.py` le uma requisicao por linha. JSON invalido ja era tratado: a linha
gera `{"ok": false, "error": "Invalid JSON: ..."}` e o laco continua. Mas `json.loads` aceita
qualquer valor JSON, nao so objeto. Uma linha `[]`, `5`, `"texto"`, `null` ou `true` passava
pelo `try` e chegava a `request.get("cmd")`, que levanta `AttributeError` fora de qualquer
`try`. O processo encerrava com codigo 1 e traceback no stderr, sem responder.

Medido na `main` (commit `98234e6`), para cada uma das cinco entradas acima seguida de uma
requisicao valida: codigo de saida 1, nenhuma resposta no stdout, a requisicao valida nunca
foi atendida.

Isso contraria duas regras ja escritas:

- a especificacao original do script (tarefa052): "Excecoes genericas nao devem matar o
  processo - capturar e retornar erro no JSON";
- docs/padroes-codigo.md, secao Python: "Nenhuma excecao pode escapar do laco de `main()`".

### Alcance
O cliente do projeto, `PythonTokenizerService`, sempre serializa um objeto
(`new { source = sourceCode }`), entao o defeito nao e alcancavel pelo produto hoje. E um
defeito de robustez do protocolo: qualquer outro cliente, ou uma mudanca futura no formato da
requisicao, derrubaria o interpretador em vez de receber um erro.

## Decisao
Checar o tipo logo depois do `json.loads`. Se o valor nao for objeto, responder
`{"ok": false, "error": "Invalid request: expected a JSON object"}` e seguir para a proxima
linha, do mesmo jeito que o JSON invalido ja faz.

O envelope de erro nao carrega `tokens`, igual ao do JSON invalido: nao houve tokenizacao.

## Escopo
- `tokenizer_service.py`: a checagem de tipo.
- `PythonTokenizerServiceTests.cs`: teste de regressao que escreve as cinco linhas direto no
  stdin do subprocesso e confere a resposta de erro, e depois que o mesmo processo ainda
  tokeniza.

## Fora de escopo
- Validar o tipo dos campos de um objeto. `{"source": 5}` ja responde com erro, pelo `except`
  generico em volta de `tokenize_source`.
