# Definition of Done - Tarefa 099

## TextScan generico
- [ ] LanguageScanRules definido com configuracao de comentarios/strings
- [ ] TextScanTranslator aceita LanguageScanRules como parametro
- [ ] CSharpScanRules extrai regras C# (antes hardcoded)
- [ ] PythonScanRules extrai regras Python
- [ ] Interface ITextScannable criada (sem breaking change em ILanguageAdapter)
- [ ] CSharpAdapter e PythonAdapter implementam ITextScannable

## Integracao
- [ ] TranslationOrchestrator usa Text Scan quando adapter is ITextScannable
- [ ] Python forward translation usa Text Scan (sem tradu)
- [ ] Fallback transparente para parser quando adapter nao e ITextScannable

## Testes
- [ ] Edge cases C# continuam passando com regras extraidas
- [ ] Edge cases Python passam com regras novas
- [ ] Equivalencia: Text Scan Python vs tokenizer Python para mesmo input
- [ ] Performance: Python Text Scan < 1ms vs ~8ms tokenizer
- [ ] Todos os testes existentes passam (zero regressoes)

## Documentacao
- [ ] adding-new-language.md atualizado com caminho Text Scan
- [ ] Tabela de linguagens suportaveis documentada
