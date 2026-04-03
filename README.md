# Babel TCC - MultiLingual Code

[![CI](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml/badge.svg)](https://github.com/NFAsylum/babel-tcc/actions/workflows/ci.yml)

Extensao VS Code que traduz codigo de programacao visualmente em tempo real, mantendo os arquivos originais intactos no disco.

## O que faz?

Desenvolvedores escrevem codigo em C# ou Python, e a extensao exibe as keywords e identificadores traduzidos para o idioma configurado (PT-BR, ES-ES, etc.). Ao salvar, o codigo volta automaticamente para a linguagem de programacao original.

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

O arquivo no disco permanece **sempre no codigo original**. A traducao e apenas visual.

## Features

- **Traducao visual de keywords** - Keywords C# e Python traduzidas (if->se, class->classe, def->definir, etc.)
- **Traducao de identificadores** - Nomes de variaveis, metodos e classes via anotacao `// tradu:`
- **Traducao reversa ao salvar** - Ao salvar, o codigo traduzido volta para o original no disco
- **Autocomplete traduzido** - Sugestoes de keywords e identificadores no idioma configurado
- **Hover com original** - Passar o mouse sobre keyword traduzida mostra a keyword original
- **Status bar** - Indicador do idioma ativo com seletor rapido
- **Syntax highlighting** - Gramatica TextMate customizada para keywords traduzidas
- **Colaboracao multilingue** - Multiplos devs no mesmo repo, cada um ve seu idioma
- **Zero impacto** - Compiladores, CI/CD, Git e IntelliSense funcionam normalmente
- **Processo persistente** - Motor de traducao roda como processo longo, sem cold start por request

## Quick Start

1. Instalar a extensao no VS Code
2. Abrir um arquivo `.cs` ou `.py`
3. Pressionar `Ctrl+Shift+P` e executar `Babel TCC: Select Language`
4. Escolher `pt-br`
5. A traducao aparece automaticamente

## Instalacao

### Pre-requisitos

- VS Code 1.85 ou superior
- .NET 8.0 Runtime
- Python 3.8+ (para suporte a arquivos Python)

### A partir do codigo-fonte

```bash
git clone https://github.com/NFAsylum/babel-tcc.git
cd babel-tcc/packages/ide-adapters/vscode
npm install
npm run build
```

Para gerar o `.vsix`: `npm run package` (requer [vsce](https://github.com/microsoft/vscode-vsce))

## Linguagens Suportadas

| Linguagem de Programacao | Status |
|--------------------------|--------|
| C# (89 keywords) | Suportado |
| Python (35 keywords) | Suportado |

## Idiomas Disponiveis

Portugues (PT-BR), Portugues ASCII, Ingles, Espanhol, Frances, Alemao, Italiano, Japones (Romaji), Chines, Arabe.

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

| Camada | Tecnologia | Funcao |
|--------|-----------|--------|
| Core Engine | C# / .NET 8 | Motor de traducao, parsing via Roslyn e tokenizer Python |
| Extension | TypeScript / VS Code API | Interface com o editor |
| Traducoes | JSON | Tabelas de keywords e mapeamentos |
| Comunicacao | JSON Lines via stdin/stdout | Bridge persistente entre TS e C# |

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
- **Testes:** xUnit (C#) + Vitest (TypeScript), 500+ testes
- **CI/CD:** GitHub Actions (matrix Ubuntu + Windows)
- **Traducoes:** JSON

## Estrutura do Projeto

```
babel-tcc/
  packages/
    core/
      MultiLingualCode.Core/        # Motor de traducao
      MultiLingualCode.Core.Host/   # Host persistente (stdin/stdout)
      MultiLingualCode.Core.Tests/  # Testes xUnit
    ide-adapters/
      vscode/                       # Extensao VS Code
        src/
          extension.ts              # Entry point
          services/                 # CoreBridge, Config, LanguageDetector
          providers/                # Content, Edit, Save, Completion, Hover
          ui/                       # StatusBar
        test/                       # Testes Vitest
        syntaxes/                   # Gramaticas TextMate
  scripts/                          # Validacao de traducoes
  tarefas/                          # Rastreamento de tarefas
```

## Documentacao

- [Arquitetura](docs/developer-guide/architecture.md) - Visao geral da arquitetura e fluxos
- [Convencoes de Codigo](CONTRIBUTING.md#convencoes-de-codigo) - Nomenclatura e estilo
- [Decisoes Tecnicas](docs/decisoes-tecnicas.md) - Registro de decisoes e justificativas
- [Guia do Usuario](docs/user-guide/) - Instalacao, uso e configuracao
- [Guia do Desenvolvedor](docs/developer-guide/) - Como estender o projeto

## Contribuicao

Contribuicoes sao bem-vindas! Veja [CONTRIBUTING.md](CONTRIBUTING.md) para detalhes.

## Licenca

[MIT](LICENSE)
