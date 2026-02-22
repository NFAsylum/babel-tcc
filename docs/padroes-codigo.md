# Padroes de Codigo

## Geral

- Codigo-fonte e comentarios tecnicos em **ingles**
- Documentacao de usuario e commits em **portugues**
- Nomes de arquivo em **ingles** (PascalCase para C#, kebab-case para TS)

## C# (.NET 8)

### Regras Obrigatorias

#### Proibicoes absolutas

| Proibido | Motivo | Usar em vez disso |
|---|---|---|
| `var` | Tipos devem ser explicitos sempre | Tipo explicito: `List<string> items = new List<string>()` |
| `?`, `?.`, `??` (nullable) | Null nao deve existir no sistema | Result pattern, valores default, string vazia |
| `throw` | Excecoes quebram o fluxo de controle | Return com Result/status de sucesso/falha |
| `internal` | Tudo deve ser publico e testavel | `public` |
| `partial` | Fragmenta a classe em multiplos arquivos | Uma classe completa por arquivo |
| `private` | Tudo deve ser acessivel e testavel | `public` |
| Constructors | Acoplam inicializacao a instanciacao | Variaveis inicializadas ou static factory method |
| Function overloading | Cria ambiguidade e dificulta leitura | Nomes descritivos distintos: `LoadFromFile`, `LoadFromStream` |
| Classes-deus | Violam responsabilidade unica | Dividir em classes coesas |
| Valores hardcoded | Dificultam manutencao | Constantes nomeadas, configuracao, ou dados estruturados |
| Nomes genericos | `data`, `info`, `result`, `temp`, `item` | Nomes descritivos: `translatedKeyword`, `keywordLookupTable` |
| Classes aninhadas | Dificulta leitura e testabilidade | Cada classe em seu proprio arquivo |

#### Praticas a evitar (usar com justificativa explicita)

| Evitar | Quando permitido |
|---|---|
| `try/catch` | Apenas em boundaries de I/O (leitura de arquivo, rede) com comentario explicando |
| `Dictionary<string, string>` | Usar modelos tipados. Permitido apenas em deserializacao de JSON temporaria |
| `Dictionary<string, object>` | Nunca. Usar modelos tipados sempre |
| Custom generics (`Service<T>`) | Apenas quando o beneficio e claro e documentado |
| `async/await` | Apenas em I/O real (disco, rede). Sempre com comentario explicando porque e async |

#### Praticas obrigatorias

| Regra | Exemplo |
|---|---|
| Tipos explicitos sempre | `string name = "value"` nunca `var name = "value"` |
| Nomes descritivos | `keywordTranslationTable` nunca `table` ou `data` |
| Wrapping de bibliotecas externas | Roslyn deve ser acessado via wrapper, nunca diretamente |
| Codigo estruturado e data-driven | Mapas de dados, tabelas de configuracao, nao if/else chains |
| Uma classe por arquivo | Excecao: enums pequenos podem ficar no arquivo da classe que os usa |
| Result pattern para erros | `OperationResult` com `Success`, `ErrorMessage` em vez de throw/catch |

### Convencoes de Nomenclatura

| Tipo | Convencao | Exemplo |
|---|---|---|
| Namespace | PascalCase | `MultiLingualCode.Core.Services` |
| Classe/Interface | PascalCase | `TranslationOrchestrator`, `ILanguageAdapter` |
| Metodo | PascalCase | `TranslateKeyword()` |
| Propriedade | PascalCase | `LanguageName` |
| Campo | PascalCase | `KeywordLookupTable` |
| Parametro | camelCase | `sourceCode` |
| Variavel local | camelCase | `translatedKeyword` |
| Constante | PascalCase | `MaxFileSize` |
| Interface | IPascalCase | `ILanguageAdapter` |
| Static factory | Create/From/With | `OperationResult.Fail("message")` |

### Testes (xUnit)

- Nomear testes: `MetodoTestado_Cenario_ResultadoEsperado`
- Exemplo: `Parse_SimpleIfStatement_ReturnsKeywordNode`
- Usar padroes Arrange-Act-Assert
- Mocks com NSubstitute

### Estrutura de Projeto

```
MultiLingualCode.Core/
├── Interfaces/          # Contratos
├── Models/
│   ├── AST/             # Hierarquia de nos
│   ├── Translation/     # Tabelas e mapas
│   └── Configuration/   # Preferencias
├── Services/            # Logica de negocio
├── LanguageAdapters/    # Adapters por linguagem
├── Utilities/           # Helpers
└── MultiLingualCode.Core.Tests/
    ├── Services/
    ├── LanguageAdapters/
    └── Models/
```

## TypeScript (VS Code Extension)

### Convencoes de Nomenclatura

| Tipo | Convencao | Exemplo |
|---|---|---|
| Arquivo | camelCase | `coreBridge.ts` |
| Classe | PascalCase | `CoreBridge` |
| Interface | PascalCase | `ValidationResult` |
| Metodo | camelCase | `translateToNaturalLanguage()` |
| Variavel/Parametro | camelCase | `sourceCode` |
| Constante | UPPER_SNAKE_CASE | `MAX_TIMEOUT` |

### Estilo

- Strict mode habilitado no tsconfig
- Preferir `const` sobre `let`; nunca usar `var`
- Tipos explicitos em assinaturas de funcao (parametros e retorno)
- Strings com aspas simples (`'`) exceto em template literals
- Mesmas regras de proibicao de null/throw/private aplicam-se

### Estrutura de Projeto

```
vscode/src/
├── extension.ts         # Entry point
├── adapters/            # IDE adapter
├── providers/           # Content, Edit, Save, Completion, Hover
├── services/            # CoreBridge, Config, LanguageDetector
└── ui/                  # StatusBar, LanguageSelector
```

## JSON (Tabelas de Traducao)

### Formato

- Indentacao: 2 espacos
- UTF-8 sem BOM
- Ordenar keys alfabeticamente (quando aplicavel)
- Ficheiros de traducao devem incluir campos `version`, `languageCode`, `languageName`, `programmingLanguage`

### Schema

Tabela de keywords (`keywords-base.json`):
```json
{
  "keywords": {
    "if": 30,
    "else": 18,
    "class": 10
  }
}
```

Traducao (`pt-br/csharp.json`):
```json
{
  "version": "1.0.0",
  "languageCode": "pt-br",
  "languageName": "Portugues Brasileiro",
  "programmingLanguage": "CSharp",
  "translations": {
    "30": "se",
    "18": "senao",
    "10": "classe"
  }
}
```

## Git

### Branches

- `main` - versao estavel
- `marco-<tarefa>` - branches de trabalho

### Commits

Formato: `tipo: descricao curta`

Tipos:
- `feat:` nova funcionalidade
- `fix:` correcao de bug
- `docs:` documentacao
- `test:` testes
- `refactor:` refatoracao sem mudanca de comportamento
- `chore:` tarefas de manutencao (CI, deps, etc.)

Exemplos:
```
feat: implementar CSharpAdapter.Parse com Roslyn
fix: corrigir round-trip de keywords com acentos
docs: adicionar guia de contribuicao para traducoes
refactor: aplicar padroes de codigo ao core
```

### Pull Requests

- Titulo curto e descritivo
- Descricao com: o que mudou, por que, como testar
- Referenciar tarefa relacionada
