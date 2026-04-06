# Tarefa 098 - TextScan generico para suporte rapido a multiplas linguagens

## Prioridade: HIGH

## Contexto
O TextScanTranslator (tarefa 061) traduz keywords C# em 0-1ms via scan
linear. Esta hardcoded para C# (skip //, /* */, #preprocessor, """).
PythonAdapter.ReverseSubstituteKeywords faz o mesmo para Python (skip #,
strings triple-quoted, f-strings). Os dois scanners sao duplicados.

## Objetivo
Generalizar o TextScanTranslator para suportar qualquer linguagem via
configuracao, eliminando a necessidade de parser/tokenizer externo para
traducao de keywords. Permitir adicionar novas linguagens com apenas:
1. Configuracao de regras de scan (comentarios, strings)
2. keywords-base.json
3. Traducoes JSON por idioma

## Escopo

### 1. LanguageScanRules (configuracao por linguagem)

```csharp
public class LanguageScanRules
{
    public string LineComment = "";          // "//" para C#, "#" para Python/Ruby
    public string BlockCommentStart = "";    // "/*" para C#/Java/JS, "" para Python
    public string BlockCommentEnd = "";      // "*/" para C#/Java/JS
    public bool HasPreprocessor = false;     // true para C#, false para Python/JS
    public List<StringDelimiter> StringDelimiters = new();
}

public class StringDelimiter
{
    public string Open = "";       // " ou ' ou """ ou ''' ou `
    public string Close = "";      // mesmo que Open (ou diferente para template literals)
    public string EscapeChar = ""; // \\ para maioria, "" para verbatim C#
    public bool IsTriple = false;  // """ ou '''
    public bool IsRaw = false;     // r"..." em Python
}
```

### 2. Regras pre-definidas para linguagens existentes e novas

| Linguagem | LineComment | BlockComment | Preprocessor | Strings |
|-----------|------------|-------------|--------------|---------|
| C# | // | /* */ | # | " ' @" $" """ |
| Python | # | — | — | " ' """ ''' f" r" |
| JavaScript | // | /* */ | — | " ' ` |
| TypeScript | // | /* */ | — | " ' ` |
| Java | // | /* */ | — | " ' """ (Java 13+) |
| Go | // | /* */ | — | " ` |
| Rust | // | /* */ | — | " r#" |
| Ruby | # | =begin/=end | — | " ' |
| PHP | // # | /* */ | — | " ' |
| Kotlin | // | /* */ | — | " """ |
| Swift | // | /* */ | — | " """ |

### 3. Refatorar TextScanTranslator

- Aceitar LanguageScanRules como parametro em vez de hardcodar C#
- Mover regras C# para CSharpScanRules
- Criar PythonScanRules

### 3b. Interface ITextScannable (sem breaking change)

Em vez de adicionar GetScanRules() a ILanguageAdapter (breaking change
que obrigaria todos os adapters a implementar), criar interface separada:

```csharp
public interface ITextScannable
{
    LanguageScanRules GetScanRules();
}
```

Adapters implementam opcionalmente:
```csharp
public class CSharpAdapter : ILanguageAdapter, ITextScannable { ... }
public class PythonAdapter : ILanguageAdapter, ITextScannable { ... }
```

### 4. Integrar no TranslationOrchestrator

```csharp
if (adapter is ITextScannable scannable && !sourceCode.Contains("tradu"))
{
    // Fast path: Text Scan com regras da linguagem
    Dictionary<string, string> map = TextScanTranslator.BuildTranslationMap(...);
    return TextScanTranslator.Translate(source, map, scannable.GetScanRules());
}
// Full path: parser/tokenizer
```

- Sem nullable, sem breaking change, extensivel
- Adapter sem ITextScannable usa parser normalmente (fallback transparente)

### 5. Suporte rapido a novas linguagens

Para adicionar JavaScript sem parser:
1. Criar JavaScriptScanRules com regras de comentario/string
2. Criar JavaScriptKeywordMap com as ~60 keywords JS
3. Criar keywords-base.json e traducoes JSON
4. Registrar adapter minimo que retorna GetScanRules()

Sem parser, sem tokenizer, sem subprocess. Traducao de keywords
funcional em 0-1ms.

### 6. Python: usar Text Scan para forward translation

Substituir tokenizer subprocess por Text Scan para arquivos Python
sem tradu. Manter tokenizer como fallback para arquivos com tradu.
Elimina overhead de IPC (~8ms por request).

## Performance esperada

| Linguagem | Atual | Com Text Scan | Speedup |
|-----------|-------|-------------|---------|
| C# (sem tradu) | 0-1ms | 0-1ms | Ja implementado |
| C# (com tradu) | 35-4077ms | 35-4077ms | Sem mudanca |
| Python (sem tradu) | ~8ms (IPC) | 0-1ms | ~8x |
| JavaScript (novo) | N/A | 0-1ms | Instantaneo |

## Impacto

- Adicionar nova linguagem: ~2 horas (config + JSONs) em vez de ~2 semanas (parser)
- Suporte a dezenas de linguagens com apenas keywords: viavel em 1 semana
- Performance 0-1ms para todas as linguagens sem tradu
- Zero dependencia externa (sem subprocess, sem parser, sem runtime adicional)
