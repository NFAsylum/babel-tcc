# Tarefa 080 - Corrigir gitignore pycache

## Prioridade: MEDIUM

## Problema

### MEDIUM-001: __pycache__ nao esta no .gitignore
Arquivo: .gitignore

`__pycache__/` e `*.pyc` nao estao no .gitignore. O arquivo `LanguageAdapters/Python/__pycache__/tokenizer_service.cpython-313.pyc` esta commitado no repositorio.

Fix: adicionar `__pycache__/` e `*.pyc` ao .gitignore e remover o arquivo commitado com `git rm --cached`.

## Nota
HIGH-004 (release.yml secrets) foi removido desta tarefa — verificado que `if: secrets.VSCE_PAT != ''` funciona corretamente em step-level no GitHub Actions (expressoes sao auto-wrapped em `${{ }}`).
