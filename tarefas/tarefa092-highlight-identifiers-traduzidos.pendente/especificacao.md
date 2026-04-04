# Tarefa 092 - Highlight de identifiers traduzidos via semantic tokens

## Prioridade: MEDIUM

## Problema
O SemanticKeywordProvider (tarefa 086) destaca keywords traduzidas no codigo traduzido. Porem identifiers traduzidos via anotacoes tradu (ex: Calculator -> Calculadora) nao tem nenhuma distincao visual. O usuario nao sabe quais palavras sao identifiers traduzidos e quais sao codigo original.

## Solucao
Expandir o SemanticKeywordProvider (ou criar provider separado) para tambem marcar identifiers traduzidos com um semantic token type distinto.

### Fluxo
1. O Core ja tem os mapeamentos de identifiers via IdentifierMapper (carregados das anotacoes tradu)
2. Expor mapa de identifiers traduzidos via Host (novo metodo GetIdentifierMap ou junto com GetKeywordMap)
3. O semantic provider consulta o mapa e marca palavras que matcham identifiers traduzidos
4. VS Code aplica highlighting diferenciado baseado no tema

### Semantic token types
- Keywords traduzidas: `keyword` (ja implementado)
- Identifiers traduzidos: `variable` com modifier `declaration`, ou tipo custom registrado no legend

### Resultado visual
O usuario ve no codigo traduzido:
- Keywords traduzidas com cor de keyword (ex: roxo)
- Identifiers traduzidos com cor/estilo diferente (ex: italico, outra cor)
- Codigo nao traduzido com cor padrao

Isso da feedback visual imediato sobre o que foi traduzido e o que nao foi.

## Dependencia com tarefa 090
A tarefa 090 (mover KeywordMapService para Core) propoe expor mapas via CoreBridge. Os identifiers poderiam ser expostos junto, evitando duplicacao de IPC.
