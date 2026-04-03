# Contexto - Tarefa 086

## Dependencias
- Nenhuma

## Bloqueia
- Tarefa 045 (deploy v1.0.0 depende de release pipeline funcional)

## Arquivos relevantes
- .github/workflows/release.yml

## Notas
- O problema do branches-ignore foi identificado no relatorio de CI/CD
  e corrigido no PR #55, mas a correcao nao chegou ao main (foi
  sobrescrita por merges posteriores)
- O guard VSCE_PAT e uma limitacao documentada do GitHub Actions:
  https://github.com/actions/runner/issues/520
- O release pipeline nunca foi testado com uma tag real
