# Contexto - Tarefa 040

## Dependencias
Nenhuma dependencia estrita. Pode rodar em paralelo com outras tarefas da Fase 5.

## Bloqueia
- Tarefa 041 (code cleanup pode incluir fixes de seguranca)
- Tarefa 045 (deploy nao deve ter vulnerabilidades conhecidas)

## Arquivos relevantes
- packages/core/src/ (todo o codigo C# para revisao)
- packages/ide-adapters/vscode/src/ (todo o codigo TypeScript para revisao)
- packages/core/MultiLingualCode.Core.csproj (dependencias .NET)
- packages/ide-adapters/vscode/package.json (dependencias npm)
- packages/ide-adapters/vscode/package-lock.json (lockfile npm)

## Notas
- Security review e especialmente importante pois a extensao processa codigo fonte do usuario.
- A extensao nao deve NUNCA executar codigo fonte - apenas ler e traduzir texto.
- Path traversal e um risco real se caminhos vierem de arquivos de configuracao JSON.
- Para o TCC, documentar a analise de seguranca e um diferencial positivo.
- Considerar usar ferramentas automatizadas: Snyk, GitHub Dependabot, ou CodeQL.
- Manter o principio de least privilege - extensao so acessa o que precisa.
