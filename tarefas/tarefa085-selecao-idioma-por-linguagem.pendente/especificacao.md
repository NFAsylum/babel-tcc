# Tarefa 085 - Selecao de idioma por linguagem de programacao

## Prioridade: HIGH

## Problema
A configuracao `babel-tcc.language` e global — aplica o mesmo idioma para
todas as linguagens de programacao. Um desenvolvedor que queira usar
`pt-br-short` para Python (keywords curtas) e `pt-br` para C# nao tem como.

As tabelas de traducao representam nao apenas um idioma mas uma preferencia
do desenvolvedor. Tabelas diferentes podem ter estilos diferentes (verboso,
compacto, formal) e faz sentido permitir escolhas diferentes por linguagem.

## Comportamento atual
```json
{
  "babel-tcc.language": "pt-br"
}
```
Aplica "pt-br" para .cs, .py e qualquer linguagem futura.

## Comportamento desejado
```json
{
  "babel-tcc.language": "pt-br",
  "babel-tcc.languageOverrides": {
    "Python": "pt-br-short",
    "CSharp": "pt-br"
  }
}
```
- `babel-tcc.language` continua como default global (retrocompativel)
- `babel-tcc.languageOverrides` permite override por linguagem de programacao
- Se nao houver override, usa o default global

## Escopo

### VS Code Extension (TypeScript)
- Adicionar configuracao `babel-tcc.languageOverrides` no package.json
- ConfigurationService: novo metodo `getLanguageForProgrammingLanguage(progLang)`
- TranslatedContentProvider: usar o metodo novo em vez de `getLanguage()`
- KeywordMapService: idem
- StatusBar: mostrar idioma da linguagem ativa (nao o global)
- Comando selectLanguage: permitir selecionar por linguagem ou global

### Core Host (C#)
- Nenhuma mudanca necessaria — o Core ja recebe o idioma por request.
  A extensao e responsavel por enviar o idioma correto.

### Testes
- ConfigurationService: testar override por linguagem
- TranslatedContentProvider: testar que usa idioma correto por extensao
- KeywordMapService: testar que carrega tabela correta por linguagem
