# Tarefa 036 - Cobertura de testes unitarios

## Fase
5 - Testes e QA

## Objetivo
Atingir cobertura de testes unitarios de 80%+ no Core (C#) e 70%+ na Extension (TypeScript), cobrindo edge cases criticos.

## Escopo
- Analisar cobertura atual com `dotnet test --collect:"XPlat Code Coverage"` e `jest --coverage`
- Identificar areas com baixa cobertura e priorizar por criticidade
- Adicionar testes para edge cases no Core (C#):
  - Codigo fonte vazio ou null
  - Arquivos com erros de sintaxe (codigo invalido)
  - Keywords nao mapeadas no idioma alvo
  - Identificadores com caracteres especiais (unicode, emojis, acentos)
  - Arquivos grandes (1000+ linhas)
  - Arquivos com encodings diferentes (UTF-8 BOM, UTF-16)
  - Traducoes com conflitos (mesmo identificador em contextos diferentes)
  - JSON de configuracao malformado ou incompleto
  - Linguagem nao registrada no LanguageRegistry
- Adicionar testes para edge cases na Extension (TypeScript):
  - Documento sem linguagem detectada
  - Core process nao disponivel ou crashando
  - Timeout na comunicacao com Core
  - Settings invalidas
  - Multiplos documentos abertos simultaneamente
  - Documento modificado durante traducao
- Configurar relatorio de cobertura no CI:
  - Gerar relatorio em formato Cobertura (XML)
  - Integrar com GitHub Actions para exibir cobertura no PR
  - Configurar threshold minimo que falha o build se nao atingido
  - Gerar badge de cobertura para o README
