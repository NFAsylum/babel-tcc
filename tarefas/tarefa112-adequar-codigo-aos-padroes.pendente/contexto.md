# Contexto - Tarefa 112

## Dependencias
Depende do PR que unificou as regras em docs/padroes-codigo.md e estendeu as proibicoes
absolutas as demais linguagens.

## Bloqueia
Nenhuma.

## Arquivos relevantes
- docs/padroes-codigo.md (as regras que este trabalho aplica)
- packages/ide-adapters/intellij/src/main/kotlin/ (todo o plugin)
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py
- scripts/copy-readme.js
- scripts/copy-translations.js
- .editorconfig (define 2 espacos para .js)

## Notas
- O inventario da especificacao e um retrato de 2026-08-16 e envelhece a cada commit.
  Recontar antes de comecar, com os comandos abaixo.
- O grosso do trabalho e o plugin IntelliJ. Python e os scripts sao pequenos.
- Adequar `private` no Kotlin muda a superficie publica de classes; o plugin tem 58 testes
  que devem continuar passando (`./gradlew test`, exige JDK 17).
- Trocar `throw` por Result no CoreBridge afeta TranslationService, SaveHandler e
  AutoTranslateManager, que hoje capturam a excecao para degradar. A degradacao precisa
  ser preservada: um Core quebrado nunca pode corromper o arquivo em disco (DT-003).

## Comandos para recontar

```bash
# Kotlin: private, throw, tipos anulaveis
grep -rho "private " packages/ide-adapters/intellij/src/main --include=*.kt | wc -l
grep -rhoE "^\s*throw " packages/ide-adapters/intellij/src/main --include=*.kt | wc -l
grep -rhoE ": [A-Z][A-Za-z<>]*\?" packages/ide-adapters/intellij/src/main --include=*.kt | wc -l

# Python: anotacoes de tipo na assinatura
grep -cE "def .*->" packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py

# JS: indentacao de 4 espacos
grep -c "^    [a-zA-Z]" scripts/*.js
```
