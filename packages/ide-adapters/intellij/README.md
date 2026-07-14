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
| Hover | `providers/HoverProvider` | Tooltip com keyword original |
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

**Pronto (MVP, este diretório):** carregamento do plugin, CoreBridge, virtual document,
reverse translation no save, settings persistentes, action de trocar idioma, hover de keyword.
Cobertura: **6 arquivos de teste, 23 testes** (unitários + integração headless via
`BasePlatformTestCase`).

**Verificação:** tudo que não exige GUI é testado programaticamente (`./gradlew test`), incluindo
o grafo de serviços real declarado no `plugin.xml`. Os critérios visuais (swap do editor, tooltip
renderizado, tradução ao vivo com Core.Host real) exigem `runIde` num ambiente com display + .NET.

**Não-MVP (blueprint pro Qwen 30B):** syntax highlighting, completion, code actions, diagnostics,
rename tradu-aware, ícones no explorer, status bar, integração com `babel-services`. Especificação
executável em [`BLUEPRINT-QWEN.md`](./BLUEPRINT-QWEN.md).

## Testes

```bash
./gradlew test        # 23 testes; relatório em build/reports/tests/test/index.html
```

Os testes fakeiam o subprocesso Core.Host (transport in-memory), então não precisam de .NET.
Os testes de integração bootam a plataforma IntelliJ headless pra validar o registro real dos
serviços e o mecanismo de `LightVirtualFile`.
