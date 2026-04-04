# Contexto - Tarefa 086

## Dependencias
- Nenhuma (KeywordMapService e registro central de linguagens ja existem)

## Bloqueia
- Nenhuma

## Arquivos afetados
- Novo: packages/ide-adapters/vscode/src/providers/semanticKeywordProvider.ts
- packages/ide-adapters/vscode/src/extension.ts (registrar provider)
- packages/ide-adapters/vscode/syntaxes/mlc-csharp.tmLanguage.json (remover keywords hardcoded)
- packages/ide-adapters/vscode/syntaxes/mlc-python.tmLanguage.json (sem mudanca — ja nao tem keywords)
- Novos testes para o semantic provider

## Notas
- O KeywordMapService (PR #47) ja carrega traducoes dinamicamente via LanguageDetector — e a base perfeita para o semantic provider
- A grammar mlc-python.tmLanguage.json ja nao destaca keywords (removido na tarefa 079) — sera beneficiada automaticamente
- A grammar mlc-csharp.tmLanguage.json tem keywords hardcoded para pt-br-ascii — sera simplificada
- Semantic tokens sao suportados desde VS Code 1.44 (nosso minimo e 1.85)
