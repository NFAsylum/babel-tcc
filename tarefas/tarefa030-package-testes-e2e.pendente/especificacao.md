# Tarefa 030 - Package e testes end-to-end

## Fase
3 - VS Code Extension

## Objetivo
Empacotar extensao como .vsix e validar com testes end-to-end.

## Escopo
- Configurar bundling (esbuild/webpack) para gerar .vsix
  - Incluir binarios do Core C# no pacote
  - Incluir tabelas de traducao
- Gerar .vsix e testar instalacao local
- Suite de testes end-to-end:
  - Abrir arquivo C# -> ver traducao PT-BR
  - Editar codigo traduzido -> salvar -> verificar .cs original
  - Trocar idioma -> verificar nova traducao
  - Toggle on/off
- Correcao de bugs encontrados
