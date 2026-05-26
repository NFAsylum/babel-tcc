# Contexto - Tarefa 102

## Dependencias
Nenhuma dependencia estrita.

## Bloqueia
- Tarefa 104 (expansao de idiomas depende das tres camadas base)

## Arquivos relevantes
- README.md (canonico, empacotado no .vsix)
- README.pt-br.md
- README.es.md
- scripts/copy-readme.js (empacota o README canonico na extensao)

## Notas
- Marketplace e Open VSX renderizam um unico README por listagem; nenhum troca por locale do usuario.
- Decisao pendente: idioma canonico empacotado (EN recomendado pelo alcance internacional, ou PT-BR).
- Cada README leva uma linha de navegacao no topo ([English] | [Portugues] | [Espanhol]).
- Camada B do plano de i18n.
- Idiomas da primeira leva: EN, PT-BR, ES.
