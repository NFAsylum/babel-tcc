# Tarefa 095 - Centralizar config keys e constantes

## Prioridade: LOW

## Problema
Config keys como 'babel-tcc.enabled', 'babel-tcc.language', etc. aparecem
como strings literais em multiplos arquivos (configurationService.ts,
coreBridge.ts, testes). Se o prefixo mudar (ex: para 'multilingual-code'),
todas as ocorrencias precisam ser encontradas e atualizadas manualmente.

Paths de traducao ('natural-languages', 'programming-languages',
'keywords-base.json') tambem aparecem como magic strings em keywordMap.ts
e Program.cs.

## Solucao
Criar modulo de constantes centralizado:

```typescript
// src/config/constants.ts
export const CONFIG_SECTION = 'babel-tcc';
export const CONFIG_KEYS = {
    ENABLED: `${CONFIG_SECTION}.enabled`,
    LANGUAGE: `${CONFIG_SECTION}.language`,
    TRANSLATIONS_PATH: `${CONFIG_SECTION}.translationsPath`,
    READONLY: `${CONFIG_SECTION}.readonly`,
} as const;

export const TRANSLATION_PATHS = {
    NATURAL_LANGUAGES: 'natural-languages',
    PROGRAMMING_LANGUAGES: 'programming-languages',
    KEYWORDS_BASE: 'keywords-base.json',
} as const;
```

## Escopo
- Criar src/config/constants.ts com config keys e paths
- Atualizar configurationService.ts para usar constantes
- Atualizar coreBridge.ts para usar constantes
- Atualizar keywordMap.ts para usar constantes
- Atualizar testes para usar constantes
- ESLint + testes passam
