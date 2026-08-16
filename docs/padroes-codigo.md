# Padrões de Código

## Geral

- Código-fonte e comentários técnicos em **inglês**
- Documentação de usuário e commits em **português**
- Nomes de arquivo em **inglês** (PascalCase para C#, kebab-case para TS)

Este arquivo é a **fonte única** das regras de código. Nenhum outro documento do
repositório repete regra daqui — uma cópia resumida em outro lugar diverge e
passa a ser seguida como se fosse a regra completa.

## Alcance das regras

As **proibições absolutas**, as **práticas a evitar** e as **práticas
obrigatórias** da seção de C# valem para **todas as linguagens do projeto**. Elas
estão escritas por extenso em C# porque é a maior camada; as seções das demais
linguagens registram apenas como cada regra se expressa naquele idioma, mais as
exceções justificadas.

Uma linguagem nova herda as mesmas proibições. Uma regra só deixa de valer
quando a linguagem não tem o construto (`partial` não existe em Kotlin) ou
quando cumpri-la exigiria abandonar um recurso estrutural do idioma — e nesse
caso a exceção fica escrita, com o motivo.

As convenções de **nomenclatura** e de **nomes de teste** são por linguagem, e
cada seção declara as suas.

### Conformidade do código existente

Estas regras valem para código novo. Parte do código escrito antes delas ainda
não é conforme — o plugin IntelliJ, o tokenizer Python e os scripts de build.

A adequação é trabalho próprio, rastreado na tarefa
`tarefas/tarefa112-adequar-codigo-aos-padroes`, e não é requisito de quem toca
nesses arquivos por outro motivo.

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
- Todas as proibições absolutas valem aqui, incluindo null, `throw`, `private` e
  o operador ternário (ver [Alcance das regras](#alcance-das-regras))

### Estrutura de Projeto

```
vscode/src/
├── extension.ts         # Entry point
├── adapters/            # IDE adapter
├── providers/           # Content, Edit, Save, Completion, Hover
├── services/            # CoreBridge, Config, LanguageDetector
└── ui/                  # StatusBar, LanguageSelector
```

## Kotlin (Plugin IntelliJ)

Todas as proibições absolutas valem aqui. A tabela registra como cada uma se
expressa em Kotlin, não uma lista nova.

### Proibições — como se aplicam

| Regra | Em Kotlin |
|---|---|
| `var` | O `var` de Kotlin é mutabilidade, não inferência. A regra que transfere é **tipo explícito na declaração**: `val timeoutMs: Long = 10_000`, nunca `val timeoutMs = 10_000`. Prefira `val` a `var` |
| `?`, `?.`, `?:` | Proibido tipo anulável. Usar Result pattern, valor default ou string vazia. **Exceção de boundary**: APIs Java que devolvem `null` (`BufferedReader.readLine()`), com o tratamento contido na função que faz a chamada |
| `? :` (ternário) | Kotlin não tem ternário. `if/else` como expressão é a forma correta |
| `throw` | Proibido. O erro atravessa a fronteira como valor, não como exceção |
| `internal`, `private` | Proibidos. Tudo `public` e testável |
| `partial` | Não existe em Kotlin |
| Constructors | **Exceção documentada**: o construtor primário é estrutural em Kotlin — `data class` não existe sem ele. Permitido apenas para declarar as propriedades do construtor primário. Proibida lógica de inicialização nele ou em `init`; isso vai para static factory no `companion object` |
| Function overloading | Proibido, incluindo default arguments que simulam overload. Nomes distintos e descritivos |
| Classes aninhadas | Proibidas. Uma classe por arquivo, declarada no topo |
| Classes-deus, valores hardcoded, nomes genéricos | Idênticos ao C# |

### Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Arquivo | PascalCase, igual ao nome da classe | `CoreBridge.kt` |
| Pacote | minúsculas, sem separador | `com.nfasylum.babel.intellij.services` |
| Classe/Interface | PascalCase | `CoreBridge`, `CoreTransport` |
| Função | camelCase | `translateToNaturalLanguage()` |
| Propriedade | camelCase | `timeoutMs` |
| Constante (`const val`) | UPPER_SNAKE_CASE | `DEFAULT_TIMEOUT_MS` |
| Parâmetro/variável local | camelCase | `sourceCode` |

### Testes (JUnit)

- Nomear com frase entre crases descrevendo o comportamento observável:
  ``fun `timeout message carries what the Core wrote to stderr`()``
- Usar padrões Arrange-Act-Assert
- Substituir dependências pelas costuras declaradas no próprio código
  (`transportFactory`, `CoreTransport`), não por mocking de framework

### Logging

- Um `Logger` por classe, via `Logger.getInstance(Classe::class.java)`
- `error` para falha real, `warn` para degradação, `debug` para ruído por
  requisição. Nunca descartar a exceção quando o overload aceita o throwable

## Python (Tokenizer do Adapter)

`tokenizer_service.py` roda no interpretador **do usuário**, num ambiente que o
projeto não controla. Isso domina as regras desta seção.

### Restrições de ambiente

| Regra | Motivo |
|---|---|
| Somente biblioteca padrão, nunca dependência de terceiros | Ninguém deve rodar `pip install` para a extensão funcionar |
| Compatível com Python 3.8+ | É o piso verificado por `PythonTokenizerService.MinimumPythonVersion` |
| Uma responsabilidade por script: tokenizar | Regra de tradução mora no C# |

### Proibições — como se aplicam

| Regra | Em Python |
|---|---|
| Tipos explícitos | Anotações de tipo obrigatórias na assinatura de função |
| `?` (nullable) | Proibido usar `None` como sinal de erro ou retorno opcional. Devolver o envelope `{"ok": false, "error": "..."}` |
| `? :` (ternário) | Proibido `a if cond else b`. Usar bloco `if/else` |
| `throw` | Nenhuma exceção pode escapar do laço de `main()`. O erro vira valor no protocolo — é o Result pattern atravessando a fronteira de processo |
| `private` | Proibido o prefixo `_` para simular visibilidade |
| Nomes genéricos, valores hardcoded, classes-deus | Idênticos ao C# |

### Protocolo

- **stdout é canal de protocolo; stderr é diagnóstico.** Nunca escrever mensagem
  livre no stdout: seria lida como resposta JSON e quebraria o enquadramento
- `sys.stdout.flush()` após cada resposta — o processo é persistente e o outro
  lado bloqueia esperando a linha

### Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|---|---|---|
| Arquivo | snake_case | `tokenizer_service.py` |
| Função | snake_case | `tokenize_source()` |
| Variável/Parâmetro | snake_case | `source_code` |
| Constante | UPPER_SNAKE_CASE | `MAX_TOKENS` |

## JavaScript (Scripts de Build)

Rodam no Node durante o empacotamento, nunca em produção. As proibições valem
igual; a diferença está no tratamento de falha.

| Regra | Em JavaScript |
|---|---|
| `var` | Proibido. `const` por padrão, `let` só quando há reatribuição real |
| `?`, `?.`, `??` | Proibido usar `null`/`undefined` como sinal. Valor default ou encerramento explícito |
| `? :` (ternário) | Proibido. Bloco `if/else` |
| `throw` | Proibido. **Falha de build encerra alto**: `console.error` com o motivo e `process.exit(1)`. Não é exceção propagada, é término explícito |
| `#private` | Proibido |
| Somente biblioteca padrão do Node (`fs`, `path`) | Sem dependências |
| CommonJS (`require`), não ESM | |
| Indentação e encoding | Seguem o `.editorconfig` |

**Falha de build nunca degrada silenciosamente.** É o oposto do runtime: lá o
arquivo do usuário é preservado mostrando o original; aqui, empacotar um `.vsix`
sem traduções ou sem README publica um produto quebrado. Falhe alto.

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
