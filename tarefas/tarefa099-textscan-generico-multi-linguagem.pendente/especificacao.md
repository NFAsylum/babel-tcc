# Tarefa 099 - TextScan generico para suporte rapido a multiplas linguagens

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

### Suporte completo (LanguageScanRules suficiente)

| Linguagem | LineComment | BlockComment | Preprocessor | Strings |
|-----------|------------|-------------|--------------|---------|
| C# | // | /* */ | # | " ' @" $" """ |
| Python | # | — | — | " ' """ ''' f" r" |
| Java | // | /* */ | — | " ' """ (Java 13+) |
| PHP | // # | /* */ | — | " ' |
| Kotlin | // | /* */ | — | " """ |

### Suporte parcial (limitacoes documentadas)

| Linguagem | Limitacao | Impacto |
|-----------|----------|---------|
| JavaScript/TypeScript | Template literals `` `${expr}` `` — backtick tratado como string delimiter, conteudo inteiro skippado | Keywords dentro de template literals (tanto texto quanto `${}`) nao sao traduzidas |
| Go | `int`, `string`, `true` nao sao reserved words — sao predeclared identifiers que podem ser redefinidos | Text Scan traduziria todas as ocorrencias, mesmo quando redefinidas pelo usuario. Raro na pratica. |
| Swift | Nested comments `/* /* */ */` e string interpolation `\(expr)` | Necessita depth counter e tratamento de `\()` |
| Rust | Lifetimes `'a` confundem com char literal. Raw strings `r#"..."#` e nested `/* */` nao suportados. | Necessitaria extensoes ao LanguageScanRules |
| Ruby | Heredocs `<<~HEREDOC`, syntaxes `%w`, `%q`, `=begin/=end` block comments | Necessitaria extensoes ao LanguageScanRules |

### Resumo de suporte

As limitacoes acima nao afetam C#, Python, Java, PHP e Kotlin.
Para JS/TS e Go, o suporte parcial e funcional para a maioria dos
arquivos — backtick tratado como string (skip tudo, keywords em `${}`
nao traduzidas), redefinicao de predeclared identifiers em Go e raro.
Para Swift, depth counter para nested comments e tratamento de `\(expr)`
sao necessarios.
Para Rust e Ruby, LanguageScanRules precisaria de extensoes (depth
counter, heredoc parser, etc.).

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

## Limitacoes conhecidas por linguagem

LanguageScanRules resolve C#, Python e linguagens com sintaxe similar.
Linguagens com construtos mais complexos precisam de extensoes ao
scanner. Documentado aqui para que o implementador saiba o que precisa
resolver ao adicionar cada linguagem.

### Suporte completo (LanguageScanRules suficiente)
| Linguagem | Notas |
|-----------|-------|
| C# | Ja implementado. Verbatim @"" com escape "" e limitacao documentada. |
| Python | LineComment="#". Triple quotes. f-strings simples. |
| Java | Mesmo que C# sem preprocessor. """ (Java 13+) requer IsTriple. |
| Kotlin | Mesmo que Java. String templates ${} simples. |
| PHP | LineComment="// e #". Sem construtos exoticos no caso basico. |

### Suporte parcial (precisa de extensoes ao scanner)
| Linguagem | Problema | Extensao necessaria |
|-----------|----------|---------------------|
| JavaScript/TypeScript | Template literals `` `...${expr}...` `` com expressoes aninhadas. Scanner trata backtick como string atomica — keywords dentro de ${} nao sao traduzidas. | Adicionar HasInterpolation + InterpolationOpen/Close ao StringDelimiter. Scanner recursivo para ${expr} (Core TokenizeLine ja faz isso). |
| Rust | Lifetimes 'a confundem com char literals. Nested comments /* /* */ */. Raw strings r#"..."# com # variavel. | Verificar ' seguido de identifier sem ' de fecho = lifetime (nao string). Depth counter para nested comments. Contagem de # para raw strings. |
| Ruby | Heredocs <<~HEREDOC...HEREDOC com delimitador arbitrario. Nested string syntaxes (%w, %q, %Q). | HeredocStyle: scanner le delimiter na abertura, matcha na linha seguinte. Syntaxes % tratadas como string com delimitador configuravel. |
| Swift | Nested comments /* /* */ */. Multi-line strings """. String interpolation \(expr). | Depth counter. Interpolacao com \() em vez de ${}. |
| Go | int, string, true, false, nil NAO sao reserved words — podem ser variaveis. TextScan traduziria todas. | NAO e problema do scanner — e do keyword map. Nao incluir builtins no keyword map de Go. Documentar quais palavras sao realmente reserved (25 keywords). |

### C# 11 raw strings (afecta linguagem actual)
C# 11 introduziu raw string literals com numero variavel de ":
  var json = """{"name": "public"}""";
  var nested = """"pode conter """ dentro"""";
O TextScan actual lida com """ (3 quotes). Nao lida com 4+.
StringDelimiter.Open/Close sao fixos — nao suportam contagem
dinamica. Fix: scanner conta " consecutivos e matcha mesmo numero.

### Python 3.12 f-strings aninhadas (afecta linguagem actual)
Python 3.12 permite nesting recursivo:
  msg = f"result: {f'{value:{".2f"}}'}"
O scanner actual nao lida com nesting de quotes dentro de {}
recursivamente. Fix: scan recursivo para f-string expressions
(similar ao Core TokenizeLine).

## Impacto

- Adicionar nova linguagem: ~2 horas (config + JSONs) em vez de ~2 semanas (parser)
- Suporte a dezenas de linguagens com apenas keywords: viavel em 1 semana
- Performance 0-1ms para todas as linguagens sem tradu
- Zero dependencia externa (sem subprocess, sem parser, sem runtime adicional)
- Linguagens com construtos complexos (JS template literals, Rust lifetimes,
  Ruby heredocs) precisam de extensoes ao scanner — documentadas acima
