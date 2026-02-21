# Babel TCC - MultiLingual Code

[![Build Core](https://github.com/NFAsylum/babel-tcc/actions/workflows/build-core.yml/badge.svg)](https://github.com/NFAsylum/babel-tcc/actions/workflows/build-core.yml)
[![Build VS Code](https://github.com/NFAsylum/babel-tcc/actions/workflows/build-vscode.yml/badge.svg)](https://github.com/NFAsylum/babel-tcc/actions/workflows/build-vscode.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Extensao VS Code que traduz codigo de programacao visualmente em tempo real, mantendo os arquivos originais intactos no disco.

## O que faz?

Desenvolvedores escrevem codigo em C# (e futuramente Python, JavaScript, etc.), e a extensao exibe as keywords e identificadores traduzidos para o idioma configurado (PT-BR, ES-ES, etc.). Ao salvar, o codigo volta automaticamente para a linguagem de programacao original.

**Antes (C# original no disco):**
```csharp
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

**Depois (o que o dev PT-BR ve no editor):**
```csharp
usando System;

espaconome OlaMundo
{
    classe Programa
    {
        estatico vazio Principal(texto[] argumentos)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

O arquivo no disco permanece **sempre em C# puro**. A traducao e apenas visual.

## Features

- **Traducao visual de keywords** - Todas as 78 keywords C# traduzidas (if->se, class->classe, void->vazio, etc.)
- **Traducao de identificadores** - Nomes de variaveis, metodos e classes via anotacao `// tradu:`
- **Traducao reversa ao salvar** - Ao salvar, o codigo traduzido volta para C# original no disco
- **Autocomplete traduzido** - Sugestoes de keywords e identificadores no idioma configurado
- **Hover com original** - Passar o mouse sobre keyword traduzida mostra a keyword C# original
- **Status bar** - Indicador do idioma ativo com seletor rapido
- **Syntax highlighting** - Gramatica TextMate customizada para keywords traduzidas
- **Colaboracao multilingue** - Multiplos devs no mesmo repo, cada um ve seu idioma
- **Zero impacto** - Compiladores, CI/CD, Git e IntelliSense funcionam normalmente

## Quick Start

1. Instalar a extensao no VS Code (Marketplace ou .vsix local)
2. Abrir um arquivo `.cs`
3. Pressionar `Ctrl+Shift+P` e executar `Babel TCC: Select Language`
4. Escolher `pt-br`
5. Executar `Babel TCC: Open Translated View` para ver o codigo traduzido

## Instalacao

### Pre-requisitos

- VS Code 1.85 ou superior
- .NET 8.0 Runtime

### A partir do codigo-fonte

```bash
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode
npm install
npm run package
```

Instalar o `.vsix` gerado: Extensions > ... > Install from VSIX.

## Linguagens Suportadas

| Linguagem de Programacao | Idioma Natural | Status |
|--------------------------|---------------|--------|
| C# | Portugues (PT-BR) | Suportado |
| Python | - | Planejado |
| JavaScript | - | Planejado |

## Arquitetura

```
VS Code Extension (TypeScript)
        |
    CoreBridge (JSON via stdin/stdout)
        |
Core Engine (C# / .NET 8)
        |
    CSharpAdapter (Roslyn)
        |
Translation Tables (JSON)
```

| Camada | Tecnologia | Funcao |
|--------|-----------|--------|
| Core Engine | C# / .NET 8 | Motor de traducao, parsing via Roslyn |
| Extension | TypeScript / VS Code API | Interface com o editor |
| Traducoes | JSON | Tabelas de keywords e mapeamentos |
| Comunicacao | JSON via stdin/stdout | Bridge entre TS e C# |

## Configuracao

Adicionar ao `settings.json`:

```json
{
  "babel-tcc.enabled": true,
  "babel-tcc.language": "pt-br"
}
```

### Sistema "tradu"

Desenvolvedores anotam identificadores customizados no codigo:

```csharp
public class Calculator // tradu:Calculadora
{
    public int operationCount; // tradu:contagemOperacoes

    public int Add(int a, int b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }
}
```

O dev PT-BR ve:

```csharp
publico classe Calculadora
{
    publico inteiro contagemOperacoes;

    publico inteiro Somar(inteiro primeiroNumero, inteiro segundoNumero)
    {
        contagemOperacoes++;
        retornar primeiroNumero + segundoNumero;
    }
}
```

## Stack

- **Core:** C# / .NET 8, Microsoft.CodeAnalysis (Roslyn)
- **Extension:** TypeScript, VS Code Extension API
- **Testes:** xUnit (C#), 251+ testes
- **CI/CD:** GitHub Actions
- **Traducoes:** JSON com schema validation

## Estrutura do Projeto

```
babel-tcc/
  packages/
    core/
      MultiLingualCode.Core/        # Motor de traducao
      MultiLingualCode.Core.Host/   # CLI para comunicacao TS<->C#
      MultiLingualCode.Core.Tests/  # Testes xUnit (251+)
    ide-adapters/
      vscode/                       # Extensao VS Code
        src/
          extension.ts              # Entry point
          services/                 # CoreBridge, Config, LanguageDetector
          providers/                # Content, Edit, Save, Completion, Hover
          ui/                       # StatusBar
        syntaxes/                   # Gramaticas TextMate
  docs/                             # Documentacao
  examples/                         # Projetos de exemplo
  tarefas/                          # Rastreamento de tarefas
```

## Documentacao

- [Arquitetura](docs/arquitetura.md) - Visao geral da arquitetura e fluxos
- [Padroes de Codigo](docs/padroes-codigo.md) - Convencoes de nomenclatura e estilo
- [Decisoes Tecnicas](docs/decisoes-tecnicas.md) - Registro de decisoes e justificativas
- [Guia do Usuario](docs/user-guide/) - Instalacao, uso e configuracao
- [Guia do Desenvolvedor](docs/developer-guide/) - Como estender o projeto
- [Resultado da PoC](docs/poc-resultado.md) - Validacao de viabilidade tecnica

## Contribuicao

Contribuicoes sao bem-vindas! Veja [CONTRIBUTING.md](CONTRIBUTING.md) para detalhes.

## Licenca

MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

Projeto de TCC (Trabalho de Conclusao de Curso).
