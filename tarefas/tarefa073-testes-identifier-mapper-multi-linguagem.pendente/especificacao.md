# Tarefa 073 - Testes IdentifierMapper com multiplas linguagens no mesmo projeto

## Prioridade: MEDIUM

## Problema
O `identifier-map.json` e por projeto, nao por linguagem. `ApplyTraduAnnotations` faz `Clear()` no inicio de cada chamada e `SaveMap()` no final — reconstruindo o mapa do zero a partir das anotacoes tradu do arquivo atual.

O cenario de problema real: se o usuario traduz `Calculator.cs` (que salva mapeamentos de C# no identifier-map.json), e depois traduz `calculator.py` (que faz Clear + salva mapeamentos de Python), os mapeamentos do C# sao **permanentemente destruidos do disco**. Na proxima vez que o usuario abrir o .cs, os mapeamentos nao existem mais.

Nota: colisao em memoria entre chamadas sequenciais NAO acontece porque `Clear()` limpa tudo entre chamadas. O problema e **perda de dados persistidos**.

## Escopo

### Testes a criar
1. **Perda de mapeamentos persistidos**: Traduzir .cs com tradu annotations, verificar que identifier-map.json tem mapeamentos. Traduzir .py, verificar que identifier-map.json perdeu mapeamentos do .cs.
2. **Round-trip multi-linguagem**: Traduzir .cs, traduzir .py, reverter .py (OK), reverter .cs (mapeamentos perdidos?).
3. **SaveMap com dados parciais**: Verificar que SaveMap apos Clear+rebuild salva apenas anotacoes do arquivo atual, nao tudo que estava no disco.

### Avaliar necessidade de fix
Se os testes confirmarem perda de dados:
- Opcao A: Nao fazer Clear — merge em vez de replace
- Opcao B: Separar identifier-map por linguagem (identifier-map-csharp.json, identifier-map-python.json)
- Opcao C: Nao salvar no disco durante traducao automatica (salvar apenas em operacoes explicitas)
