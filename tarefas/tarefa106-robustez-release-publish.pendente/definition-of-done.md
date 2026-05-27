# Definition of Done - Tarefa 106

- [ ] vsce publish e ovsx publish com retry/backoff
- [ ] Idempotencia: publish pula (trata como sucesso) quando a versao ja esta publicada no destino
- [ ] Pipeline mantem fail-fast (sem continue-on-error / sem desacoplar destinos)
- [ ] Um rerun completa os destinos faltantes sem conflito de "versao ja existe"
- [ ] Publishes seguem condicionais aos secrets (VSCE_PAT / OVSX_PAT)
- [ ] Log indica destino que falhou e destino pulado por ja estar publicado
- [ ] Comportamento validado num release de teste (ou dry-run)
