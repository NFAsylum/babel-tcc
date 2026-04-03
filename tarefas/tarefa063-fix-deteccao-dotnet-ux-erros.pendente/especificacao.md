# Tarefa 063 - Detectar .NET ausente e padronizar mensagens de erro

## Prioridade: BLOCKER

## Problemas (B2 + B3 + M1 da auditoria)

### B2: Nenhum erro claro quando .NET nao esta instalado
Se dotnet nao esta no PATH, o spawn falha silenciosamente. O crashCount incrementa e apos 3 crashes aparece uma mensagem em portugues. O usuario nao recebe feedback imediato e actionable.

### B3: Mensagens de erro em idiomas misturados
- Portugues: "o motor de traducao esta instavel" (coreBridge.ts:143)
- Ingles: "No active editor" (extension.ts:162)
- Ingles: "File type not supported" (extension.ts:168)
- Ingles: "Failed to reverse translate" (translatedContentProvider.ts:118)

A extensao sera publicada no Marketplace global. Mensagens devem ser consistentes.

### M1: Falha de traducao silenciosa
translatedContentProvider.ts:169-183 retorna codigo original se traducao falhar. Nenhuma indicacao visual ao usuario.

## Escopo

### B2: Detectar .NET no activate()
- No activate(), verificar `dotnet --version` via spawn
- Se nao encontrado, mostrar erro com link de instalacao
- Se encontrado mas versao < 8.0, mostrar warning

### B3: Padronizar mensagens para ingles
- coreBridge.ts:143: "o motor de traducao esta instavel" -> mensagem em ingles
- Verificar todas as mensagens user-facing e padronizar para ingles

### M1: Indicacao visual de falha de traducao
- Em translatedContentProvider.ts, quando traducao falha, adicionar indicacao visual
- Opcoes: statusbar warning, notification discreta, comentario no topo do arquivo traduzido
