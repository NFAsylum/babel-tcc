# Babel — IntelliJ Platform extension

Adaptador do **Babel** (código com keywords traduzidas pro idioma nativo) pra IntelliJ
Platform. Uma extension, **9 IDEs**: Rider, IntelliJ IDEA, PyCharm, WebStorm, PhpStorm,
GoLand, RubyMine, CLion e DataGrip.

O arquivo no disco fica **sempre no idioma original** (C#/Python puro); a tradução é
puramente visual no editor (DT-003). Compiladores, git e outras ferramentas continuam
funcionando sem enxergar a tradução.

## Arquitetura

```
Editor (view traduzida)  ──►  TranslationService  ──►  CoreBridge  ──►  Core.Host (C#/.NET)
        ▲                                                                       │
        └──────────────── LightVirtualFile (in-memory) ◄────── JSON Lines (stdin/stdout)
```

Reusa o motor de tradução C# (`packages/core/MultiLingualCode.Core.Host`) **verbatim**, via
subprocess trocando JSON Lines (DT-002) — o mesmo protocolo da extension VS Code. Nada de
tradução é reimplementado em Kotlin.

### Módulos (MVP)

| Camada | Classe | Papel |
|---|---|---|
| Bridge | `services/CoreBridge` | Subprocess Core.Host + protocolo JSON Lines (queue serial, timeout, restart) |
| Lógica | `services/TranslationService` | `toDisplay` / `toDisk` (fail-open) |
| Estado | `services/LanguageService` | Idioma ativo + enabled (hot path) |
| View | `providers/VirtualDocumentProvider` | Abre `.cs`/`.py` como `LightVirtualFile` traduzido |
| Save | `providers/SaveHandler` | Reverse-translate no save → disco em inglês |
| Highlight+Hover | `highlighting/BabelAnnotator` | Cor de keyword no tema + tooltip do original |
| Hover (Ctrl+Q) | `providers/HoverProvider` | Quick doc com keyword original |
| Completion | `completion/BabelCompletionContributor` | Autocomplete de keywords traduzidas |
| Status bar | `statusbar/BabelStatusBarWidget` | Idioma ativo; menu de controles (enable, readonly, idioma, overrides por extensão, show original) |
| Auto | `services/AutoTranslateManager` | Re-traduz abas abertas ao trocar idioma |
| Settings | `settings/BabelSettings` (+ Configurable) | Estado persistente (`babel.xml`) |
| Ação | `actions/SelectLanguageAction` | Command palette: trocar idioma |

## Requisitos

- JDK 17 (`JAVA_HOME` apontando pra um JDK 17).
- Runtime **.NET 8** + binário `MultiLingualCode.Core.Host` (nativo self-contained ou `.dll`
  via `dotnet`) pra tradução real. Sem ele o plugin degrada com segurança: mostra o código
  original (fail-open), não crasha.

## Build & dev

```bash
cd packages/ide-adapters/intellij

# compila + roda testes (headless, não precisa de .NET nem display)
./gradlew test

# gera o .zip distribuível do plugin
./gradlew buildPlugin        # → build/distributions/babel-intellij-<versão>.zip

# sobe uma IDE Community sandbox com o plugin carregado (precisa de display)
./gradlew runIde
```

> A primeira execução baixa o IntelliJ Platform SDK (~sandbox IDE) — pode levar 10–30 min.
> O Gradle wrapper (`./gradlew`) já está commitado; não precisa de Gradle instalado no sistema.

### Configurar o Core.Host

Aponte o binário/`.dll` em **Preferences → Babel → Custom Core.Host path**, ou deixe em branco
pra usar o binário empacotado em `bin/` do plugin (empacotamento via `prepareSandbox` fica pra
uma tarefa de release — ver blueprint).

## Status: MVP vs paridade completa

**Pronto (MVP + paridade VS Code, este diretório):** carregamento do plugin, CoreBridge,
virtual document, reverse translation no save, settings persistentes, action de trocar idioma,
syntax highlighting das keywords traduzidas, hover tooltip (mouse e Ctrl+Q), completion de
keywords, status bar com menu de controles (enable/readonly/idioma/overrides por extensão/show
original), views read-only opcionais, overrides de idioma por linguagem, e re-translate ao vivo
ao trocar idioma.
Cobertura: **11 arquivos de teste, 42 testes** (unitários + integração headless via
`BasePlatformTestCase`).

**Verificação:** tudo que não exige GUI é testado programaticamente (`./gradlew test`), incluindo
o grafo de serviços real declarado no `plugin.xml`. Os critérios visuais (swap do editor, tooltip
renderizado, tradução ao vivo com Core.Host real) exigem `runIde` num ambiente com display + .NET.

**Não-MVP (blueprint pra features futuras):** diagnostics de `// tradu` faltante, rename
tradu-aware (determinístico via Core.Host), ícones no explorer. Tudo determinístico/offline.
Especificação executável em [`BLUEPRINT.md`](./BLUEPRINT.md).

## Known limitations

### Syntax highlighting in Rider (C#)

Rider uses ReSharper's project-aware C# highlighting engine, which requires files to be
attached to a `.csproj` — the `LightVirtualFile` used to render the translated view is
not project-attached, so ReSharper does not run analysis on it. As a result:

- **Keywords are colored** (via Babel's own annotator)
- **Identifiers, strings, comments and other tokens are NOT colored** in the translated view

The file on disk keeps normal Rider highlighting; only the translated view is affected.

IntelliJ IDEA and PyCharm use grammar-based highlighting for `.java` / `.py` and are
expected to render the translated view with full syntax coloring. (The VS Code extension
uses TextMate grammars, which is why it renders full coloring in the translated view.)

Workarounds attempted / considered:
- Attaching `LightVirtualFile` to `ProjectFileIndex`: high research cost, no guaranteed fix
- Editor decorations instead of virtual file: architectural rewrite (15-25h)

Neither is scheduled — trade-off accepted for MVP.

### Save model

The translated view is a non-physical `LightVirtualFile`. IntelliJ never marks such a file's
document as "unsaved", so the per-document save path (`beforeDocumentSaving` / `setBinaryContent`)
never fires for it — which is why an earlier build appeared to "not save". Instead, edits are
persisted through `FileDocumentManagerListener.beforeAllDocumentsSaving`, which the platform
invokes on **Ctrl+S / Save All and on autosave** (frame deactivation, running, VCS ops, etc.).
At that point every open translated view is reverse-translated and written to its original disk
file. In practice edits reach disk before you compile, commit or switch away — but note the
trigger is the global save/autosave, not a per-tab save event.

## Testes

```bash
./gradlew test        # 42 testes; relatório em build/reports/tests/test/index.html
```

Os testes fakeiam o subprocesso Core.Host (transport in-memory), então não precisam de .NET.
Os testes de integração bootam a plataforma IntelliJ headless pra validar o registro real dos
serviços e o mecanismo de `LightVirtualFile`.
