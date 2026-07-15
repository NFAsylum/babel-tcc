# BLUEPRINT — Babel IntelliJ extension: features não-MVP (execução por Qwen 30B)

> **Público-alvo deste doc:** o modelo Qwen 30B que assume o trabalho depois da fase de MVP.
> A base MVP + a rodada de paridade com a extension VS Code (syntax highlighting, hover,
> completion, status bar, auto re-translate — ver "Já implementado" abaixo) já estão prontas e
> testadas em `packages/ide-adapters/intellij/`. Este blueprint cobre as features restantes.

## Como usar este blueprint

Cada seção é uma feature independente. Ordem sugerida = ordem das seções (dependências
crescem). Para cada feature:

1. Leia o **Product spec** (o que a feature faz do ponto de vista do usuário).
2. Crie os arquivos listados em **Files** a partir dos esqueletos.
3. Rode `./gradlew test buildPlugin` — tem que ficar verde antes de commitar.
4. Verifique o **DoD** (todos os critérios são verificáveis programaticamente ou via `runIde`).
5. Se bater um **Escalation gate**, pare e escale pro Marco — não improvise arquitetura de SDK.

### Convenções herdadas (não renegociáveis)

- **Branch única:** `dev-marco-agent.intellij-mvp` (ou nova `dev-marco-agent.intellij-features` se o Marco pedir).
- **Commits < 300 LOC** de diff (Kotlin tem boilerplate; +100 LOC de folga).
- **Sem `--no-verify`, sem `git rebase`, sem force push. Nunca commit em `main`.**
- **Nunca tocar** `packages/ide-adapters/vscode/`, `packages/core/`, nem `babel-services` — são de outras instâncias.
- **Core.Host é reusado verbatim** via `CoreBridge` (subprocess JSON Lines). Não reimplemente tradução.
- Português em docs/comentários pro humano; inglês em código, docstrings e prompts.
- Build/test verdes antes de marcar qualquer feature como feita.

### Blocos de reuso já prontos (MVP)

| Componente | Arquivo | O que oferece |
|---|---|---|
| `CoreBridge` | `services/CoreBridge.kt` | `translateToNaturalLanguage`, `translateFromNaturalLanguage`, `applyTranslatedEdits`, `getKeywordMap`, `getIdentifierMap`, `getSupportedLanguages`, `validateSyntax`* |
| `TranslationService` | `services/TranslationService.kt` | `toDisplay`, `toDisk`, `isTranslatable`, `dottedExtension` (fail-open) |
| `LanguageService` | `services/LanguageService.kt` | `currentLanguage`, `isTranslationActive()`, `addChangeListener` |
| `BabelSettings` | `settings/BabelSettings.kt` | estado persistente (`language`, `enabled`, `coreHostPath`) |
| `BabelKeys.TRANSLATED_VIEW` | `providers/TranslatedView.kt` | user-data que marca um `LightVirtualFile` como view traduzida |

\* `validateSyntax` ainda não tem wrapper tipado no `CoreBridge` — a Feature 4 adiciona um
(`invoke("ValidateSyntax", ...)` já funciona genérico).

**Effort total estimado:** ~40–60h Qwen, dividido em ~15–25 commits < 200 LOC.

---

## Já implementado no plugin (não reimplementar)

As features de paridade abaixo já foram entregues no MVP + rodada de parity e **não devem ser
refeitas**. Os números das features restantes preservam os IDs originais deste blueprint (por
isso a numeração começa em 3 e pula a 7):

| Feature original | Entregue como | Arquivos |
|---|---|---|
| Syntax highlighting (Feature 1) | `BabelAnnotator` | `highlighting/BabelAnnotator.kt`, `highlighting/BabelColors.kt` |
| Mouse hover tooltip | `BabelAnnotator` + `HoverProvider` (Ctrl+Q) | `highlighting/BabelAnnotator.kt`, `providers/HoverProvider.kt` |
| Completion (Feature 2) | `BabelCompletionContributor` | `completion/BabelCompletionContributor.kt` |
| Status bar (Feature 7) | `BabelStatusBarWidget` | `statusbar/BabelStatusBarWidgetFactory.kt`, `statusbar/BabelStatusBarWidget.kt` |
| Auto re-translate ao trocar idioma | `AutoTranslateManager` | `services/AutoTranslateManager.kt` |

O keyword map usado por highlighting/hover/completion é cacheado por idioma em
`TranslationService.keywordMap()` — reuse-o, não faça round-trip no Core por token.

---

## Feature 3 — Code action "Translate identifier via LLM"

**Effort:** ~6–8h · **Depende de:** Feature 8 (cliente `babel-services`)

### Product spec
Clicar num identificador (ex.: `calculateTotal`) → quick fix "Babel: traduzir identificador"
→ chama o backend `babel-services` (LLM) → escreve annotation `// tradu calculateTotal = calcularTotal`
e atualiza o mapa de identificadores.

### Files
- `actions/TranslateIdentifierAction.kt` (implementa `IntentionAction`)

```kotlin
class TranslateIdentifierAction : IntentionAction {
    override fun getText() = "Babel: traduzir identificador via LLM"
    override fun getFamilyName() = "Babel"
    override fun isAvailable(project: Project, editor: Editor?, file: PsiFile?): Boolean {
        val vf = file?.virtualFile ?: return false
        return vf.getUserData(BabelKeys.TRANSLATED_VIEW) != null
    }
    override fun invoke(project: Project, editor: Editor?, file: PsiFile?) {
        val identifier = editor?.selectionModel?.selectedText ?: return
        val translated = service<BabelServicesClient>().translateIdentifier(identifier, /* target */ "pt-BR")
        // inserir annotation `// tradu <identifier> = <translated>` + persistir no identifier map
    }
    override fun startInWriteAction() = true
}
```
```xml
<intentionAction><className>com.nfasylum.babel.intellij.actions.TranslateIdentifierAction</className></intentionAction>
```

### DoD
- `buildPlugin` verde.
- Teste unitário do cliente (Feature 8) mockando HTTP.
- `runIde` + backend rodando: selecionar identificador → intention aparece → após aplicar, annotation `// tradu` é inserida.

### Escalation gate
Se `babel-services` (Instance 1) ainda não expõe endpoint de tradução de identificador:
implemente só o esqueleto + escale sobre o contrato do endpoint. **Não invente o shape da API.**

---

## Feature 4 — Diagnostics (identificadores sem `// tradu`)

**Effort:** ~5–7h · **Depende de:** `CoreBridge.getIdentifierMap` + `ValidateSyntax`

### Product spec
Inspection que sinaliza (weak warning) identificadores num arquivo traduzido que ainda não
têm mapeamento `// tradu`, sugerindo a Feature 3 como quick fix.

### Files
- `inspections/MissingTraduInspection.kt` (`LocalInspectionTool`)

```kotlin
class MissingTraduInspection : LocalInspectionTool() {
    override fun buildVisitor(holder: ProblemsHolder, isOnTheFly: Boolean): PsiElementVisitor {
        return object : PsiElementVisitor() {
            override fun visitElement(element: PsiElement) {
                val view = element.containingFile?.virtualFile?.getUserData(BabelKeys.TRANSLATED_VIEW) ?: return
                val identifierMap = service<CoreBridge>().getIdentifierMap(view.language)
                if (isUnmappedIdentifier(element, identifierMap)) {
                    holder.registerProblem(element, "Identificador sem tradução // tradu",
                        ProblemHighlightType.WEAK_WARNING)
                }
            }
        }
    }
}
```
```xml
<localInspection language="" implementationClass="com.nfasylum.babel.intellij.inspections.MissingTraduInspection"
                 displayName="Babel: identificador sem // tradu" groupName="Babel"
                 level="WEAK WARNING" enabledByDefault="true"/>
```

### DoD
- `buildPlugin` verde.
- Teste unitário puro `isUnmappedIdentifier` com/sem entrada no map.
- `runIde`: identificador não mapeado ganha sublinhado weak-warning.

### Escalation gate
Distinguir "identificador" de "keyword" sem PSI da linguagem-alvo é frágil. Se não houver PSI
confiável na Community, escale sobre estratégia (regex-based vs PSI-based).

---

## Feature 5 — Rename tradu-aware

**Effort:** ~6–9h · **Depende de:** identifier map

### Product spec
Renomear um identificador na view traduzida deve renomear o identificador original no disco
e atualizar a annotation `// tradu`, mantendo os dois lados em sincronia.

### Files
- `refactoring/BabelRenameHandler.kt` (implementa `RenameHandler`)

```kotlin
class BabelRenameHandler : RenameHandler {
    override fun isAvailableOnDataContext(dataContext: DataContext): Boolean {
        val vf = CommonDataKeys.VIRTUAL_FILE.getData(dataContext) ?: return false
        return vf.getUserData(BabelKeys.TRANSLATED_VIEW) != null
    }
    override fun invoke(project: Project, editor: Editor?, file: PsiFile?, dataContext: DataContext?) {
        // 1. capturar novo nome (translated). 2. reverse-map pro original via identifier map.
        // 3. delegar rename ao original no disco. 4. atualizar // tradu.
    }
}
```
```xml
<renameHandler implementation="com.nfasylum.babel.intellij.refactoring.BabelRenameHandler"/>
```

### DoD
- `buildPlugin` verde.
- Teste unitário do reverse-map (translated name → original name).
- `runIde`: Shift+F6 num identificador traduzido → renomeia original no disco + atualiza `// tradu`.

### Escalation gate
Rename cross-file com PSI é a feature mais arriscada. Se o rename original não puder ser
delegado com segurança (risco de corromper arquivo), **escale antes de commitar** — melhor
entregar só a detecção do que um rename que quebra código.

---

## Feature 6 — Ícones no file explorer

**Effort:** ~3–4h · **Depende de:** nada

### Product spec
Arquivos atualmente abertos como view traduzida ganham um badge no Project View pra indicar
que estão sendo mostrados em outro idioma.

### Files
- `icons/BabelIconProvider.kt` (`IconProvider`)
- `resources/icons/babel-badge.svg` (16x16)

```kotlin
class BabelIconProvider : IconProvider() {
    override fun getIcon(element: PsiElement, flags: Int): Icon? {
        val vf = (element as? PsiFile)?.virtualFile ?: return null
        return if (vf.getUserData(BabelKeys.TRANSLATED_VIEW) != null)
            IconLoader.getIcon("/icons/babel-badge.svg", javaClass) else null
    }
}
```
```xml
<iconProvider implementation="com.nfasylum.babel.intellij.icons.BabelIconProvider"/>
```

### DoD
- `buildPlugin` verde (SVG presente em `resources/icons/`).
- `runIde`: arquivo com view traduzida aberta mostra badge no Project View.

### Escalation gate
Nenhum esperado. Se `IconLoader` não achar o SVG, verifique o path relativo ao classpath.

---

## Feature 8 — Cliente Kotlin pro backend `babel-services`

**Effort:** ~5–7h · **Depende de:** contrato de API do `babel-services` (Instance 1)

### Product spec
Cliente HTTP que fala com o backend hospedado do Babel (tradução de identificador via LLM,
sync de mapas). Base pras Features 3 e 4.

### Files
- `services/BabelServicesClient.kt` (`@Service(APP)`)

```kotlin
@Service(Service.Level.APP)
class BabelServicesClient {
    // Usa java.net.http.HttpClient (JDK 17, sem dep nova).
    private val http = HttpClient.newHttpClient()
    var baseUrl: String = "" // vem de BabelSettings (adicionar campo serviceUrl lá)

    fun translateIdentifier(identifier: String, targetLanguage: String): String {
        val body = gson.toJson(mapOf("identifier" to identifier, "targetLanguage" to targetLanguage))
        val request = HttpRequest.newBuilder(URI.create("$baseUrl/identifiers/translate"))
            .header("Content-Type", "application/json")
            .POST(HttpRequest.BodyPublishers.ofString(body)).build()
        val response = http.send(request, HttpResponse.BodyHandlers.ofString())
        return gson.fromJson(response.body(), IdentifierResponse::class.java).translated
    }
}
```

### DoD
- `buildPlugin` verde.
- Teste unitário com um `HttpClient` fake (ou `com.sun.net.httpserver.HttpServer` local) cobrindo happy path + erro HTTP.
- Integração real fica bloqueada até o backend existir (documentar).

### Escalation gate
**Não invente o contrato da API.** Se `babel-services` (Instance 1) ainda não publicou os
endpoints, implemente o cliente contra um contrato provisório documentado no topo do arquivo
e escale pro Marco confirmar antes de ligar Features 3/4 nele. `java.net.http` só — **nenhuma dep
HTTP nova** (nada de OkHttp/Retrofit sem aprovação, ver guardrail de dependências).

---

## Checklist final (quando as features restantes estiverem prontas)

- [ ] `./gradlew test buildPlugin` verde com todas as features.
- [ ] `plugin.xml` registra todos os EPs novos sem warning no `verifyPluginConfiguration`.
- [ ] Cada feature tem ao menos 1 teste (unitário puro ou platform `BasePlatformTestCase`).
- [ ] `runIde` sobe sem erro no idea.log e nenhuma feature crasha.
- [ ] Nenhuma dependência nova além de JDK/Kotlin stdlib/IntelliJ SDK/jUnit.
- [ ] Nada tocado fora de `packages/ide-adapters/intellij/`.
- [ ] Paridade documentada vs extension VS Code no README.

Boa execução. Em dúvida de API do SDK: **escale, não improvise** — o IntelliJ Platform tem quirks
por versão e é mais barato perguntar que refazer.
