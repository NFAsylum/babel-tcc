# Contexto - Tarefa 096

## Dependencias
- Tarefa 087 (selecao de idioma por linguagem — ja finalizada, PR #111)

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/ide-adapters/vscode/src/extension.ts (callback do selectLanguage)
- packages/ide-adapters/vscode/test/extension.test.ts (testes do comando)

## Notas
- Mudanca pequena e localizada: apenas o callback do selectLanguage
- SUPPORTED_LANGUAGES em src/config/languages.ts tem a lista de linguagens
- configurationService.setLanguageOverride ja existe (tarefa 087)
- Identificado na review do PR #111: utilizador nao consegue configurar
  idioma para linguagem que nao esta aberta no editor
