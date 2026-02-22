# Tarefa 034 - Exemplos praticos

## Fase
4 - Documentacao e Exemplos

## Objetivo
Criar projetos de exemplo completos que demonstram o uso da extensao end-to-end, servindo como referencia e material de teste.

## Escopo
- Criar diretorio `examples/` na raiz do repositorio
- Criar `examples/csharp/HelloWorld/`:
  - `Program.cs` com codigo C# simples (Console.WriteLine, variaveis, if/else)
  - `.multilingual/identifier-map.json` com mapeamento de identificadores para PT-BR
  - `.multilingual/config.json` com configuracao do projeto
  - `README.md` explicando o exemplo e como usa-lo
- Criar `examples/csharp/Calculator/`:
  - `Program.cs` com classe Calculator e operacoes basicas
  - `Calculator.cs` com metodos (Somar, Subtrair, Multiplicar, Dividir)
  - `.multilingual/identifier-map.json` com mapeamento completo
  - `.multilingual/config.json` com configuracao
  - `README.md` explicando o exemplo
- Criar `examples/csharp/TodoApp/` (exemplo mais complexo):
  - Multiplos arquivos .cs (Program.cs, TodoItem.cs, TodoService.cs)
  - Uso de classes, interfaces, generics, LINQ
  - `.multilingual/identifier-map.json` completo
  - `README.md` com walkthrough detalhado
- Cada exemplo deve incluir versao original (ingles) e versao traduzida esperada (PT-BR)
- Documentar em cada README como abrir, traduzir e verificar o resultado
