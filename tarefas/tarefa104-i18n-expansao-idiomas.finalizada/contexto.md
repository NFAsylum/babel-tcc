# Contexto - Tarefa 104

## Dependencias
- Tarefa 101 (Camada A - manifesto .nls)
- Tarefa 103 (Camada C - strings em runtime l10n)

## Bloqueia
Nenhuma.

## Arquivos relevantes
- packages/ide-adapters/vscode/package.nls.<locale>.json
- packages/ide-adapters/vscode/l10n/bundle.l10n.<locale>.json

## Notas
- So iniciar apos EN/PT-BR/ES estarem estaveis (camadas A e C).
- Ids de locale do VS Code: fr, de, it, ja (Japones e ja, nao jp).
- Escopo decidido: APENAS UI (manifesto .nls + l10n runtime). READMEs em FR/DE/IT/JP ficaram
  fora de escopo (muito conteudo/manutencao para baixo retorno; o Marketplace mostra um README
  unico e PT/EN/ES ja cobrem). A traducao de codigo para esses idiomas ja funcionava (tabelas
  fr-fr/de-de/it-it/ja-jp-romaji ja existiam); esta tarefa so localiza a interface da extensao.
