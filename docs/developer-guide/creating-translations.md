# Criar Traducoes

## Indice

- [Formato JSON](#formato-json)
- [Adicionar novo idioma](#adicionar-novo-idioma)
- [Categorias de traducao](#categorias-de-traducao)
- [Validacao](#validacao)
- [Testar](#testar)

## Formato JSON

As traducoes sao armazenadas em ficheiros JSON com a seguinte estrutura:

### keywords-base.json (por linguagem de programacao)

```json
{
  "keywords": {
    "class": 10,
    "if": 30,
    "else": 18,
    "void": 75,
    "int": 33,
    "return": 52
  }
}
```

### Traducao (por idioma natural)

```json
{
  "version": "1.0.0",
  "languageCode": "pt-br",
  "languageName": "Portugues Brasileiro",
  "programmingLanguage": "CSharp",
  "translations": {
    "10": "classe",
    "30": "se",
    "18": "senao",
    "75": "vazio",
    "33": "inteiro",
    "52": "retornar"
  }
}
```

## Adicionar novo idioma

1. No repositorio `babel-tcc-translations`, criar directorio `natural-languages/<codigo-idioma>/`
2. Copiar um arquivo existente da mesma linguagem de programacao como base (ex: `pt-br/python.json`)
3. Atualizar `languageCode`, `languageName` e todas as traducoes
4. Testar com o Core

Exemplo para Espanhol (ES-ES):

```
natural-languages/es-es/csharp.json
```

```json
{
  "version": "1.0.0",
  "languageCode": "es-es",
  "languageName": "Espanol",
  "programmingLanguage": "CSharp",
  "translations": {
    "10": "clase",
    "30": "si",
    "18": "sino",
    "75": "vacio",
    "33": "entero",
    "52": "retornar"
  }
}
```

## Categorias de traducao

As keywords C# estao organizadas por categoria:

| Categoria | Exemplos (EN) | Exemplos (PT-BR) |
|-----------|--------------|-------------------|
| Tipos | int, string, bool, void | inteiro, texto, booleano, vazio |
| Controle | if, else, for, while, return | se, senao, para, enquanto, retornar |
| Modificadores | public, static, abstract | publico, estatico, abstrato |
| Declaracao | class, struct, enum, namespace | classe, estrutura, enumeracao, espaconome |
| Literais | true, false, null | verdadeiro, falso, nulo |
| Operadores | new, typeof, sizeof, as, is | novo, tipode, tamanhode, como, e |

## Validacao

Verificar que:
- Todas as 89 keywords C# tem traducao
- IDs numericos correspondem ao keywords-base.json
- JSON e valido (sem erros de parsing)
- Nenhuma traducao esta vazia
- Traducoes sao palavras unicas (sem espacos) quando possivel

## Testar

```bash
cd babel-tcc
dotnet run --project packages/core/MultiLingualCode.Core.Host -- \
  --method TranslateToNaturalLanguage \
  --params '{"sourceCode":"class Program { }","fileExtension":".cs","targetLanguage":"<codigo-idioma>"}' \
  --translations packages/core/MultiLingualCode.Core.Tests/TestData/translations
```

Resultado esperado: keywords traduzidas no idioma escolhido.
