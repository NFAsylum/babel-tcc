# Padroes de Codigo

## Geral

- Codigo-fonte e comentarios tecnicos em **ingles**
- Documentacao de usuario e commits em **portugues**
- Nomes de arquivo em **ingles** (PascalCase para C#, kebab-case para TS)

## C# (.NET 8)

### Convencoes de Nomenclatura

| Tipo | Convencao | Exemplo |
|---|---|---|
| Namespace | PascalCase | `MultiLingualCode.Core.Services` |
| Classe/Interface | PascalCase | `TranslationOrchestrator`, `ILanguageAdapter` |
| Metodo publico | PascalCase | `TranslateToNaturalLanguageAsync()` |
| Metodo privado | PascalCase | `ConvertToCustomAST()` |
| Propriedade | PascalCase | `LanguageName` |
| Campo privado | _camelCase | `_languageRegistry` |
| Parametro | camelCase | `sourceCode` |
| Variavel local | camelCase | `translatedAst` |
| Constante | PascalCase | `MaxFileSize` |
| Interface | IPascalCase | `ILanguageAdapter` |

### Estilo

- Usar `async/await` para operacoes I/O
- Metodos async terminam com `Async` (ex: `TranslateToNaturalLanguageAsync`)
- Injecao de dependencia via construtor
- Nullable reference types habilitado (`<Nullable>enable</Nullable>`)
- XML docs em todas as interfaces e metodos publicos
- Uma classe por arquivo (exceto tipos pequenos auxiliares)

### Testes (xUnit)

- Nomear testes: `MetodoTestado_Cenario_ResultadoEsperado`
- Exemplo: `Parse_SimpleIfStatement_ReturnsKeywordNode`
- Usar padroes Arrange-Act-Assert
- Mocks com Moq ou NSubstitute

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
- Usar `async/await` (nunca callbacks ou `.then()` aninhados)
- Preferir `const` sobre `let`; nunca usar `var`
- Tipos explicitos em assinaturas de funcao (parametros e retorno)
- Strings com aspas simples (`'`) exceto em template literals

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
- Incluir campo `version` no topo

### Schema

Tabela de keywords (`keywords-base.json`):
```json
{
  "version": "1.0.0",
  "language": "CSharp",
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
- `develop` - desenvolvimento ativo (se usar GitFlow)
- `feature/<nome>` - novas funcionalidades
- `bugfix/<nome>` - correcoes
- `release/<versao>` - preparacao de release

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
test: adicionar testes de performance para arquivos grandes
```

### Pull Requests

- Titulo curto e descritivo
- Descricao com: o que mudou, por que, como testar
- Referenciar tarefa relacionada
- Requerer pelo menos 1 code review
