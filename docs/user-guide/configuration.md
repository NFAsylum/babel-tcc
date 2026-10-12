# Configuração

## Índice

- [Settings disponíveis](#settings-disponíveis)
- [Workspace vs Global](#workspace-vs-global)
- [Idioma padrão](#idioma-padrão)
- [Idiomas disponíveis](#idiomas-disponíveis)
- [Mapeamento customizado](#mapeamento-customizado)

## Settings disponíveis

| Setting | Tipo | Padrão | Descrição |
|---------|------|--------|-----------|
| `babel-tcc.enabled` | boolean | `true` | Ativar/desativar tradução em tempo real |
| `babel-tcc.language` | lista de idiomas | `"pt-br"` | Idioma natural alvo para a tradução. Escolhido numa lista — ver [Idiomas disponíveis](#idiomas-disponíveis) |
| `babel-tcc.languageOverrides` | object | `{}` | Idioma alvo por linguagem de programação (ex.: `{"CSharp": "pt-br", "Python": "es-es"}`), cada um escolhido na mesma lista. Usa `babel-tcc.language` quando não há override. |
| `babel-tcc.readonly` | boolean | `false` | Abrir visualizações traduzidas em modo somente leitura (impede edições acidentais no arquivo original) |
| `babel-tcc.translationsPath` | string | `""` | Caminho absoluto para o repositório babel-tcc-translations. Se vazio, auto-detecta como pasta irmã do workspace ou usa traduções embarcadas. |

Na tela de Configurações (`Ctrl+,` e buscar por `babel-tcc`), as cinco aparecem nessa ordem — das mais usadas para a mais avançada.

### Exemplo settings.json

```json
{
  "babel-tcc.enabled": true,
  "babel-tcc.language": "pt-br",
  "babel-tcc.translationsPath": "",
  "babel-tcc.readonly": false,
  "babel-tcc.languageOverrides": {}
}
```

## Workspace vs Global

- **Global:** Aplicado a todos os projetos. Usar `Ctrl+Shift+P` > `Preferences: Open User Settings`
- **Workspace:** Aplicado apenas ao projeto atual. Criar `.vscode/settings.json` na raiz do projeto

Recomendação: usar workspace settings para definir idioma por projeto.

## Idioma padrão

O idioma padrão é `pt-br` (Português Brasileiro). Para mudar:

1. `Ctrl+Shift+P` > `Babel TCC: Selecionar Idioma` (ou o atalho `Ctrl+Alt+U` `I`, ou clicar no idioma na barra de status)
2. Escolher o escopo: global ou por linguagem de programação (override)
3. Selecionar o idioma desejado

A mudança é gravada na configuração **global** do usuário — em `babel-tcc.language` (escopo global) ou em `babel-tcc.languageOverrides` (escopo por linguagem). No escopo global, se houver overrides ativos, a extensão avisa que eles bloqueiam a troca e oferece removê-los.

Também é possível escolher direto na tela de Configurações: `babel-tcc.language` é uma lista suspensa, não uma caixa de texto.

## Idiomas disponíveis

Os idiomas aparecem na lista com o nome escrito na própria grafia, e cada um tem uma descrição. O valor gravado no `settings.json` é o código da primeira coluna.

| Código | Nome na lista |
|---|---|
| `pt-br` | Português (Brasil) |
| `pt-br-ascii` | Português (Brasil, sem acentos) |
| `en-us` | English (US) |
| `es-es` | Español |
| `fr-fr` | Français |
| `de-de` | Deutsch |
| `it-it` | Italiano |
| `ja-jp-romaji` | 日本語 (romaji) |
| `zh-cn` | 中文 (简体) |
| `ar-sa` | العربية |

**Fork do repositório de traduções com um idioma a mais:** quem aponta `babel-tcc.translationsPath` para um fork que acrescenta um idioma pode escrever o código dele no `settings.json` normalmente. O VS Code sublinha o valor com um aviso, porque ele não está na lista, mas o valor é aplicado mesmo assim (DT-013).

## Mapeamento customizado

Para traduzir identificadores (nomes de classes, métodos, variáveis), use anotações `// tradu[lang]:` diretamente no código — esse é o mecanismo de mapeamento de identificadores (ver [Primeiros Passos](getting-started.md#usando-anotações-tradu)):

```csharp
public class Calculator // tradu[pt-br]:Calculadora
public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
```

As anotações são lidas a cada tradução e valem para todas as ocorrências do identificador no arquivo. Não há arquivo de mapa global ou persistente: a extensão não mantém um `identifier-map.json` entre sessões (o diretório `.multilingual` é limpo na ativação).
