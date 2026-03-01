# Tarefa 049 - Correcoes de Qualidade (Code Review)

## Prioridade: MEDIA

Correcoes identificadas na code review externa. Agrupadas por serem mudancas pontuais
e rapidas de implementar.

## Itens

### 1. Cast para tipo concreto no TranslationOrchestrator
**Arquivo:** `TranslationOrchestrator.cs:139`
**Problema:** `(NaturalLanguageProvider)Provider` quebra a abstracao da interface.
**Correcao:** Adicionar metodo na interface `INaturalLanguageProvider`:
- `GetOriginalKeyword(int keywordId)` que retorna a keyword C# original

### 2. Erro silencioso em LoadTranslationTableAsync
**Arquivo:** `NaturalLanguageProvider.cs:54-57, 71-74`
**Problema:** Se arquivo JSON falha ao carregar, metodo retorna sem sinalizar erro.
**Correcao:** Mudar retorno para `Task<OperationResult>` e propagar o erro.

### 3. Construtor publico do TranslationOrchestrator
**Arquivo:** `TranslationOrchestrator.cs:38`
**Problema:** Construtor publico bypass a validacao do factory `Create()`.
**Correcao:** Mudar visibilidade para `internal` (testes usam `[InternalsVisibleTo]`).
NOTA: padroes-codigo.md diz "NUNCA usar internal" - avaliar se aplica aqui
ou se o padrao precisa de excecao para factories.

### 4. Renomear OriginalKeyword
**Arquivo:** `KeywordNode.cs`
**Problema:** `OriginalKeyword` e sobrescrito com valor traduzido no forward translate.
Nome fica enganoso.
**Correcao:** Renomear para `Text` ou `DisplayText`. Ou manter imutavel e adicionar
campo `TranslatedText` separado (como `IdentifierNode` faz com `TranslatedName`).

### 5. JSON parseado duas vezes no Host
**Arquivo:** `Program.cs`
**Problema:** `ExtractLanguageCode` faz `JsonDocument.Parse()` e handlers fazem
`JsonSerializer.Deserialize()` no mesmo JSON.
**Correcao:** Deserializar uma vez e extrair o language code do objeto tipado.

### 6. Idiomas hardcoded na extensao
**Arquivo:** `extension.ts:76`
**Problema:** `const languages: string[] = ['pt-br']` hardcoded.
**Correcao:** Usar `coreBridge.getSupportedLanguages()` que ja existe.

## Arquivos afetados

- `packages/core/MultiLingualCode.Core/Interfaces/INaturalLanguageProvider.cs`
- `packages/core/MultiLingualCode.Core/Services/TranslationOrchestrator.cs`
- `packages/core/MultiLingualCode.Core/Services/NaturalLanguageProvider.cs`
- `packages/core/MultiLingualCode.Core/Models/AST/KeywordNode.cs`
- `packages/core/MultiLingualCode.Core.Host/Program.cs`
- `packages/ide-adapters/vscode/src/extension.ts`
- Testes correspondentes
