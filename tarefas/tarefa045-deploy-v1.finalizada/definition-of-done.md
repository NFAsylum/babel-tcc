# Definition of Done - Tarefa 045

## Concluido nesta PR (bump 1.0.0)
- [x] Pre-deploy: testes passam, CI funcional, assets de Marketplace, LICENSE/CHANGELOG, README e
      docs prontos, security review limpo (tarefas 031-033, 040-044 finalizadas)
- [x] Versao 1.0.0 definida em package.json, package-lock.json e Directory.Build.props
- [x] Entrada [1.0.0] no CHANGELOG canonico da extensao
- [x] .vsix gera sem erros: universal ~5,0 MB; pacotes por-plataforma self-contained maiores por
      design (runtime .NET embutido, DT-010), download por usuario continua unico
- [x] README, CHANGELOG e LICENSE incluidos no pacote
- [x] Pipeline de release (matrix dual-track) validado no shakeout da 0.9.3 (run verde, 7 alvos
      publicados em GitHub Release + Marketplace + Open VSX)
- [x] NuGet/NPM complementares: decisao documentada de NAO publicar (o Core e interno a extensao;
      sem caso de uso programatico externo)

## Disparado ao empurrar a tag v1.0.0 (verificacao monitorada no run)
- [ ] Tag v1.0.0 criada e empurrada para o GitHub (acao do mantenedor)
- [ ] Publicacao automatica via matrix: Marketplace + Open VSX + GitHub Release com .vsix e notas
- [ ] Pagina do Marketplace exibe icone, descricao e screenshots corretamente
- [ ] Instalacao via Marketplace em VS Code limpo + traducao basica funcionando

## Fora de escopo
- Anuncio em redes/listas: nao se aplica ao contexto do TCC.
