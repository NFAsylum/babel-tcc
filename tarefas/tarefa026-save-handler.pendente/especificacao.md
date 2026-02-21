# Tarefa 026 - SaveHandler

## Fase
3 - VS Code Extension

## Objetivo
Interceptar saves para traduzir de volta para C# original antes de gravar no disco.

## Escopo
- Implementar SaveHandler em src/providers/
  - Capturar onWillSaveTextDocument para documentos multilingual
  - Chamar CoreBridge.translateFromNaturalLanguage() com conteudo editado
  - Salvar codigo original (C#) no arquivo real (.cs)
  - Feedback visual: "Arquivo salvo com sucesso"
  - Tratamento de erros: se traducao reversa falha, nao sobrescrever original
- Testes de save
