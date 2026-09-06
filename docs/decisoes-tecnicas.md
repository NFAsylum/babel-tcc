# Decisões Técnicas

Registro das decisões técnicas tomadas no projeto e suas justificativas.

---

## DT-001: Roslyn para parsing de C#

**Decisão:** Usar Microsoft.CodeAnalysis (Roslyn) para parsear código C#.

**Alternativas consideradas:**
- Parser customizado (regex/tokenização manual)
- ANTLR com gramática C#
- Tree-sitter

**Justificativa:**
- Roslyn é o parser oficial da Microsoft para C#
- Suporte completo a todas as versões de C#
- AST precisa e detalhada
- Semântica além de sintaxe (resolução de tipos, etc.)
- Bem documentado e mantido

**Tradeoff:** Dependência pesada (~16 MB no publish com Roslyn), mas justificada pela qualidade.

---

## DT-002: Comunicação TS <-> C# via processo/JSON

**Decisão:** A extensão TypeScript comunica com o Core C# via spawn de processo .NET, trocando mensagens JSON via stdin/stdout.

**Alternativas consideradas:**
- WebAssembly (compilar C# para WASM)
- HTTP server local
- Named pipes
- gRPC

**Justificativa:**
- stdin/stdout é o método mais simples e portável
- Não precisa de porta de rede (evita conflitos)
- Funciona em todos os SO
- JSON é fácil de debugar
- Mesma abordagem usada por Language Servers (LSP)

**Tradeoff:** Overhead de spawn de processo; mitigado mantendo processo vivo.

---

## DT-003: Arquivo no disco sempre em linguagem original

**Decisão:** O arquivo `.cs` no disco contém sempre C# puro. A tradução é puramente visual no editor.

**Justificativa:**
- Compiladores e ferramentas funcionam sem modificação
- Git diff mostra código real
- CI/CD funciona normalmente
- Não quebra IntelliSense e outras extensões
- Múltiplos devs podem ver idiomas diferentes do mesmo arquivo

**Tradeoff:** Complexidade de sincronizar edições traduzidas com arquivo original.

---

## DT-004: Abordagem híbrida para repositórios

**Decisão:** Monorepo para Core + Extension, repositório separado para traduções.

**Justificativa:**
- Core e Extension estão fortemente acoplados (versão única)
- Traduções podem ser contribuídas independentemente
- Contribuidores de tradução não precisam clonar o Core
- Versionamento independente das traduções

---

## DT-005: Sistema de IDs numéricos para keywords

**Decisão:** Keywords são mapeadas para IDs numéricos (`"if" -> 30`), e traduções mapeiam IDs para texto (`"30" -> "se"`).

**Alternativas consideradas:**
- Mapeamento direto keyword -> tradução
- Enum no código C#

**Justificativa:**
- Desacopla linguagem de programação da tradução
- Permite adicionar idiomas sem modificar tabela de keywords
- IDs são estáveis; textos podem ser corrigidos
- Facilita validação de completude

---

## DT-006: Anotação "tradu" para identificadores customizados

**Decisão:** Desenvolvedores anotam identificadores com `// tradu[lang]:nomeTraduzido` no próprio código (ex.: `// tradu[pt-br]:Calculadora`). Mapeamento de parâmetros usa vírgula (`// tradu[pt-br]:Somar,a:primeiro,b:segundo`).

**Alternativas consideradas:**
- Arquivo de mapeamento externo apenas
- AI para sugerir traduções automaticamente
- Convenção de nomes

**Justificativa:**
- Tradução fica próxima do código (fácil de manter)
- Desenvolvedor controla a tradução exata
- Funciona como documentação inline

**Atualização (2026-05):** Dois ajustes em relação ao design original:
- A sintaxe ganhou o prefixo de idioma `[lang]`, permitindo vários idiomas no mesmo arquivo (ex.: `// tradu[pt-br]:Calculadora|[es]:Calculadora`). O prefixo antigo `// tradu:` (sem idioma) não é mais usado.
- **(2026-08)** A sintaxe ganhou a forma explícita `origem=traducao` (ex.: `// tradu[pt-br]:kind=tipo`). Antes o alvo era sempre o **primeiro identificador da linha**, o que erra em toda linha com mais de um identificador — em C#, a declaração vem depois do tipo, então `protected readonly ShapeKind kind; // tradu[pt-br]:tipo` renomeava o tipo e não o campo. Duas anotações que adivinhavam o mesmo alvo também se sobrescreviam: dois `using System.*` anotados resultavam num único mapeamento para `System`. A forma simples continua válida e a adivinhação só acontece quando a origem não é declarada. O separador é `=` porque `:` já separa parâmetro de tradução depois da vírgula, e reusá-lo tornaria `Somar,a:primeiro` ambíguo entre mapeamento de parâmetro e par origem/tradução.
- A persistência em disco do `identifier-map.json` durante a tradução automática foi removida (tarefa 078) por ser destrutiva: `ApplyTraduAnnotations` fazia `Clear()` + `SaveMap()`, então traduzir um segundo arquivo apagava do disco os mapeamentos do primeiro. Hoje o mapa é reconstruído em memória a partir das anotações do arquivo atual a cada tradução (annotation-driven). A tradução de identificadores só ocorre quando o arquivo contém `tradu` (caso contrário usa Text Scan, keyword-only); não há mapa de identificadores global ou persistente sem `tradu`.

---

## DT-007: MVP focado em C# + PT-BR (expandido para Python + 10 idiomas)

**Decisão original:** MVP suporta apenas C# como linguagem de programação e PT-BR como idioma alvo.

**Justificativa original:**
- Reduz escopo para entrega viável no prazo do TCC
- C# tem o melhor parser (Roslyn)
- PT-BR é o idioma da equipe
- Arquitetura permite adicionar outros facilmente depois

**Atualização (2026-04):** Python adicionado como segunda linguagem (tarefas 052-060) usando tokenizador nativo via subprocesso. Traduções expandidas para 10 idiomas naturais (pt-br, pt-br-ascii, en-us, es-es, fr-fr, de-de, it-it, ja-jp-romaji, zh-cn, ar-sa).

---

## DT-008: Text Scan como fast path para tradução de keywords

**Decisão:** Usar scan linear de texto (TextScanTranslator) para traduzir
keywords em arquivos sem anotações tradu. Roslyn é usado apenas quando o
arquivo contém `tradu` (precisa da AST para identifiers).

**Alternativas avaliadas (tarefa 061):**
- Incremental Reparse (Roslyn WithChangedText): 1x speedup — gargalo não é o parse
- Cache por Bloco (hash): 270x — mas desnecessário com Text Scan a 0-1ms
- Tradução Lazy (viewport): complexidade muito alta para ganho que o Text Scan resolve

**Justificativa:**
- Benchmark real no pipeline integrado (mesma API TranslateToNaturalLanguageAsync):
  - Sem tradu (Text Scan): 0-1ms para 17.000 linhas
  - Com tradu (Roslyn): 35-4077ms para 17.000 linhas
  - Speedup: 35-4077x
- 51 edge cases testados (strings, comments, preprocessor, raw strings C# 11): 51/51 PASS
- Equivalência com output Roslyn: 10/11 MATCH (1 mismatch aceitável em `#if` disabled region)
- Fallback automático para Roslyn quando keyword map não disponível
- Zero regressões: 566 testes passando após integração

**Aplicabilidade a Python:**
O PythonAdapter.ReverseSubstituteKeywords já implementa o mesmo padrão
(scan linear, skip #comments e strings Python). A otimização pode ser
aplicada para forward translation em Python com o mesmo approach —
substituir o tokenizer subprocess por Text Scan para arquivos sem tradu.

**Detecção:**
```csharp
bool needsRoslyn = sourceCode.Contains("tradu");
```
Se o arquivo contém "tradu", usa Roslyn (AST completo para identifiers).
Caso contrário, usa Text Scan (O(n) linear, 0-1ms).

---

## DT-009: Suporte a dois dialetos Portugol (VisuAlg + Portugol Studio)

**Decisão:** Adicionar dois adapters distintos para a família Portugol:
- `VisuAlgAdapter` (`.alg`, 48 keywords, dialeto de Claudio Morgado, case-insensitive)
- `PortugolStudioAdapter` (`.por`, 26 keywords, dialeto UNIVALI, case-sensitive)

**Alternativas consideradas:**
- Suportar apenas VisuAlg (maior alcance no ensino médio brasileiro)
- Suportar apenas Portugol Studio (mais usado em graduação)
- Suportar Portugol "didático" genérico (sem implementação canônica — inviável)

**Justificativa:**
- VisuAlg domina ensino médio/técnico; Portugol Studio domina graduação — atender ambos cobre
  toda a pipeline brasileira de ensino de algoritmos
- As duas variantes são do mesmo conceito mas com sintaxes divergentes (`inicio`/`fim` vs `{`/`}`),
  o que evidencia a generalidade da arquitetura de adapters: o mesmo engine traduz dois dialetos
  próximos sem ambiguidade
- Extensões de arquivo distintas (`.alg`, `.por`) evitam colisão na resolução via `LanguageRegistry`
- Ambos são implementados em modo "keyword-only" (sem tradu): a interface `ITextScannable` +
  `LanguageScanRules` permite operar pelo fast path do Text Scan, sem necessidade de parser real
- Reuso do helper `PortugolScanner` para reverse substitution evita duplicar a máquina de estados
  de skip de strings/comentários entre os dois adapters

**Impactos:**
- Adicionado campo `LanguageScanRules.CaseInsensitiveKeywords` (default `false`) para suportar
  VisuAlg sem regressão em C#/Python
- `TextScanTranslator.BuildTranslationMap` agora aceita o flag e cria o mapa com
  `StringComparer.OrdinalIgnoreCase` quando solicitado
- IDs numéricos são por-PL (cada dialeto começa em 0). Não há alinhamento intencional de IDs
  semanticamente equivalentes entre dialetos: a infraestrutura de tradução trata cada PL como
  namespace independente, e tentativas de compartilhar IDs entre dialetos com sintaxes divergentes
  agregam complexidade sem benefício operacional

**Tradeoffs:**
- Sem suporte a tradu annotations para esses dialetos. Aceitável: o público-alvo (alunos
  iniciantes) raramente usa identifier renaming, e a ausência de um parser real torna esse recurso
  desproporcional
- Diretório de keywords-base usa `portugolstudio/` (sem hífen) por conta de `Path.Combine` com
  `LanguageName.ToLowerInvariant()`. Decisão deliberada para não refatorar o `NaturalLanguageProvider`

---

## DT-010: Distribuição dual-track (self-contained por plataforma + universal)

**Decisão:** Distribuir a extensão em dois formatos simultâneos, deixando o cliente VS Code
escolher o pacote certo:
- Um `.vsix` **self-contained por plataforma** (`vsce package --target <rid>`), com o runtime
  .NET embutido. Sem necessidade de instalar .NET.
- Um `.vsix` **universal** (sem `--target`, framework-dependent), que depende do .NET 8 instalado.

O VS Code instala automaticamente o pacote da plataforma do usuário quando existe; caso contrário,
cai no universal. O runtime Python continua sendo dependência externa **apenas** para arquivos
`.py` (resolução lazy, ver Impactos).

**Alternativas consideradas:**
- Manter só framework-dependent (atual): menor `.vsix` (~5 MB), mas exige instalar .NET 8 — barreira
  alta para o público educacional (iniciantes, máquinas de laboratório sem permissão de instalação)
- Só self-contained por plataforma: zero barreira, mas perde portabilidade para RIDs não cobertos
- NativeAOT: menor binário nativo, mas o Roslyn não é AOT-friendly (reflection pesada) — inviável
- WebAssembly: eliminaria o runtime, mas já rejeitado em DT-002 e Roslyn-on-WASM é impraticável

**Justificativa:**
- A barreira de instalação do .NET é o gargalo universal: o core engine é C#, então C#, Python,
  VisuAlg e Portugol todos dependem do runtime .NET. Reduzir essa barreira beneficia todos os
  usuários e ataca diretamente o paradoxo da ferramenta educacional (quem mais ganha é quem menos
  tem ambiente pronto)
- O dual-track é o melhor dos dois mundos: zero barreira nas plataformas comuns (Win/Mac/Linux,
  x64 e arm64) e portabilidade preservada via fallback universal
- É padrão estabelecido para extensões que embutem binário nativo (ex.: a extensão oficial de C#
  da Microsoft)

**Números medidos (spike, .NET 8, projeto Host):**
- Framework-dependent: 16 MB descompactado, ~5,0 MB comprimido (`.vsix` atual ~5,1 MB)
- Self-contained linux-x64: 87 MB descompactado, ~35,6 MB comprimido (~36 MB por plataforma)
- O usuário baixa apenas o pacote da sua plataforma

**Impactos:**
- `release.yml` passa de 1 pacote para uma matrix (~6 alvos self-contained + 1 universal por release)
- `publish-core` precisa de variante self-contained com `-r <rid> --self-contained true`
- Armazenamento/upload maior no registry (download por usuário continua sendo um só)
- Cada bump de versão gera N pacotes
- Python: nenhuma mudança de código necessária. A resolução do Python já é lazy — só é disparada ao
  tokenizar um `.py` (`PythonTokenizerService` só resolve/spawna dentro de `Tokenize()`), e a falta
  de Python retorna `OperationResult.Fail` com mensagem útil, sem crashar. C#/VisuAlg/Portugol nunca
  tocam o Python. Falta apenas comunicar isso no README ("Python opcional, só para `.py`")

**Tradeoffs:**
- Pipeline de release mais complexo (matrix de plataformas) em troca de zero barreira de instalação
- `.vsix` por plataforma ~7x maior, aceitável porque o download por usuário continua único

**Implementação (tarefa 105):**
- O ponto crítico não é o YAML, e sim o launch do Core. O `CoreBridge.resolveLaunch()` detecta o
  formato: se o executável nativo (`MultiLingualCode.Core.Host`, ou `.exe` no Windows) existe em
  `bin/`, roda-o direto; senão, cai para `dotnet MultiLingualCode.Core.Host.dll` (universal). No
  Linux/Mac reforça o bit de execução (`chmod 0o755`): o `.vsix` preserva a permissão, mas a
  extração do VS Code ao instalar nem sempre a mantém.
- Para a distinção ficar inequívoca, o pacote universal é publicado com `-p:UseAppHost=false` (gera
  só a `.dll`, sem apphost nativo), forçando o caminho `dotnet`. O self-contained sempre gera o
  apphost nativo, que tem precedência na detecção.
- O `release.yml` cross-publica todos os RIDs a partir de um único runner Linux (o `dotnet publish`
  faz cross-compile self-contained), então a matrix não precisa de runners Windows/macOS.
- Validação automatizada: o job `smoke-self-contained` roda o binário linux-x64 dentro do container
  `mcr.microsoft.com/dotnet/runtime-deps:8.0` **sem .NET** e confirma que ele traduz, provando a
  independência do runtime. Essa imagem traz as dependências nativas de um app self-contained (ICU
  inclusa) **sem** o runtime .NET e sem depender de `apt-get`, então é determinística (sem rede) e é
  o baseline canônico do alvo. Cobre só linux-x64 (os demais RIDs não têm container/runner trivial),
  mas é o sinal que o CI antes não pegava.
- **Dependência de ICU:** "self-contained" elimina a dependência do **runtime .NET**, não de toda
  biblioteca nativa — o binário ainda exige a **ICU** do sistema (sem ela, falha com
  `Couldn't find a valid ICU package`). Na prática isso é transparente: Windows (10 1903+), macOS e a
  maioria das distros Linux já trazem ICU; só ambientes minimalistas (containers slim, alguns
  servidores headless) precisariam instalá-la. Tornar o pacote 100% sem ICU exigiria
  `InvariantGlobalization=true`, que altera o comportamento de cultura (casing/ordenação) e fica fora
  do escopo desta tarefa.
- **macOS Gatekeeper:** o binário nativo não é assinado/notarizado; no macOS o usuário pode precisar
  liberar a execução na primeira vez (Ajustes > Privacidade e Segurança). Assinatura fica fora do
  escopo desta tarefa.

---

## DT-011: Troca de idioma recarrega a visão no lugar (via `revert`)

**Decisão:** Manter **uma única URI** por arquivo (`babel-tcc-translated:/caminho/arquivo.cs`) e, ao
trocar de idioma, recarregar o conteúdo **no lugar**, sem fechar aba: limpa-se o cache do caminho e
reverte-se o editor aberto (`workbench.action.files.revert`). O `revert` força o VS Code a reler o
`readFile` do provider — que devolve o novo idioma — e mantém o documento **limpo**. Nenhuma aba é
fechada ou reaberta; o foco volta para a visão que estava ativa.

**Problema:** Trocar de idioma não atualizava o arquivo aberto — ele só mudava para o novo idioma após
um **save manual**, e as **cores quebravam** antes disso (o mapa de palavras-chave já apontava para o
novo idioma, mas o texto exibido continuava no antigo). A primeira hipótese foi o `stat()`: ele devolvia
`size: originalStat.size` — o tamanho do **arquivo original**, que não muda entre idiomas e não bate com
o conteúdo traduzido. Corrigir o `stat()` para devolver `size`/`mtime` do conteúdo traduzido é
**necessário** (o contrato do `onDidChangeFile` exige metadados corretos), mas **não é suficiente**:

> "It is important that the metadata of the file that changed provides an updated `mtime` that advanced
> from the previous value in the stat and a correct `size` value. Otherwise there may be optimizations
> in place that will not show the change in an editor."

O contrato diz apenas que o editor **"pode"** recarregar — e, na prática, um editor aberto sobre um
`FileSystemProvider` **não recarrega de forma confiável** a partir de um evento `Changed`: a própria
equipe do VS Code marca esse caminho como não investigado (microsoft/vscode#110854) e o sintoma típico é
"o editor só atualiza depois que perde e recupera o foco". Por isso o disparo passivo de `onDidChangeFile`
deixava a visão presa no idioma antigo até um save manual.

**Solução:** Em vez de depender do recarregamento passivo, recarrega-se **explicitamente** com `revert`.
O `TextFileEditorModel.revert()` chama `forceResolveFromFile()` **independentemente de o documento estar
sujo** (só `options.soft` pula a releitura) — então até uma visão limpa relê o `readFile` e passa a
exibir o novo idioma, continuando limpa. Isso resolve os dois sintomas: o texto recarrega e, como a
versão do documento muda, os tokens semânticos são recalculados sobre o texto novo, com o mapa do novo
idioma — as cores voltam a casar.

**Alternativas consideradas:**
- **Confiar no `onDidChangeFile` + `stat` correto** (tentado primeiro): não recarrega de forma confiável
  o editor aberto (acima). O `stat()` corrigido foi mantido — o `revert` relê via `readFile`/`stat`, e o
  recarregamento passivo, quando dispara, vira um reforço inofensivo.
- **Codificar o idioma na query da URI** (`?lang=pt-br`): faz cada idioma ser um documento distinto e
  **força** fechar a aba antiga e abrir a nova; na prática deixava as duas abas abertas ("multi-view").
  Rejeitada.
- **`applyEdit` para sobrescrever o conteúdo**: atualiza a visão de forma confiável, mas deixa o
  documento **sujo** (indicador de não salvo a cada troca, diálogo de edições na troca seguinte). O
  `revert` dá a mesma confiabilidade mantendo o documento limpo. Rejeitada.
- **Fechar e reabrir a aba**: frágil — `tabGroups.close` com referência guardada falha em certos
  contextos (microsoft/vscode#242867) e reabrir a mesma URI pode reusar o modelo antigo em cache.
  Rejeitada.

**Justificativa:** Recarregar no lugar via `revert` não fecha aba nenhuma — resolve os sintomas de uma
vez: o texto recarrega (releitura forçada do `readFile`), as cores casam (re-tokenização sobre o texto
novo), o documento fica limpo e o foco volta para a visão ativa. `Uri.file(uri.path)` continua mapeando
para o arquivo original, então a tradução reversa ao salvar segue acertando o original.

**Save linear (anti-corrupção) — "Salvar e trocar" gravava o original no idioma traduzido:** Com
"Salvar e trocar" e o arquivo modificado, a **1ª vez** funcionava mas a **2ª corrompia** — o `.cs`
salvo saía no idioma traduzido (código inválido). Duas causas, ambas de trabalho **adiado** (não é
multi-thread; o extension host é single-thread):

1. O `doWriteFile` re-renderizava o buffer via `setTimeout(100ms)` após salvar. Esse timer disparava
   **depois** do `revert` da troca e virava o buffer de volta ao idioma antigo, dessincronizando o
   **conteúdo do buffer** do `displayLanguages`. O save seguinte então revertia com `sourceLanguage`
   errado: `ApplyTranslatedEdits` (diff de 3 vias, `TranslationOrchestrator.cs`) chama
   `ReverseTranslateLine`, que só reverte tokens presentes no mapa de `sourceLanguage`; com o idioma
   errado os tokens ficam **como estão** (traduzidos) → o original recebe o texto traduzido.
2. O baseline do diff de 3 vias (`previousTranslated`) vinha do `cache`, que a troca **esvazia** — um
   baseline vazio faz o merge despejar todo o conteúdo traduzido no original.

Correção: o save é **100% linear** (`reverter → escrever original → atualizar estado`, sem nenhum
`setTimeout`); o buffer fica como o usuário digitou (sem re-render concorrente); e o baseline passa a
vir de um mapa dedicado **`renderedContent`** (o texto exato que o `readFile` colocou na tela, ou o que
o save anterior deixou), **imune ao `invalidatePath`**. Assim `sourceLanguage` sempre bate com o idioma
do buffer e o baseline nunca esvazia. A única re-renderização que existe é o `revert` da troca — um
único `await`, sem timer.

**Padrão seguro (uma visão por arquivo):** mantida — `handleActiveEditorChange` só abre a visão
traduzida se nenhuma já estiver aberta para aquele caminho (`isAnyTranslatedTabOpenForPath`).

**Tradeoffs:**
- O `revert` precisa que a visão esteja em foco (ele age no editor ativo), então uma troca com várias
  visões abertas pisca o foco entre elas antes de restaurar a ativa. Caso comum (uma visão) é
  transparente.
- Uma visão com **edições não salvas** é **pulada** no `revert` (reverter descartaria as edições). Pela
  via do comando isso não acontece (as edições já foram tratadas antes da troca); pela via do
  `settings.json` a visão suja mantém o idioma antigo até o próximo save, que reverte pelo idioma exibido.
- `stat()` chama `provideContent` para medir o tamanho do conteúdo traduzido; é barato porque o resultado
  fica em cache (e o `readFile` reaproveita o mesmo cache).
- O toggle **editável ↔ readonly** continua reabrindo a aba, porque o "ser readonly" está atrelado ao
  esquema da URI (dois providers registrados). É um fluxo separado e raro; mesmo assim ele fecha a aba
  antiga por **busca fresca** (`closeTab(uri)`), não por referência guardada.

**Implementação (tarefa 111):**
- `translatedContentProvider.ts`: `stat()` devolve `size` do conteúdo traduzido + `mtime` que avança;
  `invalidatePath` limpa o cache do arquivo (todos os idiomas) e dispara `onDidChangeFile` para as
  visões abertas. O `readFile` registra, por caminho, o **idioma exibido** (`displayLanguages`) e o
  **texto renderizado** (`renderedContent`) — ambos só aqui, não em `provideContent` (que o `stat()`
  também chama e sobrescreveria). O `doWriteFile` é linear: reverte usando `sourceLanguage =`
  idioma exibido e `previousTranslated =` `renderedContent` (baseline imune ao cache), escreve o
  original, descarta os idiomas em cache do arquivo e grava cache/baseline = o texto salvo. **Sem
  `setTimeout`, sem re-render concorrente, sem `freshTranslation`.**
- **Edições não salvas são tratadas ANTES da troca**, não depois. O comando `selectLanguage` chama
  `confirmUnsavedEditsBeforeLanguageChange(linguagemAfetada?)` enquanto o idioma atual ainda vale:
  "Salvar" faz um save normal (reverte no idioma correto), "Descartar" reverte o buffer, "Cancelar"
  aborta a troca. Salvar *depois* que a config já mudou descartava as edições — por isso salvar primeiro
  e só então trocar. O prompt é **escopado pela linguagem afetada**: numa troca per-language (ex.: só
  CSharp) só entram visões sujas daquela linguagem — mudar CSharp não promove/descarta uma visão Python
  suja não relacionada; numa troca global, entram todas.
- `autoTranslateManager.ts`: `refreshTranslatedTabs` percorre as visões traduzidas abertas e chama
  `reloadTranslatedView(uri, caminho)` — que limpa o cache (`invalidatePath`) e, se a visão estiver
  aberta e **limpa**, dá foco e executa `workbench.action.files.revert`; ao final restaura o foco da
  visão que estava ativa. `handleActiveEditorChange` mantém uma visão por arquivo; fechamentos de aba
  (toggle on/off e readonly) usam `closeTab(uri)` por busca fresca.
- `extension.ts`: o comando `selectLanguage` confirma edições não salvas (escopadas) antes de mudar a
  config; o file-watcher suprime o evento da **própria escrita por janela de timestamp**
  (`recentWrites` + `isRecentSelfWrite`, ~1s) e, fora da janela (mudança externa), chama
  `invalidatePath`. A janela substituiu um flag "consome-uma-vez": um save pode emitir vários eventos
  coalescidos ou nenhum (escrita no-op/falha), e o flag vazava (suprimindo uma mudança externa futura
  para sempre) ou sub-suprimia; a janela se auto-expira e cobre o burst.

**Limitação conhecida (follow-up):** uma mudança **externa** do arquivo original (git checkout,
formatador de fora) com a visão traduzida aberta dispara `invalidatePath` (limpa o cache), mas a visão
só recarrega de forma **confiável** via `revert` — que hoje só roda na troca de idioma, não no
file-watcher. Acionar `revert` no watcher esbarra no timing do `onDidChangeFile`, que dispara **antes**
do conteúdo estar atualizado (microsoft/vscode#72831), então o re-`readFile` pode ler conteúdo velho.
Fica como follow-up; o caso (editar o original por fora com a visão traduzida aberta) é incomum.

**Referências:**
- [FileSystemProvider.onDidChangeFile — contrato de `mtime`/`size`](https://vshaxe.github.io/vscode-extern/vscode/FileSystemProvider.html)
- [microsoft/vscode#110854 — recarregamento de editores abertos sobre `FileSystemProvider` (não confiável)](https://github.com/microsoft/vscode/issues/110854)
- [microsoft/vscode#242867 — `tabGroups.close` ineficaz com referência guardada](https://github.com/microsoft/vscode/issues/242867)
- [microsoft/vscode#72831 — `FileSystemWatcher.onDidChange` dispara antes do conteúdo estar atualizado](https://github.com/microsoft/vscode/issues/72831)

---

## DT-012: Descoberta declarativa por context keys

**Decisão:** A visibilidade de todo item de UI da extensão é declarada no manifesto, por cláusula
`when` sobre context keys que a extensão publica (`babelTcc.enabled`, `babelTcc.supportedFile`,
`babelTcc.translatedView`, `babelTcc.readonlyView`). As chaves ficam num único módulo,
`src/ui/contextKeys.ts`, com uma única função de recálculo (`refresh`) disparada por mudança de
editor ativo e por mudança de configuração. Nenhuma superfície nova é uma webview.

**Problema:** A extensão contribuía 5 comandos e **nenhuma** outra superfície: sem
`contributes.menus`, sem `contributes.keybindings`, sem nenhuma chamada a `setContext`. Quem não
soubesse de antemão que a extensão existe e como ela se chama não tinha como acioná-la — só pelo
Command Palette, digitando o nome certo. Para o público-alvo (estudante iniciante, professor
demonstrando em aula) isso equivale a funcionalidade inexistente.

Junto vinha um segundo problema: os títulos dos 5 comandos carregavam o prefixo `"Babel TCC: "` no
próprio `title`. O VS Code compõe esse prefixo automaticamente a partir de `category`, e **só** no
Command Palette; dentro de um menu de contexto o prefixo é ruído, porque o contexto já é óbvio.

**Alternativas consideradas:**
- **Checagem em runtime dentro do comando** (o que já existe): o item de menu apareceria sempre e
  responderia "tipo de arquivo não suportado" *depois* do clique. É a falha de UX que a etapa veio
  corrigir: o usuário descobre que a ação não servia só depois de tentar. Rejeitada como mecanismo
  de descoberta — mas **mantida como defesa**, porque o Command Palette continua expondo todos os
  comandos (INV-02) e portanto um comando ainda pode ser invocado num contexto inválido.
- **View container próprio** (ícone na Activity Bar com uma árvore): dá descoberta forte, mas ocupa
  espaço permanente na barra lateral por uma extensão que age sobre o editor ativo, não sobre uma
  coleção navegável. Custo de UI alto para o ganho.
- **Webview de onboarding:** daria controle visual total, mas traz bundle novo, ciclo de vida
  próprio, e uma superfície de acessibilidade que passaria a ser responsabilidade nossa (foco,
  navegação por teclado, contraste, leitor de tela) em vez de ser herdada do VS Code. Contraria
  INV-15. Rejeitada — o walkthrough nativo (`contributes.walkthroughs`, tarefa116) cobre o mesmo
  caso de uso com acessibilidade nativa.
- **Publicar as chaves de dentro de cada módulo que já observa o estado** (status bar, provider,
  autoTranslateManager): evitaria uma classe nova, mas espalharia a verdade sobre visibilidade por
  vários arquivos, que foi exatamente como o `description` do Marketplace ficou desatualizado por
  duas releases. Rejeitada em favor de uma fonte única.

**Justificativa:**
- O `when` é avaliado pelo próprio VS Code, antes de desenhar o item: um menu que não se aplica
  simplesmente não aparece, em vez de aparecer e falhar. É a diferença entre a extensão parecer
  quebrada e parecer atenta ao contexto.
- Uma fonte única (`contextKeys.ts`) com um único `refresh` significa que não existe estado de UI
  derivado em dois lugares que possam divergir.
- O custo real — manter as chaves sincronizadas com o estado — é mitigado por teste:
  `test/ui/contextKeys.test.ts` cobre o recálculo, e `test/manifest/manifest.test.ts` garante que
  todo item de menu tem `when` não vazio e que todo comando citado em `menus`/`keybindings` existe.

**Consequências:**
- Os nomes das chaves são **camelCase** (`babelTcc.*`), não `babel-tcc.*` como a seção de
  configuração: o hífen é lido como operador de subtração dentro de uma expressão `when`, então
  `babel-tcc.enabled` não funcionaria. É a única inconsistência de nomenclatura da extensão e existe
  por imposição da plataforma.
- `vscode.window.activeTextEditor` é `TextEditor | undefined` por definição da API. O nulo é
  recebido **só** em `refresh()` e normalizado ali para string vazia, sem propagar — mesmo
  tratamento que `docs/padroes-codigo.md` já aceita para as fronteiras com APIs Java no plugin
  IntelliJ.
- `publishKey` não aguarda o `setContext`: o VS Code reavalia as cláusulas `when` quando o valor
  chega, e bloquear o handler de evento do editor nisso só adicionaria latência.
- `editor/title` recebe **um** botão por vez ("abrir visão traduzida" quando o arquivo é suportado e
  a visão não está aberta; "mostrar original" quando já se está nela). Dois ícones concorrentes na
  barra do editor são poluição — e o teste de contrato do manifesto trava essa regra.
- Os keybindings usam chord com prefixo próprio `ctrl+alt+b` (`cmd+alt+b` no mac) em vez de
  combinações de uma tecla só. No Windows o `AltGr` do teclado ABNT2 chega às aplicações como
  `Ctrl+Alt`, então o prefixo foi verificado em teclado ABNT2 real (2026-08-16): não produz
  caractere. O teste de contrato rejeita as teclas que o ABNT2 alcança por AltGr.

**Implementação (tarefa 113):**
- `src/config/constants.ts`: `CONTEXT_KEYS` com os nomes das 4 chaves.
- `src/ui/contextKeys.ts`: `ContextKeyManager`, criado por static factory (`create`) — o construtor
  só declara propriedades, como manda `docs/padroes-codigo.md`. `create` assina
  `window.onDidChangeActiveTextEditor` e `configService.onDidChangeConfiguration`, e publica o
  primeiro valor das 4 chaves. `dispose()` libera as 2 subscrições.
- `src/extension.ts`: cria o manager e o registra em `context.subscriptions`.
- `package.json`: `category` e `icon` nos comandos, `shortTitle` nos 2 usados em `editor/title`,
  `contributes.menus` (`editor/title` e `editor/context`) e `contributes.keybindings`.
- `package.nls*.json` (7): título sem o prefixo `"Babel TCC: "`, chave nova `extension.category`.
- Testes: `test/ui/contextKeys.test.ts`, `test/manifest/manifest.test.ts` (contrato entre
  `package.json`, os 7 nls e `constants.ts`) e `test/l10n/bundles.test.ts` (contrato entre as
  chamadas `l10n.t()` e os 6 bundles de runtime).

**Referências:**
- [when clause contexts — operadores e context keys embutidas](https://code.visualstudio.com/api/references/when-clause-contexts)
- [contributes.menus — grupos e `navigation`](https://code.visualstudio.com/api/references/contribution-points#contributes.menus)
- [contributes.keybindings — chords e variantes por plataforma](https://code.visualstudio.com/api/references/contribution-points#contributes.keybindings)
