# Fase 2 - Language Adapters

**Objetivo:** Implementar suporte completo para C# usando Roslyn.
**Pre-requisito:** Fase 1 concluida (Core Engine funcional).

---

## Tarefa 2.1 - CSharpAdapter: Parse basico com Roslyn

**Escopo:**
- [ ] Adicionar pacote NuGet `Microsoft.CodeAnalysis.CSharp`
- [ ] Implementar `CSharpAdapter : ILanguageAdapter` em `Core/LanguageAdapters/`
- [ ] Implementar `Parse(string sourceCode)`:
  - Usar `CSharpSyntaxTree.ParseText()` para obter AST Roslyn
  - Converter `SyntaxNode` Roslyn -> nossa hierarquia `ASTNode`
  - Suportar: keywords (if, else, for, while, class, namespace, using, etc.)
  - Suportar: declaracoes de variaveis, metodos, classes
  - Preservar posicoes (linha/coluna) nos nodes
- [ ] Implementar `GetKeywordMap()`: carregar de keywords-base.json
- [ ] Implementar `ExtractIdentifiers()`: usar Roslyn para encontrar todos IdentifierNameSyntax
- [ ] Testes com codigo C# simples (HelloWorld, declaracoes basicas)

**Entrega:** Parse de C# simples gera AST customizada correta.

---

## Tarefa 2.2 - CSharpAdapter: Generate (AST -> codigo)

**Escopo:**
- [ ] Implementar `Generate(ASTNode ast)`:
  - Percorrer AST e gerar texto C#
  - Preservar indentacao e formatacao original
  - Preservar comentarios
  - Preservar espacos em branco significativos
- [ ] Implementar round-trip: Parse -> Generate deve produzir codigo identico
- [ ] Testes de round-trip com multiplos exemplos

**Dependencia:** Tarefa 2.1

**Entrega:** Generate produz codigo C# valido a partir da AST.

---

## Tarefa 2.3 - CSharpAdapter: Validacao e diagnosticos

**Escopo:**
- [ ] Implementar `ValidateSyntax(string sourceCode)`:
  - Usar Roslyn para detectar erros de sintaxe
  - Retornar ValidationResult com lista de erros
  - Incluir posicao (linha/coluna) dos erros
- [ ] Mapear DiagnosticSeverity do Roslyn para nosso modelo
- [ ] Testes com codigo valido e invalido

**Entrega:** Validacao detecta e reporta erros de sintaxe corretamente.

---

## Tarefa 2.4 - Sistema de deteccao "tradu"

**Escopo:**
- [ ] Implementar parser de comentarios `// tradu:nome`
- [ ] Suportar formatos:
  - `// tradu:nomeTraduzido` (traducao simples)
  - `// tradu:Metodo,param1:traducao1,param2:traducao2` (metodo + parametros)
  - `// tradu:"texto traduzido"` (traducao de string literal)
- [ ] Integrar com `IdentifierMapper` para persistir traducoes detectadas
- [ ] Detectar anotacoes durante o Parse do CSharpAdapter
- [ ] Testes com exemplos do plano (Calculator com tradu)

**Dependencia:** Tarefas 2.1, 1.7

**Entrega:** Anotacoes "tradu" sao detectadas e mapeadas automaticamente.

---

## Tarefa 2.5 - CSharpAdapter: Recursos avancados

**Escopo:**
- [ ] Suporte a classes e structs completos
- [ ] Suporte a propriedades (get/set)
- [ ] Suporte a namespaces e using directives
- [ ] Suporte a metodos (assinatura completa, generics basico)
- [ ] Suporte a enums
- [ ] Suporte a expressoes basicas (operadores, chamadas de metodo)
- [ ] Suporte a LINQ basico (where, select, orderby)
- [ ] Testes com codigo C# complexo (classe completa com varios membros)

**Dependencia:** Tarefas 2.1, 2.2

**Entrega:** Adapter suporta traducao de projetos C# realistas.

---

## Tarefa 2.6 - TranslationOrchestrator: Implementacao completa

**Escopo:**
- [ ] Implementar `TranslateAST()` completo:
  - Traduzir KeywordNodes usando NaturalLanguageProvider
  - Traduzir IdentifierNodes usando IdentifierMapper
  - Traduzir LiteralNodes marcados como traduzíveis
  - Recursao correta para todos os filhos
- [ ] Implementar `ReverseTranslateAST()` completo:
  - Reverter keywords traduzidas para originais
  - Reverter identificadores traduzidos
  - Reverter literais traduzidos
- [ ] Implementar `GenerateTranslatedCode()`:
  - Gerar codigo com keywords no idioma alvo
  - Manter estrutura e formatacao
- [ ] Testes end-to-end: C# -> PT-BR -> C# (round-trip completo)

**Dependencia:** Tarefas 2.1-2.5, 1.8

**Entrega:** Traducao completa C# <-> PT-BR funciona end-to-end.

---

## Tarefa 2.7 - Testes de integracao do Core

**Escopo:**
- [ ] Suite de testes com exemplos reais:
  - HelloWorld (traducao simples)
  - Calculator com tradu (identificadores customizados)
  - Classe complexa (namespaces, propriedades, metodos)
- [ ] Testes de round-trip: original -> traduzido -> original == original
- [ ] Testes de performance: medir tempo de traducao por tamanho de arquivo
- [ ] Atingir 80%+ de cobertura no Core
- [ ] Correcao de bugs encontrados

**Dependencia:** Tarefa 2.6

**Entrega:** Suite de testes verde, cobertura >= 80%.

---

## Dependencias

```
2.1 ──> 2.2 ──> 2.5
2.1 ──> 2.3
2.1 ──> 2.4
2.1 + 2.2 + 2.5 + 1.8 ──> 2.6
2.6 ──> 2.7
```
