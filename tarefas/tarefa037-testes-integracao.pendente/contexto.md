# Contexto - Tarefa 037

## Dependencias
- Fases 1-3 completas (extensao funcional end-to-end)
- Tarefa 034 (exemplos praticos como fixtures de teste)
- Tarefa 030 (testes e2e basicos como base)
- Tarefa 025 (edit interceptor para cenario de edicao)
- Tarefa 026 (save handler para cenario de save)

## Bloqueia
- Tarefa 039 (testes de compatibilidade usam cenarios de integracao)
- Tarefa 041 (code cleanup precisa de testes de integracao como safety net)

## Arquivos relevantes
- packages/ide-adapters/vscode/src/test/ (testes existentes)
- examples/ (projetos de exemplo como fixtures)
- packages/ide-adapters/vscode/src/test/suite/ (test suites)
- .vscode-test/ (VS Code test runner)

## Notas
- Usar @vscode/test-electron para rodar testes em instancia real do VS Code.
- Cenarios de round-trip sao os mais criticos - qualquer perda de dados e inaceitavel.
- Considerar gravar videos dos testes para documentacao do TCC.
- Testes de integracao sao mais lentos - configurar CI para roda-los em step separado.
- Fixtures de teste (projetos exemplo) devem ser versionados no repositorio.
