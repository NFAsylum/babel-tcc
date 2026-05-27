# Definition of Done - Tarefa 106

- [ ] vsce publish e ovsx publish com retry/backoff
- [ ] Falha de um registry nao bloqueia os outros publishes nem o GitHub Release
- [ ] GitHub Release criado mesmo com falha parcial de registry
- [ ] Log indica claramente o destino que falhou
- [ ] Publishes seguem condicionais aos secrets (VSCE_PAT / OVSX_PAT)
- [ ] Comportamento validado num release de teste (ou dry-run)
