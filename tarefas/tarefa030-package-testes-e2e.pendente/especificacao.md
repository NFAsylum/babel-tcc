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
- Smoke tests basicos (validacao minima de que tudo funciona junto):
  - Extensao ativa ao abrir arquivo .cs
  - Traducao basica funciona (keyword traduzida aparece)
  - Toggle on/off funciona
- Correcao de bugs encontrados

Nota: suite completa de testes e2e com cenarios detalhados fica na Tarefa 037.
