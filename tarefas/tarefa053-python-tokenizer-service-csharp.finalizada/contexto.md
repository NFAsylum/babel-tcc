# Contexto - Tarefa 053

## Dependencias
- Tarefa 052 (script Python tokenizer_service.py)

## Bloqueia
- Tarefa 056 (PythonAdapter depende deste servico)

## Arquivos relevantes
- packages/core/MultiLingualCode.Core/LanguageAdapters/Python/tokenizer_service.py (tarefa 052)
- packages/core/MultiLingualCode.Core.Host/Program.cs (referencia de como processos sao gerenciados)

## Notas
- Performance medida: startup ~28ms (unico), requests subsequentes ~0.13ms
- Padrao similar ao coreBridge.ts na extensao VS Code (que invoca o Host .NET)
- A tarefa 050 (processo persistente CoreBridge) descreve um padrao similar do lado TypeScript — reutilizar conceitos de la
- O `PythonTokenizerService` sera usado internamente pelo `PythonAdapter`, nao exposto diretamente
