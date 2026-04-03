# Tarefa 081 - Corrigir completionProvider hardcoded e nullable no csproj

## Prioridade: MEDIUM

## Problemas

### MEDIUM-002: completionProvider hardcoded "C# keyword"
Arquivo: completionProvider.ts linha 43

`item.detail = 'C# keyword: ${original}'` — hardcoded. Para arquivos Python, mostra "C# keyword" em vez de "Python keyword". O hoverProvider foi corrigido (usa LanguageDetector) mas o completionProvider nao.

Fix: usar LanguageDetector para detectar a linguagem do arquivo e mostrar o label correto, igual ao hoverProvider.

### MEDIUM-007: csproj Nullable enable contradiz convencao
Arquivos: Core.csproj, Core.Host.csproj, Core.Tests.csproj

Todos tem `<Nullable>enable</Nullable>` mas CONTRIBUTING.md proibe nullable. O codigo usa `null!` workarounds em varios lugares (ASTNode.Parent, OperationResultGeneric, FindScopedTranslation, PythonTokenizerService.Process). Program.cs usa `string?` (linha 94) e `TranslationOrchestrator?` (linha 197).

Opcoes:
a) Desabilitar nullable nos csproj (alinha com convencao, mas perde analise do compiler)
b) Atualizar CONTRIBUTING.md para permitir nullable em boundaries com APIs .NET
c) Manter como esta e documentar a inconsistencia

Recomendacao: opcao (b) — o nullable do compiler previne bugs reais.
