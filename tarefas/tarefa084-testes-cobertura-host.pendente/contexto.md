# Contexto - Tarefa 084

## Dependencias
- Tarefa 075 (testes CreateRegistry — ja finalizada, base existente)
- Tarefa 050 (processo persistente — GetOrCreateOrchestrator adicionado)

## Bloqueia
- Nenhuma

## Arquivos relevantes
- packages/core/MultiLingualCode.Core.Host/Program.cs (codigo a testar)
- packages/core/MultiLingualCode.Core.Host/CoreResponse.cs
- packages/core/MultiLingualCode.Core.Host/TranslateRequest.cs
- packages/core/MultiLingualCode.Core.Host/ReverseTranslateRequest.cs
- packages/core/MultiLingualCode.Core.Host/ValidateRequest.cs
- packages/core/MultiLingualCode.Core.Tests/Services/LanguageRegistryTests.cs (testes existentes)
- packages/core/MultiLingualCode.Core.Tests/MultiLingualCode.Core.Tests.csproj (ja tem ref ao Host)

## Notas
- Todos os metodos do Program.cs sao public static — faceis de testar
- O csproj de testes ja tem ProjectReference ao Host (adicionado na tarefa 075)
- TestData/translations tem apenas pt-br (suficiente para testes)
- Testes do Host devem usar TestData real, nao mocks
