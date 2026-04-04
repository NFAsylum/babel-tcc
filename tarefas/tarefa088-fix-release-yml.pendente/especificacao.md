# Tarefa 088 - Corrigir problemas no release.yml

## Prioridade: HIGH

## Problemas

### 1. release.yml falha em todo push (CRITICAL)
O workflow tem trigger `on.push.tags: ['v*']` mas o GitHub Actions avalia
o workflow em todo push. Pushes sem tags resultam em 0 jobs executados e
status "failure" no Actions tab.

Fix: adicionar `branches-ignore: ['**']` para que o workflow so seja
avaliado quando tags estao presentes.

### 2. Nome do .vsix inconsistente (LOW)
`vsce package --out babel-tcc-${{ github.ref_name }}.vsix` mas o pacote
se chama `multilingual-code`. O .vsix deveria ser `multilingual-code-${{ github.ref_name }}.vsix`.

## Arquivos afetados
- .github/workflows/release.yml
