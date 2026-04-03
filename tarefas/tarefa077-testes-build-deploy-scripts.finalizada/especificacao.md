# Tarefa 077 - Testes de build e deploy para scripts de subprocesso

## Prioridade: MEDIUM

## Problema
Scripts externos (como `tokenizer_service.py`) precisam ser copiados para o output directory via `.csproj` config. Se a config for esquecida ao adicionar uma nova linguagem com subprocesso, o adapter falha em runtime com "script not found". Isso nao e detectado por testes unitarios que instanciam o adapter diretamente.

## Escopo

### 1. Teste de build que verifica presenca de scripts
- Apos `dotnet build`, verificar que `tokenizer_service.py` existe no output directory
- Caminho esperado: `{outputDir}/LanguageAdapters/Python/tokenizer_service.py`
- Teste pode ser um script no CI ou um teste C# que verifica `File.Exists(PythonTokenizerService.GetScriptPath())`

### 2. Teste de consistencia .csproj
- Verificar que cada arquivo .py/.js/etc em LanguageAdapters/ tem entrada correspondente no .csproj com CopyToOutputDirectory
- Pode ser um teste que lista arquivos non-.cs na pasta e verifica que estao no .csproj

### 3. Documentar requisito no adding-new-language.md
- Adicionar passo sobre configuracao do .csproj ao guia de adicionar linguagem
- Incluir exemplo do XML necessario
