# Tarefa 073 - Testes IdentifierMapper com multiplas linguagens no mesmo projeto

## Prioridade: MEDIUM

## Problema
O `identifier-map.json` e por projeto, nao por linguagem. Se um projeto tiver arquivos C# e Python com anotacoes tradu, os mapeamentos colidem no mesmo `IdentifierMapData.Identifiers`. Um identifier `Add` mapeado em C# como `Somar` seria aplicado no Python tambem.

## Escopo

### Testes a criar
1. **Colisao de identifiers**: Projeto com `Calculator.cs` (tradu Calculator->Calculadora) e `calculator.py` (identifier Calculator diferente). Verificar que mapeamentos nao interferem.
2. **Isolamento por operacao**: Traduzir .cs, depois traduzir .py no mesmo orchestrator. Verificar que identifiers do .cs nao vazam para o .py.
3. **Round-trip multi-linguagem**: Traduzir .cs, traduzir .py, reverter .py, reverter .cs — verificar que ambos retornam ao original.

### Avaliar necessidade de namespacing
Se os testes revelarem colisao, propor solucao:
- Prefixar identifiers com extensao/linguagem no IdentifierMapData
- Ou separar identifier-map por linguagem (identifier-map-csharp.json, identifier-map-python.json)
