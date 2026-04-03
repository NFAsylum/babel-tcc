# Tarefa 080 - Corrigir release.yml secrets e gitignore pycache

## Prioridade: HIGH (release.yml) + MEDIUM (gitignore)

## Problemas

### HIGH-004: release.yml secrets context invalido
Arquivo: .github/workflows/release.yml linha 128

`if: secrets.VSCE_PAT != ''` — o context `secrets` nao esta disponivel em expressoes `if` do GitHub Actions. A condicao nunca funciona como esperado. O publish no Marketplace pode nunca executar ou sempre executar.

Fix: usar `if: ${{ secrets.VSCE_PAT != '' }}` com a sintaxe de expressao correta, ou usar `if: env.VSCE_PAT != ''` com env mapping.

### MEDIUM-001: __pycache__ nao esta no .gitignore
Arquivo: .gitignore

`__pycache__/` e `*.pyc` nao estao no .gitignore. O arquivo `LanguageAdapters/Python/__pycache__/tokenizer_service.cpython-313.pyc` esta commitado no repositorio.

Fix: adicionar `__pycache__/` e `*.pyc` ao .gitignore e remover o arquivo commitado com `git rm --cached`.
