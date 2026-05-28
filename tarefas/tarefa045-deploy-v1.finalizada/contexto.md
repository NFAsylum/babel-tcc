# Contexto - Tarefa 045

## Dependencias
- Tarefa 041 (code cleanup completo)
- Tarefa 042 (CI/CD pipeline funcional)
- Tarefa 043 (Marketplace assets prontos)
- Tarefa 044 (LICENSE e CHANGELOG presentes)
- Tarefa 031 (README finalizado)
- Tarefa 032 (documentacao do usuario)
- Tarefa 033 (documentacao do desenvolvedor)
- Tarefa 040 (security review limpo)

## Bloqueia
- Tarefa 046 (TCC precisa da extensao publicada como resultado)

## Arquivos relevantes
- package.json (versao e metadata)
- packages/core/Directory.Build.props (versao .NET)
- .github/workflows/ (pipeline de release)
- CHANGELOG.md (release notes)
- README.md (descricao do Marketplace)
- .vscodeignore (controle do pacote)

## Notas
- Esta e a tarefa mais critica do projeto - o deploy final.
- Fazer um dry-run com versao 0.9.0-beta antes da 1.0.0 real.
- Manter backup do .vsix gerado em local separado.
- Apos publicacao, monitorar issues e reviews no Marketplace.
- O primeiro deploy pode demorar ate 24h para aparecer no Marketplace.
- Ter um plano de rollback: saber como despublicar ou publicar fix rapidamente.
- Para o TCC, o deploy bem-sucedido e o principal entregavel tecnico.
