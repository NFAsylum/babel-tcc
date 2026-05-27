# Criar Traduções

## Índice

- [Formato JSON](#formato-json)
- [Adicionar novo idioma](#adicionar-novo-idioma)
- [Categorias de tradução](#categorias-de-tradução)
- [Validação](#validação)
- [Testar](#testar)

## Formato JSON

As traduções são armazenadas em arquivos JSON com a seguinte estrutura:

### keywords-base.json (por linguagem de programação)

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

### Tradução (por idioma natural)

```json
{
  "version": "1.0.0",
  "languageCode": "pt-br",
  "languageName": "Português (Brasil)",
  "programmingLanguage": "CSharp",
  "translations": {
    "10": "classe",
    "30": "se",
    "18": "senão",
    "75": "vazio",
    "33": "inteiro",
    "52": "retornar"
  }
}
```

## Adicionar novo idioma

1. No repositório `babel-tcc-translations`, criar diretório `natural-languages/<codigo-idioma>/`
2. Copiar um arquivo existente da mesma linguagem de programação como base (ex: `pt-br/python.json`)
3. Atualizar `languageCode`, `languageName` e todas as traduções
4. Testar com o Core

Exemplo para Espanhol (ES-ES):

```
natural-languages/es-es/csharp.json
```

```json
{
  "version": "1.0.0",
  "languageCode": "es-es",
  "languageName": "Español",
  "programmingLanguage": "CSharp",
  "translations": {
    "10": "clase",
    "30": "si",
    "18": "sino",
    "75": "vacío",
    "33": "entero",
    "52": "retornar"
  }
}
```

## Categorias de tradução

As keywords C# estão organizadas por categoria:

| Categoria | Exemplos (EN) | Exemplos (PT-BR) |
|-----------|--------------|-------------------|
| Tipos | int, string, bool, void | inteiro, texto, booleano, vazio |
| Controle | if, else, for, while, return | se, senão, para, enquanto, retornar |
| Modificadores | public, static, abstract | público, estático, abstrato |
| Declaração | class, struct, enum, namespace | classe, estrutura, enumeração, espaçonome |
| Literais | true, false, null | verdadeiro, falso, nulo |
| Operadores | new, typeof, sizeof, as, is | novo, tipode, tamanhode, como, igual |

## Validação

Verificar que:
- Todas as 89 keywords C# têm tradução
- IDs numéricos correspondem ao keywords-base.json
- JSON é válido (sem erros de parsing)
- Nenhuma tradução está vazia
- Traduções são palavras únicas (sem espaços) quando possível

## Testar

```bash
cd babel-tcc
dotnet run --project packages/core/MultiLingualCode.Core.Host -- \
  --method TranslateToNaturalLanguage \
  --params '{"sourceCode":"class Program { }","fileExtension":".cs","targetLanguage":"<codigo-idioma>"}' \
  --translations ../babel-tcc-translations
```

Resultado esperado: keywords traduzidas no idioma escolhido.
