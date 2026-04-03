# Tarefa 087 - Indice reverso para IdentifierMapper e literais

## Prioridade: MEDIUM

## Problema
`IdentifierMapper.GetOriginal()` e `TranslationOrchestrator.TranslateAstReverse()` (para literais) fazem busca linear reversa: percorrem todos os entries do mapa para encontrar o original a partir da traducao. Isso e O(n*m) onde n = entries e m = idiomas.

### GetOriginal (IdentifierMapper.cs:135-141)
```csharp
foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Data.Identifiers)
{
    if (kvp.Value.ContainsKey(sourceLanguage)
        && string.Equals(kvp.Value[sourceLanguage], translatedIdentifier, StringComparison.Ordinal))
    {
        return OperationResultGeneric<string>.Ok(kvp.Key);
    }
}
```
Chamado uma vez por identifier no AST durante reverse translation. Para 100 identifiers e 50 entries no mapa = 5000 comparacoes.

### Literal reverse (TranslationOrchestrator.cs:230-239)
Mesmo padrao: percorre `IdentifierMapperService.Data.Literals` para encontrar o literal original a partir do traduzido.

## Solucao: indice reverso
Construir um indice reverso `Dictionary<string, string>` mapeando `(traducao, idioma) -> original` uma unica vez quando o mapa e carregado ou modificado. Lookup reverso vira O(1).

### Implementacao
1. Adicionar ao IdentifierMapper:
   - Campo `ReverseIdentifiers`: `Dictionary<string, Dictionary<string, string>>` (traducao -> (idioma -> original))
   - Campo `ReverseLiterals`: similar
   - Reconstruir indices em `LoadMap()` e apos cada `SetTranslation()`/`SetLiteralTranslation()`
2. `GetOriginal()` usa `ReverseIdentifiers[translatedIdentifier][sourceLanguage]` — O(1)
3. `TranslateAstReverse` para literais usa `ReverseLiterals` — O(1)
4. Manter metodos originais como fallback se indice nao tiver a entry (safety net)

## Impacto na performance
De O(n*m) para O(1) por lookup. Para arquivos com muitos identifiers e literais, a melhoria e significativa. Custo de construcao do indice e O(n) (uma vez).
