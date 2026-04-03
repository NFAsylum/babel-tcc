# Contexto - Tarefa 065

## Dependencias
- Nenhuma

## Bloqueia
- Nenhuma

## Arquivos afetados
- packages/core/MultiLingualCode.Core.Tests/ (varios)
- packages/core/MultiLingualCode.Core.Host/Program.cs (3 nullable aceitos como excecao)

## Notas
- Os 3 nullable em Program.cs sao boundary com APIs .NET que retornam null (aceitar)
- Rodar dotnet test apos cada mudanca para garantir zero regressoes
