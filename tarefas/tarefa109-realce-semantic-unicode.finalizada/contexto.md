# Contexto - Tarefa 109

## Dependencias
Nenhuma.

## Bloqueia
Nenhuma. Melhora a fidelidade visual da demonstracao (tarefa 043) em idiomas nao-latinos.

## Arquivos relevantes
- packages/ide-adapters/vscode/src/providers/semanticKeywordProvider.ts (regex de tokenizacao)
- packages/ide-adapters/vscode/test/providers/semanticKeywordProvider.test.ts (testes)

## Notas
- Bug descoberto ao observar que chines e arabe apareciam com cor diferente dos demais idiomas.
- Mesma classe de bug da tarefa 108 (suposicao latino-centrica em regex).
- RTL do arabe e do editor, nao da extensao - fora de escopo.
