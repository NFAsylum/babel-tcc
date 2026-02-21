# Setup do Ambiente de Desenvolvimento

## Pre-requisitos

| Ferramenta | Versao Minima | Download |
|---|---|---|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ | https://nodejs.org |
| VS Code | 1.80+ | https://code.visualstudio.com |
| Git | 2.30+ | https://git-scm.com |

### Extensoes VS Code Recomendadas

- C# Dev Kit (Microsoft)
- ESLint
- Prettier
- GitLens

## Clonar Repositorios

```bash
# Repositorio principal
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc

# Repositorio de traducoes (em pasta separada ou como submodulo)
git clone https://github.com/NFAsylum/babel-tcc-translations.git
```

## Build do Core (C#)

```bash
cd packages/core

# Restaurar dependencias
dotnet restore

# Build
dotnet build

# Rodar testes
dotnet test
```

## Build da Extension (TypeScript)

```bash
cd packages/ide-adapters/vscode

# Instalar dependencias
npm install

# Build
npm run build

# Lint
npm run lint
```

## Rodar a Extension em Modo Debug

1. Abrir a pasta `packages/ide-adapters/vscode` no VS Code
2. Pressionar `F5` (ou Run > Start Debugging)
3. Uma nova janela do VS Code abre com a extensao carregada
4. Abrir um arquivo `.cs` para testar

## Estrutura de Pastas

```
babel-tcc/
├── packages/
│   ├── core/                          # C# Core Engine
│   │   ├── MultiLingualCode.Core/     # Biblioteca principal
│   │   └── MultiLingualCode.Core.Tests/
│   └── ide-adapters/
│       └── vscode/                    # VS Code Extension
│           ├── src/
│           └── package.json
├── examples/                          # Exemplos de uso
├── docs/                              # Documentacao
├── tarefas/                           # Tarefas do projeto
└── README.md
```

## Variaveis de Ambiente

Nenhuma variavel de ambiente e necessaria para desenvolvimento local. A extensao encontra o Core automaticamente via caminho relativo.

## Problemas Comuns

### dotnet nao encontrado
Verificar se o .NET SDK esta no PATH:
```bash
dotnet --version
```

### npm install falha
Limpar cache e tentar novamente:
```bash
npm cache clean --force
rm -rf node_modules
npm install
```

### Extension nao ativa
Verificar Output Channel "MultiLingual Code" no VS Code para logs de erro.
