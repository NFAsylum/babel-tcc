# Perguntas Frequentes (FAQ)

## Indice

- [Geral](#geral)
- [Funcionalidade](#funcionalidade)
- [Tecnico](#tecnico)
- [Contribuicao](#contribuicao)

## Geral

### O que e o Babel TCC?
Uma extensao VS Code que traduz codigo de programacao visualmente para o seu idioma natural. O ficheiro no disco permanece sempre na linguagem original.

### Que linguagens de programacao sao suportadas?
**C#** e **Python**. A arquitetura permite adicionar novas linguagens.

### Que idiomas naturais sao suportados?
10 idiomas: Portugues (Brasil), Portugues (ASCII), English, Espanol, Francais, Deutsch, Italiano, Nihongo (Romaji), Zhongwen, Arabiyyah.

### O codigo compilado e afetado?
Nao. O ficheiro no disco permanece sempre na linguagem original. Compiladores, linters, CI/CD e Git funcionam normalmente.

### Posso usar em projetos reais?
Sim. A extensao e segura para usar em projetos reais porque nao altera os ficheiros no disco.

## Funcionalidade

### Como traduzir nomes de variaveis/metodos?
Usar anotacoes `// tradu:NomeTraduzido` no codigo ou criar um ficheiro `.multilingual/identifier-map.json`.

### O autocomplete funciona com keywords traduzidas?
Sim. O CompletionProvider sugere keywords traduzidas ao digitar no painel traduzido.

### Posso ter multiplos idiomas no mesmo projeto?
A configuracao e por usuario/workspace. Cada desenvolvedor pode ter seu idioma configurado independentemente.

### O que acontece ao salvar?
O TranslatedContentProvider traduz automaticamente o codigo de volta para a linguagem original antes de gravar no disco.

### O que sao anotacoes "tradu"?
Comentarios no formato `// tradu:nomeTraduzido` que definem como identificadores devem ser traduzidos. Ver [Primeiros Passos](getting-started.md#usando-anotacoes-tradu).

## Tecnico

### Porque preciso do .NET 8.0?
O motor de traducao usa Roslyn para C# e um subprocesso Python para Python. O Host .NET coordena ambos.

### A extensao funciona offline?
Sim. Toda a traducao e feita localmente. Nenhuma conexao a internet e necessaria.

### Quanto espaco ocupa?
A extensao com binarios Core ocupa aproximadamente 16 MB devido a dependencia do Roslyn.

### Posso usar com outras extensoes C#?
Sim. A extensao funciona de forma independente e nao interfere com o OmniSharp ou outras extensoes C#.

## Contribuicao

### Como contribuir com novas traducoes?
Ver [CONTRIBUTING.md](../../CONTRIBUTING.md) e o [guia de traducoes](../developer-guide/creating-translations.md).

### Como adicionar suporte a nova linguagem de programacao?
Ver o [guia para desenvolvedores](../developer-guide/adding-new-language.md).

### Onde reportar bugs?
Abrir issue em https://github.com/NFAsylum/babel-tcc/issues com os detalhes descritos no [troubleshooting](troubleshooting.md#reportar-um-bug).
