# Guia para Criar Traducoes

Este guia explica como contribuir com novas traducoes de idioma para o projeto. **Nao e necessario saber programar em C# ou TypeScript** para contribuir com traducoes.

## Estrutura das Traducoes

O repositorio `babel-tcc-translations` contem:

```
programming-languages/
├── csharp/
│   └── keywords-base.json       # Keywords C# -> IDs numericos
├── python/
│   └── keywords-base.json
└── javascript/
    └── keywords-base.json

natural-languages/
├── pt-br/
│   ├── csharp.json              # Traducoes PT-BR para C#
│   └── common-identifiers.json  # Termos comuns
├── es-es/
│   └── csharp.json
└── [seu-idioma]/
    └── csharp.json
```

## Como Funciona

Cada keyword de programacao tem um **ID numerico** definido em `keywords-base.json`:

```json
{
  "keywords": {
    "if": 30,
    "else": 18,
    "class": 10,
    "void": 75
  }
}
```

Cada traducao mapeia esses IDs para o idioma alvo:

```json
{
  "translations": {
    "30": "se",
    "18": "senao",
    "10": "classe",
    "75": "vazio"
  }
}
```

## Passo a Passo: Criar Nova Traducao

### 1. Fork e clone

```bash
git clone https://github.com/SEU-USUARIO/babel-tcc-translations.git
cd babel-tcc-translations
```

### 2. Criar pasta do idioma

Use o codigo de idioma no formato `xx-yy` (ISO 639-1 + ISO 3166-1):

```bash
mkdir -p natural-languages/fr-fr
```

Exemplos de codigos:
- `pt-br` - Portugues Brasileiro
- `es-es` - Espanhol (Espanha)
- `fr-fr` - Frances (Franca)
- `de-de` - Alemao (Alemanha)
- `it-it` - Italiano (Italia)
- `ja-jp` - Japones (Japao)

### 3. Criar arquivo de traducao

Copiar o template stub (`docs/template.json`) e preencher:

```json
{
  "version": "1.0.0",
  "languageCode": "fr-fr",
  "languageName": "Francais",
  "programmingLanguage": "CSharp",
  "compatibleKeywordVersion": "1.0.0",
  "translations": {
    "0": "abstrait",
    "1": "comme",
    "2": "base",
    "3": "booleen",
    "4": "arreter",
    "10": "classe",
    "30": "si",
    "18": "sinon",
    "33": "entier",
    "75": "vide"
  },
  "commonIdentifiers": {
    "count": "compteur",
    "index": "indice",
    "value": "valeur",
    "name": "nom"
  }
}
```

### 4. Regras para boas traducoes

- **Sem acentos em keywords** (manter compatibilidade com teclados): prefira `senao` em vez de `senão`
- **Uma palavra so** quando possivel: `paracada` (foreach), `espaconome` (namespace)
- **Consistencia**: se `int` e `inteiro`, `uint` deve ser `uinteiro`
- **Intuitividade**: a traducao deve ser reconhecivel por um falante nativo
- Traducoes de `commonIdentifiers` sao sugestoes; usuarios podem sobrescrever

### 5. Validar

```bash
# Verificar que o JSON e valido
node -e "JSON.parse(require('fs').readFileSync('natural-languages/fr-fr/csharp.json'))"

# Verificar que todos os IDs existem em keywords-base.json
# (ferramenta de validacao sera adicionada futuramente)
```

### 6. Submeter PR

```bash
git checkout -b feature/add-french-translation
git add natural-languages/fr-fr/
git commit -m "feat: adicionar traducao Frances (FR-FR) para C#"
git push origin feature/add-french-translation
```

Abrir Pull Request no GitHub com:
- Titulo: "feat: adicionar traducao [Idioma] para [Linguagem]"
- Indicar se e falante nativo ou fluente
- Indicar nivel de completude (quantos % das keywords foram traduzidos)

## Referencia: IDs de Keywords C#

| ID | Keyword | Descricao |
|---|---|---|
| 0 | abstract | Classe/metodo abstrato |
| 3 | bool | Tipo booleano |
| 4 | break | Sair de loop |
| 6 | case | Caso em switch |
| 7 | catch | Capturar excecao |
| 10 | class | Declarar classe |
| 11 | const | Constante |
| 18 | else | Senao (condicional) |
| 27 | for | Loop for |
| 28 | foreach | Loop foreach |
| 30 | if | Se (condicional) |
| 33 | int | Tipo inteiro |
| 39 | namespace | Espaco de nomes |
| 47 | private | Acesso privado |
| 49 | public | Acesso publico |
| 52 | return | Retornar valor |
| 58 | static | Estatico |
| 59 | string | Tipo texto |
| 65 | try | Tentar (excecoes) |
| 72 | using | Importar namespace |
| 75 | void | Sem retorno |
| 77 | while | Loop while |

Consultar `programming-languages/csharp/keywords-base.json` para a lista completa de 78 keywords.
