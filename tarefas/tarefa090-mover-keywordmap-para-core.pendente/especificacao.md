# Tarefa 090 - Mover logica de keyword map da extensao para o Core

## Prioridade: MEDIUM

## Problema
O `KeywordMapService` (packages/ide-adapters/vscode/src/providers/keywordMap.ts) reimplementa em TypeScript logica que o Core (C#) ja tem:

- Le `keywords-base.json` e `{idioma}/{linguagem}.json` do disco
- Faz JSON parse dos arquivos de traducao
- Constroi mapa reverso (keyword traduzida -> original)

O `NaturalLanguageProvider` no Core ja faz exatamente isso. A extensao duplica a logica, lendo os mesmos arquivos por conta propria.

Alem disso, `languageDetector.ts` / `languages.ts` duplicam o mapeamento extensao->linguagem que o `LanguageRegistry` no Core ja possui.

## Problema de design
A extensao deveria ter apenas logica especifica do VS Code (UI, commands, providers). Lookup de traducoes, mapeamento de linguagens, e qualquer logica reutilizavel entre IDEs deveria estar no Core.

Se um futuro adapter JetBrains ou Neovim for criado, teria que reimplementar o KeywordMapService novamente.

## Solucao
Expor o mapa de keywords traduzidas via o Host (CoreBridge) como um novo metodo:

1. **Novo metodo no Host**: `GetKeywordMap(fileExtension, targetLanguage)` retorna `Record<string, string>` (traduzida -> original)
2. **KeywordMapService simplificado**: em vez de ler JSON do disco, chama `coreBridge.getKeywordMap(filePath, language)` e cacheia o resultado
3. **CompletionProvider e HoverProvider**: sem mudancas (continuam usando KeywordMapService)
4. **Remover**: leitura de JSON e parse de traducoes do KeywordMapService

## Escopo
- Adicionar metodo `GetKeywordMap` ao Host Program.cs
- Adicionar metodo `getKeywordMap` ao CoreBridge
- Simplificar KeywordMapService para usar CoreBridge
- Remover leitura direta de ficheiros de traducao da extensao
- Avaliar se languageDetector pode usar info do Core (GetSupportedExtensions)
