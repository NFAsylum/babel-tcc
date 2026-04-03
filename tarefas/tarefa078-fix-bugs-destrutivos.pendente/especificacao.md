# Tarefa 078 - Corrigir bugs destrutivos (ApplyTradu + refreshingPaths)

## Prioridade: HIGH

## Problemas

### HIGH-001: ApplyTraduAnnotations destroi identifier-map.json
Arquivo: TranslationOrchestrator.cs linhas 258-300

`ApplyTraduAnnotations` faz `Data.Identifiers.Clear()` e `Data.Literals.Clear()` no inicio, depois salva o mapa no disco (linha 299). Se o IdentifierMapper tinha mapeamentos carregados de `identifier-map.json`, eles sao destruidos permanentemente e substituidos apenas pelas anotacoes tradu encontradas no arquivo atual.

Fix: nao limpar mapeamentos existentes — fazer merge em vez de replace. Ou nao salvar no disco durante traducao (salvar apenas quando usuario edita explicitamente).

### HIGH-003: refreshingPaths nunca removido em caso de erro
Arquivo: translatedContentProvider.ts linhas 103-109

`refreshingPaths.add(originalPath)` e executado antes de `applyEdit()`. Se `applyEdit` lanca excecao, o `delete` no finally nunca executa (nao ha finally — o catch na linha 107 nao faz delete). O path fica em `refreshingPaths` permanentemente, bloqueando writes futuros para esse arquivo.

Fix: usar try/finally para garantir que `refreshingPaths.delete(originalPath)` executa sempre.
