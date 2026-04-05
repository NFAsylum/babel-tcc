# Tarefa 096 - Melhorar comando selectLanguage com lista completa de linguagens

## Prioridade: MEDIUM

## Problema
O comando `babel-tcc.selectLanguage` (extension.ts) detecta a linguagem do
ficheiro activo no editor e oferece apenas duas opcoes de scope:
- "All languages (global)"
- "{LinguagemActiva} only"

Se o utilizador quer configurar o idioma para Python mas tem um ficheiro C#
aberto, nao consegue. Tem que abrir um .py primeiro e repetir o comando.
Sem ficheiro aberto, o scope per-language nao aparece.

## Comportamento actual
```
1. QuickPick: "All languages (global)" | "CSharp only" (se .cs aberto)
2. QuickPick: lista de idiomas
```
Linguagem activa e a unica opcao per-language.

## Comportamento desejado
```
1. QuickPick:
   - "$(globe) All languages (global)"
   - "$(file-code) CSharp only"     <- destaque se .cs activo
   - "$(file-code) Python only"
   (todas as linguagens registradas, activa em destaque)
2. QuickPick: lista de idiomas
```

## Implementacao

### Extrair logica de construcao para funcao pura testavel
A logica de construcao dos scope items deve ser uma funcao pura exportada,
nao inline no callback do comando. Isso permite testar directamente sem
depender de mocks do VS Code QuickPick.

```typescript
// Em extension.ts ou num modulo separado
import { SUPPORTED_LANGUAGES } from './config/languages';

export type ScopeItem = { label: string; scope: 'global' | 'language'; language: string | undefined };

export function buildScopeItems(activeProgrammingLanguage: string | undefined): ScopeItem[] {
  const items: ScopeItem[] = [
    { label: '$(globe) All languages (global)', scope: 'global', language: undefined },
  ];

  for (const lang of SUPPORTED_LANGUAGES) {
    const isActive: boolean = lang.name === activeProgrammingLanguage;
    items.push({
      label: `$(file-code) ${lang.name} only${isActive ? ' (active)' : ''}`,
      scope: 'language',
      language: lang.name,
    });
  }

  return items;
}
```

O callback do selectLanguage chama `buildScopeItems(programmingLanguage)`
e passa o resultado ao `showQuickPick`. Os testes chamam `buildScopeItems`
directamente e verificam os items retornados.

### Usar a linguagem seleccionada (nao a detectada)
O `setLanguageOverride` deve usar a linguagem do item seleccionado, nao
a linguagem detectada do editor:

```typescript
if (scopeChoice.scope === 'language' && scopeChoice.language) {
  await configService.setLanguageOverride(scopeChoice.language, selected);
}
```

### Sem ficheiro aberto
Todas as linguagens aparecem na lista sem destaque. O utilizador pode
configurar qualquer linguagem independente do contexto.

## Escopo
- Extrair buildScopeItems como funcao pura exportada
- Modificar callback do selectLanguage para usar buildScopeItems
- Importar SUPPORTED_LANGUAGES de config/languages.ts
- Nao alterar configurationService, statusBar, ou outros providers
- Manter retrocompatibilidade: "All languages" continua como primeira opcao
- Testes chamam buildScopeItems directamente (sem mocks de QuickPick)

## Notas
- SUPPORTED_LANGUAGES ja existe e e usado em testes de consistencia (tarefa 074)
- A lista tem actualmente 2 linguagens (CSharp, Python) — 3 itens no total
- Se linguagens futuras forem adicionadas ao registro, aparecem automaticamente
