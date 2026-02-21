# Definition of Done - Tarefa 040

- [ ] Codigo fonte malicioso/invalido nao causa crash (tratamento graceful)
- [ ] Arquivos muito grandes sao rejeitados ou tratados com limite configuravel
- [ ] JSON malformado e tratado com erro descritivo (nao crash)
- [ ] Schema de identifier-map.json e config.json e validado antes do uso
- [ ] Path traversal testado e bloqueado (../, symlinks, paths absolutos)
- [ ] Nenhum campo JSON e executado como codigo
- [ ] Extensao apenas le/escreve dentro do workspace e diretorios permitidos
- [ ] `dotnet list package --vulnerable` retorna zero vulnerabilidades criticas
- [ ] `npm audit` retorna zero vulnerabilidades criticas ou altas
- [ ] Dependencias com vulnerabilidades conhecidas atualizadas ou mitigadas
- [ ] Comunicacao stdin/stdout validada contra mensagens malformadas
- [ ] Nenhuma informacao sensivel gravada em logs
- [ ] Relatorio de security review documentado
