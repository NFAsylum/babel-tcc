# Configuracao

## Indice

- [Settings disponiveis](#settings-disponiveis)
- [Workspace vs Global](#workspace-vs-global)
- [Idioma padrao](#idioma-padrao)
- [Mapeamento customizado](#mapeamento-customizado)

## Settings disponiveis

| Setting | Tipo | Padrao | Descricao |
|---------|------|--------|-----------|
| `babel-tcc.enabled` | boolean | `true` | Ativar/desativar traducao |
| `babel-tcc.language` | string | `"pt-br"` | Idioma alvo para traducao |
| `babel-tcc.translationsPath` | string | `""` | Caminho absoluto para o repositorio babel-tcc-translations. Se vazio, auto-detecta como pasta irmã do workspace ou usa traduções embarcadas. |
| `babel-tcc.readonly` | boolean | `false` | Abrir views traduzidas em modo readonly (impede edições acidentais no arquivo original) |

### Exemplo settings.json

```json
{
  "babel-tcc.enabled": true,
  "babel-tcc.language": "pt-br",
  "babel-tcc.translationsPath": "",
  "babel-tcc.readonly": false
}
```

## Workspace vs Global

- **Global:** Aplicado a todos os projetos. Usar `Ctrl+Shift+P` > `Preferences: Open User Settings`
- **Workspace:** Aplicado apenas ao projeto atual. Criar `.vscode/settings.json` na raiz do projeto

Recomendacao: usar workspace settings para definir idioma por projeto.

## Idioma padrao

O idioma padrao e `pt-br` (Portugues Brasileiro). Para mudar:

1. `Ctrl+Shift+P` > `Babel TCC: Select Language`
2. Selecionar o idioma desejado
3. A selecao persiste na configuracao global

## Mapeamento customizado

Para projetos que usam identificadores em ingles, criar um ficheiro `.multilingual/identifier-map.json` na raiz do projeto:

```json
{
  "identifiers": {
    "Calculator": {
      "pt-br": "Calculadora"
    },
    "Add": {
      "pt-br": "Somar"
    }
  },
  "literals": {}
}
```

Alternativamente, usar anotacoes `// tradu:` diretamente no codigo (ver [Primeiros Passos](getting-started.md#usando-anotacoes-tradu)).
