# Multilingual Code - babel-tcc

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
3. Pressionar `Ctrl+Shift+P` e executar `MultiLingual: Select Language`
4. Escolher `Portugues Brasileiro (PT-BR)`
5. Executar `MultiLingual: Open Translated` para ver o codigo traduzido

## Especificacoes Tecnicas

### Arquitetura

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

O sistema e composto por camadas desacopladas:

| Camada | Tecnologia | Funcao |
|---|---|---|
| Core Engine | C# / .NET 8 | Motor de traducao, parsing via Roslyn |
| Extension | TypeScript / VS Code API | Interface com o editor |
| Traducoes | JSON | Tabelas de keywords e mapeamentos |
| Comunicacao | JSON via stdin/stdout | Bridge entre TS e C# |

### Stack

- **Core:** C# / .NET 8, Microsoft.CodeAnalysis (Roslyn)
- **Extension:** TypeScript, VS Code Extension API
- **Testes:** xUnit (C#), Mocha (TypeScript)
- **CI/CD:** GitHub Actions
- **Traducoes:** JSON com schema validation

### Escopo do MVP

- **Linguagem de programacao:** C# (via Roslyn)
- **Idioma alvo:** Portugues Brasileiro (PT-BR)
- **IDE:** VS Code
- **Keywords:** Todas as 78 keywords C#
- **Identificadores:** Via anotacao `// tradu:` e identifier-map.json

### Performance

- Arquivo pequeno (< 100 linhas): < 100ms
- Arquivo medio (100-500 linhas): < 500ms
- Arquivo grande (500-2000 linhas): < 2s

### Sistema "tradu"

Desenvolvedores anotam identificadores customizados no codigo:

```csharp
public class Calculator
{
    private int operationCount = 0;  // tradu:contagemOperacoes

    public int Add(int a, int b)  // tradu:Somar,a:primeiroNumero,b:segundoNumero
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
    privado inteiro contagemOperacoes = 0;

    publico inteiro Somar(inteiro primeiroNumero, inteiro segundoNumero)
    {
        contagemOperacoes++;
        retornar primeiroNumero + segundoNumero;
    }
}
```

## Repositorios

| Repositorio | Descricao |
|---|---|
| [babel-tcc](https://github.com/NFAsylum/babel-tcc) | Core Engine + VS Code Extension + Docs |
| [babel-tcc-translations](https://github.com/NFAsylum/babel-tcc-translations) | Tabelas de keywords e traducoes por idioma |

## Documentacao

- [Arquitetura](docs/arquitetura.md) - Visao geral da arquitetura e fluxos
- [Padroes de Codigo](docs/padroes-codigo.md) - Convencoes de nomenclatura e estilo
- [Setup do Ambiente](docs/setup-ambiente.md) - Como configurar o ambiente de desenvolvimento
- [Guia de Traducoes](docs/guia-traducoes.md) - Como contribuir com novas traducoes
- [Decisoes Tecnicas](docs/decisoes-tecnicas.md) - Registro de decisoes e justificativas

## Contribuicao

Contribuicoes sao bem-vindas! Veja o [guia de traducoes](docs/guia-traducoes.md) para contribuir com novos idiomas - **nao e necessario saber C# ou TypeScript**.

## Licenca

A definir.

---

Projeto de TCC (Trabalho de Conclusao de Curso).
