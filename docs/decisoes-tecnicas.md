# Decisoes Tecnicas

Registro das decisoes tecnicas tomadas no projeto e suas justificativas.

---

## DT-001: Roslyn para parsing de C#

**Decisao:** Usar Microsoft.CodeAnalysis (Roslyn) para parsear codigo C#.

**Alternativas consideradas:**
- Parser customizado (regex/tokenizacao manual)
- ANTLR com gramatica C#
- Tree-sitter

**Justificativa:**
- Roslyn e o parser oficial da Microsoft para C#
- Suporte completo a todas as versoes de C#
- AST precisa e detalhada
- Semantica alem de sintaxe (resolucao de tipos, etc.)
- Bem documentado e mantido

**Tradeoff:** Dependencia pesada (~16 MB no publish com Roslyn), mas justificada pela qualidade.

---

## DT-002: Comunicacao TS <-> C# via processo/JSON

**Decisao:** A extensao TypeScript comunica com o Core C# via spawn de processo .NET, trocando mensagens JSON via stdin/stdout.

**Alternativas consideradas:**
- WebAssembly (compilar C# para WASM)
- HTTP server local
- Named pipes
- gRPC

**Justificativa:**
- stdin/stdout e o metodo mais simples e portavel
- Nao precisa de porta de rede (evita conflitos)
- Funciona em todos os SO
- JSON e facil de debugar
- Mesma abordagem usada por Language Servers (LSP)

**Tradeoff:** Overhead de spawn de processo; mitigado mantendo processo vivo.

---

## DT-003: Arquivo no disco sempre em linguagem original

**Decisao:** O arquivo `.cs` no disco contem sempre C# puro. A traducao e puramente visual no editor.

**Justificativa:**
- Compiladores e ferramentas funcionam sem modificacao
- Git diff mostra codigo real
- CI/CD funciona normalmente
- Nao quebra IntelliSense e outras extensoes
- Multiplos devs podem ver idiomas diferentes do mesmo arquivo

**Tradeoff:** Complexidade de sincronizar edicoes traduzidas com arquivo original.

---

## DT-004: Abordagem hibrida para repositorios

**Decisao:** Monorepo para Core + Extension, repositorio separado para traducoes.

**Justificativa:**
- Core e Extension estao fortemente acoplados (versao unica)
- Traducoes podem ser contribuidas independentemente
- Contribuidores de traducao nao precisam clonar o Core
- Versionamento independente das traducoes

---

## DT-005: Sistema de IDs numericos para keywords

**Decisao:** Keywords sao mapeadas para IDs numericos (`"if" -> 30`), e traducoes mapeiam IDs para texto (`"30" -> "se"`).

**Alternativas consideradas:**
- Mapeamento direto keyword -> traducao
- Enum no codigo C#

**Justificativa:**
- Desacopla linguagem de programacao da traducao
- Permite adicionar idiomas sem modificar tabela de keywords
- IDs sao estaveis; textos podem ser corrigidos
- Facilita validacao de completude

---

## DT-006: Anotacao "tradu" para identificadores customizados

**Decisao:** Desenvolvedores anotam identificadores com `// tradu:nomeTraduzido` no proprio codigo.

**Alternativas consideradas:**
- Arquivo de mapeamento externo apenas
- AI para sugerir traducoes automaticamente
- Convencao de nomes

**Justificativa:**
- Traducao fica proxima do codigo (facil de manter)
- Desenvolvedor controla a traducao exata
- Funciona como documentacao inline
- Mapeamento externo (identifier-map.json) complementa para persistencia

---

## DT-007: MVP focado em C# + PT-BR (expandido para Python + 10 idiomas)

**Decisao original:** MVP suporta apenas C# como linguagem de programacao e PT-BR como idioma alvo.

**Justificativa original:**
- Reduz escopo para entrega viavel no prazo do TCC
- C# tem o melhor parser (Roslyn)
- PT-BR e o idioma da equipe
- Arquitetura permite adicionar outros facilmente depois

**Atualizacao (2026-04):** Python adicionado como segunda linguagem (tarefas 052-060) usando tokenizador nativo via subprocesso. Traduções expandidas para 10 idiomas naturais (pt-br, pt-br-ascii, en-us, es-es, fr-fr, de-de, it-it, ja-jp-romaji, zh-cn, ar-sa).

---

## DT-008: Text Scan como fast path para traducao de keywords

**Decisao:** Usar scan linear de texto (TextScanTranslator) para traduzir
keywords em arquivos sem anotacoes tradu. Roslyn e usado apenas quando o
arquivo contem `tradu` (precisa da AST para identifiers).

**Alternativas avaliadas (tarefa 061):**
- Incremental Reparse (Roslyn WithChangedText): 1x speedup — gargalo nao e o parse
- Cache por Bloco (hash): 270x — mas desnecessario com Text Scan a 0-1ms
- Traducao Lazy (viewport): complexidade muito alta para ganho que o Text Scan resolve

**Justificativa:**
- Benchmark real no pipeline integrado (mesma API TranslateToNaturalLanguageAsync):
  - Sem tradu (Text Scan): 0-1ms para 17.000 linhas
  - Com tradu (Roslyn): 35-4077ms para 17.000 linhas
  - Speedup: 35-4077x
- 51 edge cases testados (strings, comments, preprocessor, raw strings C# 11): 51/51 PASS
- Equivalencia com output Roslyn: 10/11 MATCH (1 mismatch aceitavel em `#if` disabled region)
- Fallback automatico para Roslyn quando keyword map nao disponivel
- Zero regressoes: 566 testes passando apos integracao

**Aplicabilidade a Python:**
O PythonAdapter.ReverseSubstituteKeywords ja implementa o mesmo padrao
(scan linear, skip #comments e strings Python). A otimizacao pode ser
aplicada para forward translation em Python com o mesmo approach —
substituir o tokenizer subprocess por Text Scan para arquivos sem tradu.

**Deteccao:**
```csharp
bool needsRoslyn = sourceCode.Contains("tradu");
```
Se o arquivo contem "tradu", usa Roslyn (AST completo para identifiers).
Caso contrario, usa Text Scan (O(n) linear, 0-1ms).
