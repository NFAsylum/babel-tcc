# Contexto - Tarefa 103

## Dependencias
Nenhuma dependencia estrita.

## Bloqueia
- Tarefa 104 (expansao de idiomas depende das tres camadas base)

## Arquivos relevantes
- packages/ide-adapters/vscode/src/ (mensagens em runtime hardcoded)
- packages/ide-adapters/vscode/l10n/ (bundles a criar)
- packages/ide-adapters/vscode/package.json (campo l10n)

## Notas
- Usa a API vscode.l10n com bundles l10n/bundle.l10n.<locale>.json.
- Primeiro levantar TODAS as mensagens hardcoded voltadas ao usuario (quick-picks, notificacoes, avisos) e listar; depois migrar uma por uma.
- Os testes ja indicam mensagens de aviso hardcoded (ex.: no active editor, unsupported file).
- Camada C do plano de i18n.
- Idiomas da primeira leva: EN, PT-BR, ES.
