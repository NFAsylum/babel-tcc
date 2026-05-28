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
  `confirmUnsavedEditsBeforeLanguageChange()` enquanto o idioma atual ainda vale: "Salvar" faz um save
  normal (reverte no idioma correto), "Descartar" reverte o buffer, "Cancelar" aborta a troca. Salvar
  *depois* que a config já mudou descartava as edições — por isso salvar primeiro e só então trocar.
- `autoTranslateManager.ts`: `refreshTranslatedTabs` percorre as visões traduzidas abertas e chama
  `reloadTranslatedView(uri, caminho)` — que limpa o cache (`invalidatePath`) e, se a visão estiver
  aberta e **limpa**, dá foco e executa `workbench.action.files.revert`; ao final restaura o foco da
  visão que estava ativa. `handleActiveEditorChange` mantém uma visão por arquivo; fechamentos de aba
  (toggle on/off e readonly) usam `closeTab(uri)` por busca fresca.
- `extension.ts`: o comando `selectLanguage` confirma edições não salvas antes de mudar a config; o
  file-watcher **consome** o evento da própria escrita (`writingPaths`: deleta e ignora — supressão
  determinística, sem timer) e, para mudanças externas, chama `invalidatePath`.

**Referências:**
- [FileSystemProvider.onDidChangeFile — contrato de `mtime`/`size`](https://vshaxe.github.io/vscode-extern/vscode/FileSystemProvider.html)
- [microsoft/vscode#110854 — recarregamento de editores abertos sobre `FileSystemProvider` (não confiável)](https://github.com/microsoft/vscode/issues/110854)
- [microsoft/vscode#242867 — `tabGroups.close` ineficaz com referência guardada](https://github.com/microsoft/vscode/issues/242867)
