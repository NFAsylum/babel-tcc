# Roadmap — Babel IntelliJ extension

> Features não-MVP planejadas pra próxima fase de trabalho do adapter IntelliJ.
> A base MVP + a paridade com a extension VS Code já está pronta em
> `packages/ide-adapters/intellij/` (ver README lá).

## Como usar este roadmap

Cada seção é uma feature independente. Ordem sugerida = ordem das seções (dependências
crescem). Para cada feature:

1. Leia o **Product spec** (o que a feature faz do ponto de vista do usuário).
2. Crie os arquivos listados em **Files** a partir dos esqueletos.
3. Rode `./gradlew test buildPlugin` — tem que ficar verde antes de commitar.
4. Verifique o **DoD** (todos os critérios são verificáveis programaticamente ou via `runIde`).
5. Se bater um **Escalation gate**, pare e escale pro Marco — não improvise arquitetura de SDK.

### Convenções herdadas (não renegociáveis)

- **Branch dedicada** por feature (ex.: `dev-marco-agent.intellij-diagnostics`); nunca commit em `main`.
- **Commits < 300 LOC** de diff (Kotlin tem boilerplate; +100 LOC de folga).
- **Sem `--no-verify`, sem `git rebase`, sem force push. Nunca commit em `main`.**
- **Nunca tocar** `packages/ide-adapters/vscode/` nem `packages/core/`.
- **Core.Host é reusado verbatim** via `CoreBridge` (subprocess JSON Lines). Não reimplemente tradução.
- **Babel é 100% determinístico e offline.** Nenhuma feature pode depender de serviço hospedado,
  rede, ou tradução dinâmica externa. A tradução vem sempre do Core.Host (tabelas estáticas +
  anotações `// tradu` no próprio código).
- Português em docs/comentários pro humano; inglês em código e docstrings.
- Build/test verdes antes de marcar qualquer feature como feita.

Blocos de reuso e features já implementadas: ver `packages/ide-adapters/intellij/README.md`. O
keyword map usado por highlighting/hover/completion é cacheado por idioma em
`TranslationService.keywordMap()` — reuse-o, não faça round-trip no Core por token.

**Effort total estimado:** ~30–45h de trabalho futuro, dividido em commits < 200 LOC.

---

## Feature 1 — Diagnostics (identificadores sem `// tradu`)

**Effort:** ~5–7h · **Depende de:** `CoreBridge.getIdentifierMap`

### Product spec
Inspection que sinaliza (weak warning) identificadores num arquivo traduzido que ainda não têm
mapeamento `// tradu`. O quick fix é **determinístico**: insere um template `// tradu <id> = `
para o usuário preencher — Babel nunca infere/traduz identificadores sozinho.

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
- `runIde`: identificador não mapeado ganha sublinhado weak-warning; quick fix insere o template `// tradu`.

### Escalation gate
Distinguir "identificador" de "keyword" sem PSI da linguagem-alvo é frágil. Se não houver PSI
confiável na Community, escale sobre estratégia (regex-based vs PSI-based).

---

## Feature 2 — Rename tradu-aware

**Effort:** ~6–9h · **Depende de:** identifier map (determinístico, via Core.Host)

### Product spec
Renomear um identificador na view traduzida deve renomear o identificador original no disco e
atualizar a annotation `// tradu`, mantendo os dois lados em sincronia. Tudo determinístico: o
nome novo é reverse-mapeado para o original via identifier map do Core.

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

## Feature 3 — Ícones no file explorer

**Effort:** ~3–4h · **Depende de:** nada

### Product spec
Arquivos atualmente abertos como view traduzida ganham um badge no Project View pra indicar que
estão sendo mostrados em outro idioma.

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

## Checklist final (quando as features restantes estiverem prontas)

- [ ] `./gradlew test buildPlugin` verde com todas as features.
- [ ] `plugin.xml` registra todos os EPs novos sem warning no `verifyPluginConfiguration`.
- [ ] Cada feature tem ao menos 1 teste (unitário puro ou platform `BasePlatformTestCase`).
- [ ] `runIde` sobe sem erro no idea.log e nenhuma feature crasha.
- [ ] Nenhuma feature depende de rede/serviço hospedado — tudo determinístico via Core.Host.
- [ ] Nenhuma dependência nova além de JDK/Kotlin stdlib/IntelliJ SDK/jUnit.
- [ ] Nada tocado fora de `packages/ide-adapters/intellij/`.

Boa execução. Em dúvida de API do SDK: **escale, não improvise** — o IntelliJ Platform tem quirks
por versão e é mais barato perguntar que refazer.
