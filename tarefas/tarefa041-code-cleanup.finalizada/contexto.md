# Contexto - Tarefa 041

## Dependencias
- Fase 5 completa (testes como safety net para refatoracoes)
- Tarefa 036 (cobertura de testes como baseline)
- Tarefa 037 (testes de integracao para validar refatoracoes)

## Bloqueia
- Tarefa 042 (CI/CD completo roda sobre codebase limpa)
- Tarefa 045 (deploy requer codebase finalizada)

## Arquivos relevantes
- packages/core/src/ (todo o codigo C#)
- packages/ide-adapters/vscode/src/ (todo o codigo TypeScript)
- packages/core/tests/ (testes C#)
- packages/ide-adapters/vscode/src/test/ (testes TypeScript)
- .editorconfig (padroes de formatacao)
- docs/padroes-codigo.md (convencoes de referencia)

## Notas
- Fazer refatoracoes em commits pequenos e atomicos para facilitar rollback.
- Rodar testes apos CADA refatoracao, nao apenas ao final.
- Usar ferramentas automatizadas: `dotnet format`, `eslint --fix`, `prettier`.
- Code review pode ser feito por pares ou auto-review com checklist.
- Esta tarefa e um "gate" antes do deploy - nao pular ou fazer superficialmente.
- Considerar usar analyzers do .NET (StyleCop, SonarAnalyzer) para detectar issues.
