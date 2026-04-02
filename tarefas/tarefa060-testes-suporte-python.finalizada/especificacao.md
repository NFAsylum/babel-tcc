# Tarefa 060 - Testes para o suporte a Python

## Fase
7 - Suporte a Python

## Objetivo
Criar testes unitarios e de integracao para todos os componentes do suporte a Python.

## Escopo

### 1. PythonKeywordMapTests.cs
- GetId retorna ID correto para cada uma das 35 keywords
- GetId retorna -1 para nao-keywords
- GetText retorna texto correto para cada ID
- GetMap retorna todas as 35 keywords
- Case-sensitivity: `True` != `true`, `False` != `false`, `None` != `none`

### 2. PythonTokenizerServiceTests.cs
- Tokeniza codigo simples (`def foo(): pass`) e retorna tokens corretos
- Identifica keywords vs identifiers corretamente
- Retorna posicoes corretas (linha, coluna)
- Trata strings de todos os tipos (single, double, triple, f-string, raw, byte)
- Trata erros de tokenizacao (string nao terminada) retornando ok=false
- Trata caso Python nao instalado com OperationResult.Fail
- Processo persistente funciona para multiplos requests sequenciais

### 3. PythonAdapterTests.cs
Seguindo o padrao de `CSharpAdapterTests.cs`:
- Properties_AreCorrect (LanguageName="Python", FileExtensions=[".py"])
- Parse_SimpleFunction_ExtractsKeywords (`def`, `return`)
- Parse_ClassDeclaration_ExtractsAll (`class`, `def`)
- Parse_IfElse_ExtractsConditionalKeywords (`if`, `elif`, `else`)
- Parse_ForLoop_ExtractsLoopKeywords (`for`, `in`, `break`, `continue`)
- Parse_TryExcept_ExtractsExceptionKeywords (`try`, `except`, `finally`, `raise`)
- Parse_AsyncAwait_ExtractsKeywords (`async`, `await`)
- Parse_Import_ExtractsKeywords (`import`, `from`, `as`)
- Parse_BooleanOperators_ExtractsKeywords (`and`, `or`, `not`, `is`, `in`)
- Parse_PreservesPositions (StartPosition, EndPosition corretos)
- Parse_ExtractsLiterals (strings, numeros)
- Parse_StringLiterals_AreTranslatable
- Generate_WithoutChanges_PreservesOriginalCode
- Generate_WithTranslatedKeywords_ReplacesInCode
- Generate_WithTranslatedIdentifiers_ReplacesInCode
- Generate_MultiLine_PreservesStructure (indentacao preservada!)
- RoundTrip_SimpleCode_PreservesStructure
- ReverseSubstituteKeywords_SkipsComments (comentarios `#`)
- ReverseSubstituteKeywords_SkipsStrings (todos os tipos Python)
- ReverseSubstituteKeywords_SkipsFStrings
- ReverseSubstituteKeywords_SkipsTripleQuotedStrings
- GetKeywordMap_ReturnsAllPythonKeywords (35 keywords)
- ValidateSyntax_ValidCode_ReturnsValid
- ValidateSyntax_InvalidCode_ReturnsDiagnostics
- ExtractIdentifiers_ReturnsUserDefinedNames

### 4. Testes de integracao
Adicionar a `CoreIntegrationTests.cs` ou criar novo arquivo:
- Fluxo completo: codigo Python -> traduzir para pt-br -> verificar resultado
- Fluxo reverso: codigo Python traduzido -> reverter para original
- Round-trip: codigo Python -> traduzir -> reverter -> verificar igualdade
- Traducoes Python carregam corretamente do disco (keywords-base.json + pt-br/python.json)
- LanguageRegistry resolve PythonAdapter para extensao `.py`

### Nota sobre ambiente
Testes do PythonTokenizerService e PythonAdapter requerem Python instalado na maquina. Considerar:
- Skip condicional se Python nao estiver disponivel
- Documentar pre-requisito nos testes
- CI/CD precisa ter Python instalado
