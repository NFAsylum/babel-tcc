# Tarefa 037 - Testes de integracao end-to-end

## Fase
5 - Testes e QA

## Objetivo
Criar e executar cenarios de teste end-to-end completos que validem o fluxo inteiro da extensao com projetos reais.

## Escopo
- Cenario 1 - Traducao C# para PT-BR:
  - Abrir projeto C# real no VS Code
  - Ativar extensao e selecionar idioma PT-BR
  - Verificar que keywords sao traduzidas corretamente
  - Verificar que identificadores customizados sao traduzidos
  - Verificar que strings literais NAO sao traduzidas
  - Verificar que comentarios NAO sao traduzidos (ou sao, conforme configuracao)
- Cenario 2 - Traducao reversa PT-BR para C#:
  - Abrir codigo com visualizacao em PT-BR
  - Desativar traducao / voltar para idioma original
  - Verificar que codigo volta ao estado original identico
- Cenario 3 - Round-trip:
  - Traduzir arquivo C# para PT-BR
  - Salvar (gerar identifier-map.json atualizado se necessario)
  - Traduzir de volta para ingles
  - Comparar byte-a-byte com original
  - Verificar que nenhuma informacao foi perdida
- Cenario 4 - Edicao durante traducao:
  - Abrir arquivo traduzido
  - Editar codigo (adicionar metodo, mudar variavel)
  - Salvar arquivo
  - Verificar que edicao persiste corretamente no codigo original
- Cenario 5 - Multiplos idiomas:
  - Traduzir para PT-BR, verificar
  - Trocar para ES (espanhol), verificar
  - Voltar para original, verificar
- Cenario 6 - Projetos reais:
  - Testar com projeto de exemplo HelloWorld
  - Testar com projeto Calculator (multiplos arquivos)
  - Testar com projeto TodoApp (classes, interfaces, generics)
- Automatizar cenarios usando VS Code Extension Testing framework
- Documentar resultados de cada cenario com evidencias
