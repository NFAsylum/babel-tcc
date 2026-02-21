# Contexto - Tarefa 039

## Dependencias
- Tarefa 036 (cobertura de testes unitarios como pre-requisito)
- Tarefa 037 (testes de integracao para rodar em cada plataforma)
- Tarefa 003 (CI/CD basico para expandir com matrix strategy)

## Bloqueia
- Tarefa 045 (deploy requer compatibilidade verificada)
- Tarefa 043 (marketplace requer lista de plataformas suportadas)

## Arquivos relevantes
- package.json (engines.vscode - versao minima)
- .github/workflows/ (CI pipeline para adicionar matrix)
- packages/core/MultiLingualCode.Core.csproj (TargetFramework)
- docs/setup-ambiente.md (pre-requisitos por plataforma)

## Notas
- Priorizar Windows e Linux por serem as plataformas mais acessiveis para teste.
- macOS pode ser testado via GitHub Actions (macos-latest runner).
- A compatibilidade com .NET Runtime (sem SDK) e importante para usuarios finais.
- Paths com unicode sao especialmente problematicos no Windows - testar com acentos e caracteres CJK.
- VS Code 1.80 como minimo e uma escolha conservadora - ajustar se APIs mais novas forem necessarias.
- Documentar claramente quais plataformas sao "suportadas" vs "devem funcionar mas nao testadas".
