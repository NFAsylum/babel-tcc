# Tarefa 067 - Padronizar nomes de testes C# para convencao de 3 partes

## Prioridade: LOW

## Problema (L1 da auditoria)
185 dos 416+ testes C# (~44%) nao seguem a convencao MetodoTestado_Cenario_ResultadoEsperado do CONTRIBUTING.md. A maioria usa 2 partes (Sujeito_Comportamento).

## Escopo
- Renomear testes para seguir o padrao MetodoTestado_Cenario_ResultadoEsperado
- Priorizar por arquivo (um arquivo por commit para facilitar review)
- Garantir que todos os testes passam apos cada rename

## Notas
- Renomear testes nao muda funcionalidade — puramente cosmético
- Pode ser feito incrementalmente (nao precisa de tudo num PR so)
