# Contexto - Tarefa 101

## Dependencias
Nenhuma dependencia estrita.

## Bloqueia
- Tarefa 104 (expansao de idiomas depende das tres camadas base)

## Arquivos relevantes
- packages/ide-adapters/vscode/package.json (strings externalizadas para %chave%)
- packages/ide-adapters/vscode/package.nls.json (EN, fallback)
- packages/ide-adapters/vscode/package.nls.pt-br.json
- packages/ide-adapters/vscode/package.nls.es.json

## Notas
- O VS Code seleciona o arquivo .nls conforme o idioma do editor; sem correspondencia, usa o package.nls.json (EN).
- Ids de locale seguem a convencao do VS Code: pt-br e es.
- Cobre apenas strings do manifesto (titulos de comando, descricoes de settings, descricao da extensao); mensagens em runtime sao a Tarefa 103.
- Camada A do plano de i18n.
- Implementado no PR #142.
