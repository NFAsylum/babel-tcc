# Tarefa 088 - Otimizar FindScopedTranslation com Dictionary

## Prioridade: LOW

## Problema
`FindScopedTranslation()` (TranslationOrchestrator.cs:340-351) faz busca linear numa List para cada identifier no AST. Chamado centenas de vezes por arquivo (uma vez por IdentifierNode no AST).

```csharp
foreach ((string scopedName, string translation, int startLine, int endLine) in ScopedTranslations)
{
    if (scopedName == name && line >= startLine && line <= endLine)
    {
        return translation;
    }
}
```

Na pratica, `ScopedTranslations` e tipicamente pequena (0-10 entries de parameter mappings), entao o impacto e menor. Mas a estrutura de dados e subotima.

## Solucao
Trocar `List<(string, string, int, int)>` por `Dictionary<string, List<(string Translation, int StartLine, int EndLine)>>` indexado por nome.

### Implementacao
1. Mudar tipo de `ScopedTranslations` para Dictionary
2. Em `ApplyTraduAnnotations`, construir o dictionary ao adicionar entries
3. `FindScopedTranslation` faz lookup por nome (O(1)), depois percorre a lista de ranges para aquele nome (tipicamente 1-2 entries)

## Impacto na performance
De O(n) para O(1) + O(r) onde r = ranges para aquele nome (tipicamente 1). Melhoria real e pequena porque n tambem e pequeno, mas a estrutura correta previne degradacao se o numero de scoped translations crescer.
