# Contribuir para o babel-tcc

Obrigado pelo interesse em contribuir! Este documento explica como participar no desenvolvimento.

## Como contribuir

### Reportar bugs

1. Verifique se o bug já foi reportado nas [Issues](https://github.com/NFAsylum/babel-tcc/issues)
2. Se não, crie uma nova issue usando o template de bug report
3. Inclua passos para reproduzir, comportamento esperado e atual

### Sugerir funcionalidades

1. Abra uma issue usando o template de feature request
2. Descreva o problema que a funcionalidade resolve
3. Inclua exemplos de uso se possível

### Submeter código

1. Fork o repositório
2. Crie uma branch a partir de `main`: `git checkout -b minha-feature`
3. Faça as alterações seguindo as convenções de código
4. Adicione testes para novas funcionalidades
5. Certifique-se que todos os testes passam
6. Abra um Pull Request

## Configurar ambiente de desenvolvimento

### Pré-requisitos

- .NET 8 SDK
- Node.js 20+
- Python 3.8+ (para suporte a Python)
- JDK 17 (para o plugin IntelliJ)
- VS Code (para testar a extensão)

O build do plugin IntelliJ espera o repositório `babel-tcc-translations` clonado
como **irmão** de `babel-tcc`; a task `bundleTranslations` aborta com mensagem
explícita se não encontrar.

### Build

```bash
# Core (C#)
cd packages/core
dotnet build

# VS Code Extension (TypeScript)
cd packages/ide-adapters/vscode
npm install
npm run build

# Plugin IntelliJ (Kotlin)
cd packages/ide-adapters/intellij
./gradlew build
```

### Testes

```bash
# Testes do Core
dotnet test packages/core/MultiLingualCode.Core.Tests

# Testes da extensão
cd packages/ide-adapters/vscode
npm test

# Testes do plugin IntelliJ
cd packages/ide-adapters/intellij
./gradlew test
```

## Convenções de código

As regras de código do projeto vivem em um único lugar:
**[docs/padroes-codigo.md](docs/padroes-codigo.md)**.

Lá estão as proibições absolutas que valem para todas as linguagens, as práticas
que exigem justificativa explícita, as convenções de nomenclatura por linguagem
e as regras de Git.

Não duplique regra de código neste arquivo. Uma cópia resumida aqui já divergiu
do documento real uma vez, e uma contribuição seguiu o resumo achando que era a
regra completa.

## Estrutura do projeto

```
babel-tcc/
  packages/
    core/                          # Motor de tradução (C#/.NET 8)
      MultiLingualCode.Core/       # Biblioteca principal (C# + Python)
      MultiLingualCode.Core.Host/  # CLI persistente (stdin/stdout JSON)
      MultiLingualCode.Core.Tests/ # Testes unitários e integração
    ide-adapters/
      vscode/                      # Extensão VS Code (TypeScript)
      intellij/                    # Plugin IntelliJ (Kotlin)
  scripts/                         # Scripts de build (JavaScript/Node)
  tarefas/                         # Gestão de tarefas (.pendente/.finalizada)
  examples/                        # Exemplos de uso (C# e Python)
  docs/                            # Documentação técnica e do usuário
```

## Adicionar novas funcionalidades

- **Nova linguagem de programação**: Ver [docs/developer-guide/adding-new-language.md](docs/developer-guide/adding-new-language.md)
- **Novo IDE**: Ver [docs/developer-guide/adding-new-ide.md](docs/developer-guide/adding-new-ide.md)
- **Novas traduções**: Ver [docs/developer-guide/creating-translations.md](docs/developer-guide/creating-translations.md)

## Processo de review

1. PRs devem ter descrição clara do que foi alterado
2. Todos os testes devem passar
3. Novas funcionalidades devem incluir testes
4. Código deve seguir as convenções documentadas

## Licença

Ao contribuir, você concorda que suas contribuições serão licenciadas sob a mesma licença do projeto.
