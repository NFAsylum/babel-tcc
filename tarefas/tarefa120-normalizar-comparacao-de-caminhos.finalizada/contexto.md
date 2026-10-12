# Contexto - Tarefa 120

## Dependencias
Nenhuma. A correcao e independente da etapa de UX e pode sair antes ou depois dos PRs 2 e 3.

## Bloqueia
Nada formalmente. Mas convem sair cedo: o defeito atinge o mecanismo de recarga do DT-011
(`reloadTranslatedView`), entao qualquer tarefa que mexa em troca de idioma ou em ciclo de vida de
aba herda o comportamento intermitente enquanto isto nao for corrigido.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/providers/autoTranslateManager.ts (linhas 66, 378, 437)
- packages/ide-adapters/vscode/src/providers/translatedContentProvider.ts (linhas 250, 257)
- packages/ide-adapters/vscode/src/services/ (destino sugerido do modulo novo)
- packages/ide-adapters/vscode/test/providers/autoTranslateManager.test.ts
- packages/ide-adapters/vscode/test/__mocks__/vscode.ts (o mock de `Uri` preserva a caixa como
  recebida, entao da para escrever o teste de regressao sem adaptacao)
- docs/decisoes-tecnicas.md (DT-011, que descreve o mecanismo afetado)

## Como reproduzir

1. `code --extensionDevelopmentPath=<...>/packages/ide-adapters/vscode <...>/examples`
2. Abrir `examples/csharp/Calculator/Calculator.cs`. A visao traduzida abre e substitui o original.
3. Com o cursor dentro do codigo, `Ctrl+Alt+B` `O` (ou botao direito > "Mostrar Codigo Original").
4. Painel Saida, canal "Babel TCC".

Sintoma: a tela pisca e volta ao traduzido, e o log mostra o par

```
Showing original for: /c:/Users/...
AutoTranslate: replaced c:\Users\... with translated view
```

**A reproducao e intermitente por natureza** — depende da caixa que o VS Code atribuiu ao documento
naquela sessao. Se nao reproduzir de primeira, feche a janela, reabra e tente de novo. Nas nossas
medicoes reproduziu numa execucao e nao reproduziu na seguinte, com os mesmos passos.

## Notas
- Diagnostico feito instrumentando `dist/extension.js` (saida de build, ignorada pelo git) em vez de
  mexer no `src/`, para nao sujar a arvore de trabalho durante um PR aberto. `npm run build` desfaz.
- `closeTab` compara `uri.toString()` e a trava compara `uri.path` — duas formas diferentes de
  comparar a mesma coisa no mesmo arquivo. A correcao deve unificar isso, nao so consertar a caixa.
- Cuidado ao escolher a normalizacao: baixar o caminho inteiro para minusculo **quebra Linux**, onde
  caminhos sao sensiveis a caixa de verdade. So a letra do drive deve ser normalizada.
- Ambiente: a maquina de desenvolvimento tem pouca RAM; rodar a suite com
  `npx vitest run --no-file-parallelism` em vez de `npm test`.
