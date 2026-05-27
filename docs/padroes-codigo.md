# Padrões de Código

## Geral

- Código-fonte e comentários técnicos em **inglês**
- Documentação de usuário e commits em **português**
- Nomes de arquivo em **inglês** (PascalCase para C#, kebab-case para TS)

## C# (.NET 8)

### Regras Obrigatórias

#### Proibições absolutas

| Proibido | Motivo | Usar em vez disso |
|---|---|---|
| `var` | Tipos devem ser explícitos sempre | Tipo explícito: `List<string> items = new List<string>()` |
| `?`, `?.`, `??` (nullable) | Null não deve existir no sistema | Result pattern, valores default, string vazia |
| `? :` (ternário) | Reduz legibilidade e dificulta debug | Bloco `if/else` explícito |
| `throw` | Exceções quebram o fluxo de controle | Return com Result/status de sucesso/falha |
| `internal` | Tudo deve ser público e testável | `public` |
| `partial` | Fragmenta a classe em múltiplos arquivos | Uma classe completa por arquivo |
| `private` | Tudo deve ser acessível e testável | `public` |
| Constructors | Acoplam inicialização à instanciação | Variáveis inicializadas ou static factory method |
| Function overloading | Cria ambiguidade e dificulta leitura | Nomes descritivos distintos: `LoadFromFile`, `LoadFromStream` |
| Classes-deus | Violam responsabilidade única | Dividir em classes coesas |
| Valores hardcoded | Dificultam manutenção | Constantes nomeadas, configuração, ou dados estruturados |
| Nomes genéricos | `data`, `info`, `result`, `temp`, `item` | Nomes descritivos: `translatedKeyword`, `keywordLookupTable` |
| Classes aninhadas | Dificulta leitura e testabilidade | Cada classe em seu próprio arquivo |

#### Práticas a evitar (usar com justificativa explícita)

| Evitar | Quando permitido |
|---|---|
| `try/catch` | Apenas em boundaries de I/O (leitura de arquivo, rede) com comentário explicando |
| `Dictionary<string, string>` | Usar modelos tipados. Permitido apenas em desserialização de JSON temporária |
| `Dictionary<string, object>` | Nunca. Usar modelos tipados sempre |
| Custom generics (`Service<T>`) | Apenas quando o benefício é claro e documentado |
| `async/await` | Apenas em I/O real (disco, rede). Sempre com comentário explicando porque é async |

#### Práticas obrigatórias

| Regra | Exemplo |
|---|---|
| Tipos explícitos sempre | `string name = "value"` nunca `var name = "value"` |
| Nomes descritivos | `keywordTranslationTable` nunca `table` ou `data` |
| Wrapping de bibliotecas externas | Roslyn deve ser acessado via wrapper, nunca diretamente |
| Código estruturado e data-driven | Mapas de dados, tabelas de configuração, não if/else chains |
| Uma classe por arquivo | Exceção: enums pequenos podem ficar no arquivo da classe que os usa |
| Result pattern para erros | `OperationResult` com `Success`, `ErrorMessage` em vez de throw/catch |

### Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Namespace | PascalCase | `MultiLingualCode.Core.Services` |
| Classe/Interface | PascalCase | `TranslationOrchestrator`, `ILanguageAdapter` |
| Método | PascalCase | `TranslateKeyword()` |
| Propriedade | PascalCase | `LanguageName` |
| Campo | PascalCase | `KeywordLookupTable` |
| Parâmetro | camelCase | `sourceCode` |
| Variável local | camelCase | `translatedKeyword` |
| Constante | PascalCase | `MaxFileSize` |
| Interface | IPascalCase | `ILanguageAdapter` |
| Static factory | Create/From/With | `OperationResult.Fail("message")` |

### Testes (xUnit)

- Nomear testes: `MetodoTestado_Cenario_ResultadoEsperado`
- Exemplo: `Parse_SimpleIfStatement_ReturnsKeywordNode`
- Usar padrões Arrange-Act-Assert
- Mocks com NSubstitute

### Estrutura de Projeto

```
MultiLingualCode.Core/
├── Interfaces/          # Contratos
├── Models/
│   ├── AST/             # Hierarquia de nós
│   ├── Translation/     # Tabelas e mapas
│   └── Configuration/   # Preferências
├── Services/            # Lógica de negócio
├── LanguageAdapters/    # Adapters por linguagem
├── Utilities/           # Helpers
└── MultiLingualCode.Core.Tests/
    ├── Services/
    ├── LanguageAdapters/
    └── Models/
```

## TypeScript (VS Code Extension)

### Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Arquivo | camelCase | `coreBridge.ts` |
| Classe | PascalCase | `CoreBridge` |
| Interface | PascalCase | `ValidationResult` |
| Método | camelCase | `translateToNaturalLanguage()` |
| Variável/Parâmetro | camelCase | `sourceCode` |
| Constante | UPPER_SNAKE_CASE | `MAX_TIMEOUT` |

### Estilo

- Strict mode habilitado no tsconfig
- Preferir `const` sobre `let`; nunca usar `var`
- Tipos explícitos em assinaturas de função (parâmetros e retorno)
- Strings com aspas simples (`'`) exceto em template literals
- Mesmas regras de proibição de null/throw/private aplicam-se

### Estrutura de Projeto

```
vscode/src/
├── extension.ts         # Entry point
├── adapters/            # IDE adapter
├── providers/           # Content, Edit, Save, Completion, Hover
├── services/            # CoreBridge, Config, LanguageDetector
└── ui/                  # StatusBar, LanguageSelector
```

## JSON (Tabelas de Tradução)

### Formato

- Indentação: 2 espaços
- UTF-8 sem BOM
- Ordenar keys alfabeticamente (quando aplicável)
- Arquivos de tradução devem incluir campos `version`, `languageCode`, `languageName`, `programmingLanguage`

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

Tradução (`pt-br/csharp.json`):
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

- `main` - versão estável
- `marco-<tarefa>` - branches de trabalho

### Commits

Formato: `tipo: descricao curta`

Tipos:
- `feat:` nova funcionalidade
- `fix:` correção de bug
- `docs:` documentação
- `test:` testes
- `refactor:` refatoração sem mudança de comportamento
- `chore:` tarefas de manutenção (CI, deps, etc.)

Exemplos:
```
feat: implementar CSharpAdapter.Parse com Roslyn
fix: corrigir round-trip de keywords com acentos
docs: adicionar guia de contribuicao para traducoes
refactor: aplicar padroes de codigo ao core
```

### Pull Requests

- Título curto e descritivo
- Descrição com: o que mudou, por que, como testar
- Referenciar tarefa relacionada

### Histórico append-only

O histórico do Git é append-only. São **proibidos**:

| Operação | Motivo |
|---|---|
| `git commit --amend` | Reescreve commit existente, mesmo antes de push |
| `git rebase` (interativo ou não) | Reescreve sequência de commits |
| `git push --force` / `--force-with-lease` | Sobrescreve histórico remoto, pode clobbar trabalho de colaboradores |
| `git reset --hard` para descartar commits | Apaga commits do histórico local |

Quando algo precisa ser corrigido após um commit (mesmo antes do push), criar **novo commit**: `fix:`, `revert:`, `refactor:` conforme apropriado. A trilha de "fiz -> desfiz" é mais transparente em code review do que história reescrita, e mostra explicitamente o raciocínio que levou à decisão final.

Se houver situação real de recuperação (ex: commit acidental de segredo), parar e alinhar antes de qualquer ação destrutiva — nunca aplicar reescrita de histórico unilateralmente.
