# Tarefa 089 - Cache global para KeywordMapService

## Prioridade: LOW

## Problema
`KeywordMapService` (keywordMap.ts) le e parseia JSON do disco toda vez que o idioma ou linguagem de programacao muda. O cache e por instancia — se multiplos providers acessarem o servico, o cache pode nao ser compartilhado. Cada mudanca de idioma causa file I/O + JSON parse.

```typescript
private load(language: string, programmingLanguage: string): void {
    const base = JSON.parse(fs.readFileSync(basePath, 'utf-8'));
    const translation = JSON.parse(fs.readFileSync(translationPath, 'utf-8'));
    // Build the map
}
```

Na pratica, o KeywordMapService e instanciado uma vez e compartilhado (wire-up em extension.ts). Mas o cache e invalidado a cada mudanca de configuracao (configSubscription), forcando re-leitura do disco.

## Solucao
Implementar cache estatico a nivel de modulo, indexado por `${language}:${programmingLanguage}`. Manter o cache entre mudancas de configuracao — so invalidar se os arquivos de traducao no disco mudarem (ou nunca, ja que nao mudam durante uma sessao).

### Implementacao
1. Adicionar cache estatico: `static loadedMaps: Map<string, Record<string, string>>`
2. `load()` verifica cache antes de ler disco
3. `invalidate()` limpa apenas a chave afetada, nao todo o cache
4. Opcionalmente: pre-aquecer cache no activate para idioma configurado

## Impacto na performance
Elimina file I/O repetido. Cada mudanca de idioma que antes custava ~10-50ms (disco + parse) passa a ser O(1) apos primeira carga. Impacto real e pequeno porque mudancas de idioma sao infrequentes.
