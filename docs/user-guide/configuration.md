# Configuração

## Índice

- [Settings disponíveis](#settings-disponíveis)
- [Workspace vs Global](#workspace-vs-global)
- [Idioma padrão](#idioma-padrão)
- [Mapeamento customizado](#mapeamento-customizado)

## Settings disponíveis

| Setting | Tipo | Padrão | Descrição |
|---------|------|--------|-----------|
| `babel-tcc.enabled` | boolean | `true` | Ativar/desativar tradução em tempo real |
| `babel-tcc.language` | string | `"pt-br"` | Idioma natural alvo para a tradução |
| `babel-tcc.translationsPath` | string | `""` | Caminho absoluto para o repositório babel-tcc-translations. Se vazio, auto-detecta como pasta irmã do workspace ou usa traduções embarcadas. |
| `babel-tcc.readonly` | boolean | `false` | Abrir visualizações traduzidas em modo somente leitura (impede edições acidentais no arquivo original) |
| `babel-tcc.languageOverrides` | object | `{}` | Idioma alvo por linguagem de programação (ex.: `{"CSharp": "pt-br", "Python": "es-es"}`). Usa `babel-tcc.language` quando não há override. |

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

1. `Ctrl+Shift+P` > `Babel TCC: Selecionar Idioma`
2. Escolher o escopo: global ou por linguagem de programação (override)
3. Selecionar o idioma desejado

A mudança é gravada na configuração **global** do usuário — em `babel-tcc.language` (escopo global) ou em `babel-tcc.languageOverrides` (escopo por linguagem). No escopo global, se houver overrides ativos, a extensão avisa que eles bloqueiam a troca e oferece removê-los.

## Mapeamento customizado

Para traduzir identificadores (nomes de classes, métodos, variáveis), use anotações `// tradu[lang]:` diretamente no código — esse é o mecanismo de mapeamento de identificadores (ver [Primeiros Passos](getting-started.md#usando-anotações-tradu)):

```csharp
public class Calculator // tradu[pt-br]:Calculadora
public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo
```

As anotações são lidas a cada tradução e valem para todas as ocorrências do identificador no arquivo. Não há arquivo de mapa global ou persistente: a extensão não mantém um `identifier-map.json` entre sessões (o diretório `.multilingual` é limpo na ativação).
