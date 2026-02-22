# Tarefa 040 - Security review

## Fase
5 - Testes e QA

## Objetivo
Realizar revisao de seguranca do projeto, validando inputs, prevenindo vulnerabilidades conhecidas e auditando dependencias.

## Escopo
- Validacao de input - codigo fonte:
  - Verificar que codigo fonte malicioso nao causa crash ou comportamento inesperado
  - Testar com arquivos extremamente grandes (DoS via parsing)
  - Testar com caracteres de controle e sequencias de escape
  - Verificar que o parser nao executa codigo (apenas le/analisa)
- Validacao de input - JSON:
  - Validar schema de identifier-map.json antes de processar
  - Validar schema de config.json antes de processar
  - Testar com JSON malformado, deeply nested, circular references
  - Limitar tamanho maximo de arquivos JSON aceitos
- Path traversal:
  - Verificar que caminhos em configuracao nao permitem acesso fora do workspace
  - Sanitizar paths recebidos via JSON (../, symlinks)
  - Testar com paths absolutos, relativos, e com caracteres especiais
- Code injection via JSON:
  - Verificar que valores em identifier-map.json nao sao executados
  - Testar com payloads de injection em campos de traducao
  - Verificar que output traduzido nao introduz codigo executavel
- Permissoes de arquivo:
  - Verificar que a extensao apenas le/escreve onde deve
  - Verificar permissoes do processo Core
  - Nao gravar dados sensiveis em logs
- Auditoria de dependencias:
  - Executar `dotnet list package --vulnerable` no Core
  - Executar `npm audit` na extensao
  - Atualizar dependencias com vulnerabilidades conhecidas
  - Documentar dependencias e suas licencas
- Comunicacao Core <-> Extension:
  - Verificar que stdin/stdout nao expoe informacoes sensiveis
  - Validar formato de mensagens trocadas
  - Testar com mensagens malformadas
