# Tarefa 114 - Configuracoes navegaveis

## Fase
7 - UX, Interface e Acessibilidade (VS Code)

## Objetivo
Ninguem deveria precisar saber que a string e exatamente `ja-jp-romaji`. Trocar as configuracoes de
idioma de string livre para lista navegavel, com rotulo legivel em cada opcao, e ordenar as cinco
propriedades por frequencia de uso.

## Contexto
Hoje `babel-tcc.language` e `"type": "string"` sem restricao: a interface de Configuracoes do VS Code
desenha uma caixa de texto vazia e o usuario precisa **digitar** o codigo do locale na mao. Para
acertar `ja-jp-romaji` ou `pt-br-ascii` e preciso ter lido a documentacao antes - exatamente a
barreira que esta etapa existe para derrubar. O mesmo vale para as cinco propriedades de
`babel-tcc.languageOverrides`.

As cinco propriedades tambem aparecem em ordem alfabetica, porque nenhuma declara `order`. O
resultado e que `translationsPath`, que e a opcao mais avancada e que quase ninguem toca, aparece
antes de `readonly`.

## Decisao
`enum` com `enumItemLabels` (rotulo legivel, no proprio idioma) e `enumDescriptions` (explicacao
localizada via nls). O VS Code passa a desenhar um dropdown em vez de caixa de texto.

Os rotulos de `enumItemLabels` ficam **no idioma nativo de cada locale** (`Portugues (Brasil)`,
`Deutsch`, `日本語 (romaji)`), nao traduzidos para o idioma da interface. E a convencao de todo
seletor de idioma: quem procura o proprio idioma reconhece o nome dele escrito como ele escreve.
Por isso esses rotulos **nao** entram nos arquivos nls - sao identicos nos 7 idiomas de interface.

`order` nas cinco propriedades, por frequencia de uso decrescente: `enabled` 1, `language` 2,
`languageOverrides` 3, `readonly` 4, `translationsPath` 5.

### Trade-off aceito e documentado

O `enum` **desliga a digitacao livre de um locale novo**. Como as traducoes sao empacotadas dentro do
`.vsix`, isso e correto no caso normal: so existem os 10 locales embarcados. Quem aponta
`babel-tcc.translationsPath` para um fork do repositorio de traducoes com um locale a mais vera um
aviso de validacao amarelo no `settings.json` - mas **o valor ainda e aplicado**, porque o VS Code
nao bloqueia valor fora do enum, apenas sinaliza. Cenario raro, degradacao suave, ganho grande para o
caso comum. Aceito, e registrado como decisao arquitetural no DT-013.

### Conformidade com docs/padroes-codigo.md
Esta tarefa e 100% manifesto e nls; nao ha codigo TypeScript de producao novo. A regra que se aplica
e a de **nenhuma string literal na UI**: toda descricao nova entra nos 7 `package.nls*.json`.

## Escopo
- `package.json`: `enum`, `enumItemLabels` e `enumDescriptions` em `babel-tcc.language` e nas 5
  propriedades de `babel-tcc.languageOverrides`; `order` nas 5 propriedades de configuracao.
- `package.nls.json` + 6 traducoes: as chaves de `enumDescriptions`.
- `test/manifest/manifest.test.ts`: o `enum` e **exatamente** o conjunto de pastas em
  `natural-languages`, lidas de `BABEL_TCC_TRANSLATIONS_PATH`, da copia `translations/` gerada pelo
  build ou do repositorio irmao de um checkout de desenvolvimento. Sem skip: se nenhuma fonte existe,
  o teste falha com a instrucao de configurar a variavel.
- `.github/workflows/ci.yml`: o job da extensao faz checkout do repositorio de traducoes e aponta
  `BABEL_TCC_TRANSLATIONS_PATH` para ele, como o job do Core ja fazia - para o teste rodar em todo PR.
- `docs/user-guide/configuration.md`: `babel-tcc.language` como lista, a tabela de idiomas e o caso do
  fork com idioma a mais.
- `docs/decisoes-tecnicas.md`: DT-013.

## Fora de escopo
- Qualquer mudanca no codigo que le as configuracoes. `ConfigurationService` continua recebendo
  string e nao precisa saber que agora ela vem de uma lista.
- `markdownDescription` com links: avaliado e descartado por ora - nenhuma das cinco propriedades tem
  link util a oferecer que a descricao ja nao cubra.

## Regra de ouro estendida a camada de UI
O `enum` precisa ser identico ao conjunto de pastas em `translations/natural-languages`. Se alguem
adicionar um idioma no repositorio de traducoes e esquecer o manifesto, o teste de contrato quebra.
E o mesmo principio do `scripts/validate.py`, agora aplicado a uma lista que mora no manifesto.

## Revisao do PR #185 (2026-10-12)
Na primeira versao o teste do enum usava `it.skipIf` e so rodava com o repositorio de traducoes ao
lado. O job da extensao no CI nao fazia checkout dele, entao o teste era pulado no CI e em qualquer
clone limpo - o item do DoD estava marcado como cumprido, mas nao era exercido onde importa. Agora o
teste le `BABEL_TCC_TRANSLATIONS_PATH`, o CI fornece o repositorio e o skip saiu.
