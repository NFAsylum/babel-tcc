# Contexto - Tarefa 073

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma (mas resultado pode gerar tarefa de fix se colisao confirmada)

## Arquivos afetados
- packages/core/MultiLingualCode.Core.Tests/Integration/CoreIntegrationTests.cs (novos testes)
- packages/core/MultiLingualCode.Core/Services/IdentifierMapper.cs (se precisar de fix)

## Notas
- ApplyTraduAnnotations faz `IdentifierMapperService.Data.Identifiers.Clear()` no inicio — isso ja isola por chamada, mas nao por linguagem numa mesma sessao
- O IdentifierMapper.LoadMap() carrega de `{projectPath}/.multilingual/identifier-map.json` — um arquivo unico
