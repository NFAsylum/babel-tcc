# Definition of Done - Tarefa 114

## Manifesto
- [x] `babel-tcc.language` com `enum` dos 10 locales
- [x] `enumItemLabels` com o nome de cada idioma na grafia nativa
- [x] `enumDescriptions` apontando para chaves nls
- [x] As 5 propriedades de `languageOverrides` com o mesmo `enum` e os mesmos rotulos
- [x] `order` nas 5 propriedades de configuracao (enabled 1, language 2, languageOverrides 3,
      readonly 4, translationsPath 5)

## i18n
- [x] Chaves de `enumDescriptions` nos 7 `package.nls*.json`
- [x] Nenhuma chave orfa e nenhuma faltando (o teste de contrato da tarefa113 cobre)

## Testes
- [x] `manifest.test.ts`: o `enum` de `babel-tcc.language` e exatamente o conjunto de pastas em
      `natural-languages`
- [x] O teste do enum roda no CI, sem skip: le `BABEL_TCC_TRANSLATIONS_PATH`, e o job da extensao faz
      checkout do repositorio de traducoes
- [x] Teste de mutacao: com `BABEL_TCC_TRANSLATIONS_PATH` apontando para uma pasta com 1 locale, o
      teste falha
- [x] `manifest.test.ts`: `enum`, `enumItemLabels` e `enumDescriptions` tem o mesmo comprimento
- [x] `manifest.test.ts`: as 5 propriedades de `languageOverrides` usam o mesmo enum da propriedade
      global
- [x] `manifest.test.ts`: toda propriedade de configuracao tem `order`, e os valores nao se repetem
- [x] Suite verde (`npx vitest run --no-file-parallelism`) - 259 passam, junto com a tarefa113
- [x] `npm run lint` limpo e `npm run build` passando

## Validacao manual (F5)
- [x] Configuracoes > Babel TCC mostra **dropdown** no idioma, nao caixa de texto
- [x] Cada opcao aparece com o nome legivel do idioma, nao so o codigo
- [x] As 5 propriedades aparecem na ordem declarada, nao em ordem alfabetica
- [x] Trocar o idioma pelo dropdown re-traduz as visoes abertas (nao regrediu o DT-011)
- [x] O comando "Selecionar Idioma" continua funcionando como antes

## Processo
- [x] Commits em conventional commits com escopo `vscode`
- [x] Pasta da tarefa renomeada para `.finalizada`
- [x] `CHANGELOG.md` atualizado
- [x] DT-013 registrado em `docs/decisoes-tecnicas.md` (lista fechada e o trade-off do fork)
- [x] `docs/user-guide/configuration.md` descreve `babel-tcc.language` como lista
