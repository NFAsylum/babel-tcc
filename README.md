# Babel TCC - MultiLingual Code

[![CI](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml/badge.svg)](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml)

**Português** | [English](README.en.md) | [Español](README.es.md)

Extensão VS Code que traduz código de programação visualmente em tempo real, mantendo os arquivos originais intactos no disco.

![Demonstração: o mesmo código C# alternando entre idiomas em tempo real](images/babel-tcc-demo.gif)

## O que faz?

Desenvolvedores escrevem código em C# ou Python, e a extensão exibe as keywords e identificadores traduzidos para o idioma configurado (PT-BR, ES-ES, etc.). Ao salvar, o código volta automaticamente para a linguagem de programação original.

**Antes (C# original no disco, com anotações `// tradu` nos identificadores):**
```csharp
using System;

namespace HelloWorld // tradu[pt-br]:OlaMundo
{
    class Program // tradu[pt-br]:Programa
    {
        static void Main(string[] args) // tradu[pt-br]:Principal,args:argumentos
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

**Depois (o que o dev PT-BR vê no editor; os comentários `// tradu` continuam visíveis):**
```csharp
usando System;

espaçonome OlaMundo // tradu[pt-br]:OlaMundo
{
    classe Programa // tradu[pt-br]:Programa
    {
        estático vazio Principal(texto[] argumentos) // tradu[pt-br]:Principal,args:argumentos
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
```

O arquivo no disco permanece **sempre no código original**. A tradução é apenas visual.

## Features

- **Tradução visual de keywords** - Keywords C# e Python traduzidas (if->se, class->classe, def->definir, etc.)
- **Tradução de identificadores** - Nomes de variáveis, métodos e classes via anotação `// tradu[lang]:`
- **Tradução reversa ao salvar** - Ao salvar, o código traduzido volta para o original no disco
- **Autocomplete traduzido** - Sugestões de keywords e identificadores no idioma configurado
- **Hover com original** - Passar o mouse sobre keyword traduzida mostra a keyword original
- **Status bar** - Indicador do idioma ativo com seletor rápido
- **Syntax highlighting** - Gramática TextMate customizada para keywords traduzidas
- **Colaboração multilíngue** - Múltiplos devs no mesmo repo, cada um vê seu idioma
- **Zero impacto** - Compiladores, CI/CD, Git e IntelliSense funcionam normalmente
- **Processo persistente** - Motor de tradução roda como processo longo, sem cold start por request

## Quick Start

1. Instalar a extensão no VS Code
2. Abrir um arquivo `.cs` ou `.py`
3. Pressionar `Ctrl+Shift+P` e executar `Babel TCC: Select Language`
4. Escolher `pt-br`
5. A tradução aparece automaticamente

## Instalação

### Pré-requisitos

- VS Code 1.85 ou superior
- **.NET 8.0 Runtime** — necessário **apenas** no pacote universal. Os pacotes por plataforma
  (Windows/Linux/macOS) já embutem o runtime e **não exigem .NET instalado** (ver
  [Pacotes de distribuição](#pacotes-de-distribuição)).
- Python 3.8+ — **opcional**, necessário somente para traduzir arquivos `.py`
- Nada adicional para VisuAlg / Portugol Studio / JavaScript (operam pelo Text Scan, sem parser externo)

### Pacotes de distribuição

A extensão é publicada em duas variantes (decisão DT-010); o VS Code, o Marketplace e o
Open VSX escolhem automaticamente o pacote certo para o seu sistema:

| Variante | Quem recebe | Exige .NET? | Tamanho |
|----------|-------------|-------------|---------|
| Por plataforma (self-contained) | Windows, Linux e macOS (x64 e arm64) | Não — runtime embutido | maior (~36 MB) |
| Universal (fallback) | Qualquer outra plataforma | Sim — .NET 8.0 Runtime | menor (~5 MB) |

> O pacote por plataforma dispensa o **.NET**, mas ainda usa a biblioteca **ICU** do sistema (presente
> por padrão no Windows 10+, macOS e na maioria das distros Linux; só ambientes minimalistas precisam
> instalá-la, ex.: `libicu`).

### A partir do código-fonte

```bash
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode
npm install
npm run build
```

Para gerar o `.vsix`: `npm run package` (requer [vsce](https://github.com/microsoft/vscode-vsce))

## Linguagens Suportadas

| Linguagem de Programação | Extensão | Keywords | Modo |
|--------------------------|----------|----------|------|
| C# | `.cs` | 91 | Roslyn + Text Scan, suporta tradu |
| Python | `.py` | 35 | CPython subprocess + Text Scan, suporta tradu |
| VisuAlg (Claudio Morgado) | `.alg` | 48 | Text Scan keyword-only, case-insensitive |
| Portugol Studio (UNIVALI) | `.por` | 26 | Text Scan keyword-only, case-sensitive |
| JavaScript | `.js` | 38 | Text Scan keyword-only, case-sensitive |

## Idiomas Disponíveis

Português (PT-BR), Português ASCII, Inglês, Espanhol, Francês, Alemão, Italiano, Japonês (Romaji), Chinês, Árabe.

O mesmo `Calculator.cs` exibido em quatro idiomas — os arquivos no disco continuam no código original:

| Português (PT-BR) | Chinês (zh-cn) |
|:---:|:---:|
| ![Calculator.cs em português](images/babel-tcc-pt-br.png) | ![Calculator.cs em chinês](images/babel-tcc-zh-cn.png) |
| **Alemão (de-de)** | **Japonês — Romaji (ja-jp-romaji)** |
| ![Calculator.cs em alemão](images/babel-tcc-de-de.png) | ![Calculator.cs em japonês romanizado](images/babel-tcc-ja-jp-romaji.png) |

## Arquitetura

```
VS Code Extension (TypeScript)
        |
    CoreBridge (JSON Lines via stdin/stdout)
        |
Core Engine (C# / .NET 8)
    |           |
CSharpAdapter   PythonAdapter
  (Roslyn)     (tokenize stdlib)
        |
Translation Tables (JSON)
```

| Camada | Tecnologia | Função |
|--------|-----------|--------|
| Core Engine | C# / .NET 8 | Motor de tradução, parsing via Roslyn e tokenizer Python |
| Extension | TypeScript / VS Code API | Interface com o editor |
| Traduções | JSON | Tabelas de keywords e mapeamentos |
| Comunicação | JSON Lines via stdin/stdout | Bridge persistente entre TS e C# |

## Configuração

Adicionar ao `settings.json`:

```json
{
  "babel-tcc.enabled": true,
  "babel-tcc.language": "pt-br"
}
```

### Sistema "tradu"

Desenvolvedores anotam identificadores customizados no código:

```csharp
public class Calculator // tradu[pt-br]:Calculadora
{
    public int operationCount; // tradu[pt-br]:contagemOperacoes

    public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        operationCount++;
        return a + b;
    }
}
```

O dev PT-BR vê (as anotações `// tradu` continuam visíveis):

```csharp
público classe Calculadora // tradu[pt-br]:Calculadora
{
    público inteiro contagemOperacoes; // tradu[pt-br]:contagemOperacoes

    público inteiro Somar(inteiro primeiroNumero, inteiro segundoNumero) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
    {
        contagemOperacoes++;
        retornar primeiroNumero + segundoNumero;
    }
}
```

## Stack

- **Core:** C# / .NET 8, Microsoft.CodeAnalysis (Roslyn)
- **Extension:** TypeScript, VS Code Extension API
- **Testes:** xUnit (C#) + Vitest (TypeScript), 849 testes (667 C#, 182 TS)
- **CI/CD:** GitHub Actions (matrix Ubuntu + Windows)
- **Traduções:** JSON

## Estrutura do Projeto

```
babel-tcc/
  packages/
    core/
      MultiLingualCode.Core/        # Motor de tradução
      MultiLingualCode.Core.Host/   # Host persistente (stdin/stdout)
      MultiLingualCode.Core.Tests/  # Testes xUnit
    ide-adapters/
      vscode/                       # Extensão VS Code
        src/
          extension.ts              # Entry point
          services/                 # CoreBridge, Config, LanguageDetector
          providers/                # Content, Edit, Save, Completion, Hover
          ui/                       # StatusBar
        test/                       # Testes Vitest
        syntaxes/                   # Gramáticas TextMate
  scripts/                          # Validação de traduções
  tarefas/                          # Rastreamento de tarefas
```

## Documentação

- [Arquitetura](docs/developer-guide/architecture.md) - Visão geral da arquitetura e fluxos
- [Convenções de Código](CONTRIBUTING.md#convenções-de-código) - Nomenclatura e estilo
- [Decisões Técnicas](docs/decisoes-tecnicas.md) - Registro de decisões e justificativas
- [Guia do Usuário](docs/user-guide/) - Instalação, uso e configuração
- [Guia do Desenvolvedor](docs/developer-guide/) - Como estender o projeto

## Contribuição

Contribuições são bem-vindas! Veja [CONTRIBUTING.md](CONTRIBUTING.md) para detalhes.

## Licença

[MIT](LICENSE)
