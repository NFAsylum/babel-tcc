# Perguntas Frequentes (FAQ)

## Índice

- [Geral](#geral)
- [Funcionalidade](#funcionalidade)
- [Técnico](#técnico)
- [Contribuição](#contribuição)

## Geral

### O que é o Babel TCC?
Uma extensão VS Code que traduz código de programação visualmente para o seu idioma natural. O ficheiro no disco permanece sempre na linguagem original.

### Que linguagens de programação são suportadas?
**C#**, **Python**, **VisuAlg** (`.alg`) e **Portugol Studio** (`.por`). A arquitetura permite adicionar novas linguagens.

### Que idiomas naturais são suportados?
10 idiomas: Português (Brasil), Português (ASCII), English, Español, Français, Deutsch, Italiano, Nihongo (Romaji), Zhongwen, Arabiyyah.

### O código compilado é afetado?
Não. O ficheiro no disco permanece sempre na linguagem original. Compiladores, linters, CI/CD e Git funcionam normalmente.

### Posso usar em projetos reais?
Sim. A extensão é segura para usar em projetos reais porque não altera os ficheiros no disco.

## Funcionalidade

### Como traduzir nomes de variáveis/métodos?
Usar anotações `// tradu[lang]:NomeTraduzido` no código (ex.: `// tradu[pt-br]:Calculadora`). É o único mecanismo de tradução de identificadores; não há arquivo de mapa persistente.

### O autocomplete funciona com keywords traduzidas?
Sim. O autocomplete sugere keywords traduzidas ao digitar na visualização traduzida.

### Posso ter múltiplos idiomas no mesmo projeto?
Sim. A configuração é por usuário/workspace, e `babel-tcc.languageOverrides` permite um idioma alvo por linguagem de programação. Cada desenvolvedor pode ter seu idioma configurado independentemente.

### O que acontece ao salvar?
Ao salvar, a extensão traduz automaticamente o código de volta para a linguagem original antes de gravar no disco.

### O que são anotações "tradu"?
Comentários no formato `// tradu[lang]:nomeTraduzido` que definem como identificadores devem ser traduzidos. Ver [Primeiros Passos](getting-started.md#usando-anotações-tradu).

## Técnico

### Porque preciso do .NET 8.0?
O motor de tradução é escrito em C# e roda sobre o .NET 8. Ele usa Roslyn para C# e um subprocesso Python para arquivos `.py`; o Host .NET coordena ambos.

### A extensão funciona offline?
Sim. Toda a tradução é feita localmente. Nenhuma conexão à internet é necessária.

### Quanto espaço ocupa?
A extensão com os binários do Core ocupa aproximadamente 16 MB, devido à dependência do Roslyn.

### Posso usar com outras extensões C#?
Sim. A extensão funciona de forma independente e não interfere com o OmniSharp ou outras extensões C#.

## Contribuição

### Como contribuir com novas traduções?
Ver [CONTRIBUTING.md](../../CONTRIBUTING.md) e o [guia de traduções](../developer-guide/creating-translations.md).

### Como adicionar suporte a nova linguagem de programação?
Ver o [guia para desenvolvedores](../developer-guide/adding-new-language.md).

### Onde reportar bugs?
Abrir issue em https://github.com/NFAsylum/babel-tcc/issues com os detalhes descritos no [troubleshooting](troubleshooting.md#reportar-um-bug).
