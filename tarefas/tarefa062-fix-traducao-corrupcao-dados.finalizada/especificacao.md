# Tarefa 062 - Corrigir traducao "e" que causa corrupcao de dados

## Prioridade: BLOCKER

## Problema (B1 da auditoria)
No pt-br-ascii/csharp.json, o ID 36 (is) e traduzido como "e". A variavel "e" e extremamente comum em C# (catch (Exception e), LINQ .Where(e => ...), event handlers (sender, e)). O ReverseSubstituteKeywords converte toda ocorrencia de "e" de volta para "is", corrompendo o codigo ao salvar na view editavel.

O pt-br (com acento) traduz "is" como "é". O engine usa OrdinalIgnoreCase que e accent-sensitive, entao "é" != "e". O bug afeta APENAS pt-br-ascii.

## Escopo
- Repositorio: babel-tcc-translations
- Trocar "e" para "igual" no pt-br-ascii/csharp.json (ID 36)
- Verificar se pt-br-ascii/python.json tem o mesmo problema (ID 23, "is")
- Rodar validate.py para garantir unicidade

## Impacto
Corrupcao silenciosa de codigo no round-trip com pt-br-ascii.
