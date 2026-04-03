# Contribuir para o babel-tcc

Obrigado pelo interesse em contribuir! Este documento explica como participar no desenvolvimento.

## Como contribuir

### Reportar bugs

1. Verifique se o bug ja foi reportado nas [Issues](https://github.com/NFAsylum/babel-tcc/issues)
2. Se nao, crie uma nova issue usando o template de bug report
3. Inclua passos para reproduzir, comportamento esperado e atual

### Sugerir funcionalidades

1. Abra uma issue usando o template de feature request
2. Descreva o problema que a funcionalidade resolve
3. Inclua exemplos de uso se possivel

### Submeter codigo

1. Fork o repositorio
2. Crie uma branch a partir de `main`: `git checkout -b minha-feature`
3. Faca as alteracoes seguindo as convencoes de codigo
4. Adicione testes para novas funcionalidades
5. Certifique-se que todos os testes passam
6. Abra um Pull Request

## Configurar ambiente de desenvolvimento

### Pre-requisitos

- .NET 8 SDK
- Node.js 18+
- VS Code (para testar a extensao)

### Build

```bash
# Core (C#)
cd packages/core
dotnet build

# VS Code Extension (TypeScript)
cd packages/ide-adapters/vscode
npm install
npm run build
```

### Testes

```bash
# Testes do Core
dotnet test packages/core/MultiLingualCode.Core.Tests

# Testes da extensao
cd packages/ide-adapters/vscode
npm test
```

## Convencoes de codigo

### C# (Core)

- Nao usar `var` - tipos explicitos sempre
- Nao usar `private` ou `internal` - tudo `public`
- Nao usar `throw` - usar `OperationResult` para erros
- Evitar nullable (`?`, `??`) — permitido em boundaries com APIs .NET que retornam null (ex: `Environment.GetEnvironmentVariable`, `Path.GetDirectoryName`, `Version.TryParse`)
- Uma classe por ficheiro
- Nomes de testes: `MetodoTestado_Cenario_ResultadoEsperado`

### TypeScript (VS Code Extension)

- Strict mode habilitado
- Usar `const` em vez de `let` quando possivel
- Nunca usar `var`
- Tipos explicitos em parametros e retornos
- Aspas simples
- Ficheiros em camelCase, classes em PascalCase

## Estrutura do projeto

```
babel-tcc/
  packages/
    core/                          # Motor de traducao (C#/.NET 8)
      MultiLingualCode.Core/       # Biblioteca principal
      MultiLingualCode.Core.Host/  # CLI para comunicacao com IDE
      MultiLingualCode.Core.Tests/ # Testes unitarios
    ide-adapters/
      vscode/                      # Extensao VS Code
  examples/                        # Exemplos de uso
  docs/                           # Documentacao
```

## Adicionar novas funcionalidades

- **Nova linguagem de programacao**: Ver [docs/developer-guide/adding-new-language.md](docs/developer-guide/adding-new-language.md)
- **Novo IDE**: Ver [docs/developer-guide/adding-new-ide.md](docs/developer-guide/adding-new-ide.md)
- **Novas traducoes**: Ver [docs/developer-guide/creating-translations.md](docs/developer-guide/creating-translations.md)

## Processo de review

1. PRs devem ter descricao clara do que foi alterado
2. Todos os testes devem passar
3. Novas funcionalidades devem incluir testes
4. Codigo deve seguir as convencoes documentadas

## Licenca

Ao contribuir, voce concorda que suas contribuicoes serao licenciadas sob a mesma licenca do projeto.
