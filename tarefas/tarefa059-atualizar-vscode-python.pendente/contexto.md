# Contexto - Tarefa 059

## Dependencias
- PR #47 (KeywordMapService dinamico) — ja mergeado em main. A secao 6 da especificacao assume que o KeywordMapService existe. Se por algum motivo for revertido, os providers (completionProvider, hoverProvider) precisariam de mudancas manuais para suportar Python.

## Bloqueia
- Nenhuma diretamente

## Arquivos afetados
- packages/ide-adapters/vscode/package.json (activationEvents, languages, grammars)
- packages/ide-adapters/vscode/syntaxes/ (novo mlc-python.tmLanguage.json)
- packages/ide-adapters/vscode/src/services/languageDetector.ts
- packages/ide-adapters/vscode/src/extension.ts
- packages/ide-adapters/vscode/src/providers/hoverProvider.ts
- packages/ide-adapters/vscode/src/services/autoTranslateManager.ts (verificar)
- packages/ide-adapters/vscode/src/services/coreBridge.ts (verificar)

## Notas
- Esta tarefa e independente das tarefas de backend (052-058) e pode ser feita em paralelo.
- Porem, para testar end-to-end, o backend precisa estar pronto (tarefas 052-058).
- O KeywordMapService (providers/keywordMap.ts) carrega traducoes dinamicamente via LanguageDetector.detectLanguage(). Apos atualizar o LanguageDetector para reconhecer .py, completion e hover funcionam para Python automaticamente.
- Verificar se ha testes da extensao VS Code que precisam ser atualizados.
