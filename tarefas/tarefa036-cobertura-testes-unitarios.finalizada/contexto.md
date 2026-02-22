# Contexto - Tarefa 036

## Dependencias
- Fases 1-3 completas (codigo a ser testado precisa existir)
- Tarefa 003 (CI/CD basico configurado)
- Tarefa 020 (testes de integracao do Core como base)
- Tarefa 030 (testes e2e da extensao como base)

## Bloqueia
- Tarefa 039 (testes de compatibilidade dependem de boa cobertura unitaria)
- Tarefa 041 (code cleanup precisa de testes verdes como safety net)

## Arquivos relevantes
- packages/core/tests/ (testes C# existentes)
- packages/ide-adapters/vscode/src/test/ (testes TypeScript existentes)
- .github/workflows/ (CI pipeline para adicionar cobertura)
- packages/core/MultiLingualCode.Core.Tests.csproj (configuracao de testes C#)
- packages/ide-adapters/vscode/jest.config.js (configuracao Jest)

## Notas
- Usar coverlet para cobertura C# e jest --coverage para TypeScript.
- Considerar usar ReportGenerator para gerar relatorio HTML local.
- Focar em cobertura de branch (branch coverage) alem de line coverage.
- Nao sacrificar qualidade dos testes por cobertura numerica - testes devem verificar comportamento real.
- Edge cases com caracteres especiais sao especialmente importantes dado o contexto multilingual do projeto.
