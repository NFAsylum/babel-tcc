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

## Funcionalidades (MVP)

- Traducao visual de keywords C# para PT-BR
- Traducao de identificadores customizados via anotacao `// tradu:`
- Traducao reversa ao salvar (PT-BR -> C# no disco)
- Autocomplete com keywords traduzidas
- Hover com traducao original
- Status bar com idioma ativo
- Syntax highlighting para codigo traduzido

## Arquitetura

| Camada | Tecnologia | Funcao |
|---|---|---|
| Core Engine | C# / .NET 8 | Motor de traducao, parsing via Roslyn |
| Extension | TypeScript / VS Code API | Interface com o editor |
| Traducoes | JSON | Tabelas de keywords e mapeamentos |
| Comunicacao | JSON via stdin/stdout | Bridge entre TS e C# |

Veja a [documentacao de arquitetura](docs/arquitetura.md) para detalhes.

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

### Planejamento Original

- [Plano Geral](docs/plano-geral.txt) - Plano detalhado de implementacao
- [Roadmap](docs/roadmap.txt) - Cronograma e estimativas
- [Estimativas](docs/estimativas.txt) - Estimativas com uso de IA
- [Repositorios](docs/repos.txt) - Estrategia de organizacao

## Tarefas

O progresso do projeto e rastreado na pasta [tarefas/](tarefas/):

| Fase | Descricao | Status |
|---|---|---|
| [Fase 0](tarefas/fase-0-setup.md) | Setup e Onboarding | Pendente |
| [Fase 1](tarefas/fase-1-core-engine.md) | Core Engine (C#) | Pendente |
| [Fase 2](tarefas/fase-2-language-adapters.md) | Language Adapters (Roslyn) | Pendente |
| [Fase 3](tarefas/fase-3-vscode-extension.md) | VS Code Extension (TypeScript) | Pendente |
| [Fase 4](tarefas/fase-4-documentacao.md) | Documentacao e Exemplos | Pendente |
| [Fase 5](tarefas/fase-5-testes-qa.md) | Testes e QA | Pendente |
| [Fase 6](tarefas/fase-6-deploy.md) | Polimento e Deploy | Pendente |

## Stack Tecnologica

- **Core:** C# / .NET 8, Microsoft.CodeAnalysis (Roslyn)
- **Extension:** TypeScript, VS Code Extension API
- **Testes:** xUnit (C#), Mocha (TypeScript)
- **CI/CD:** GitHub Actions
- **Traducoes:** JSON com schema validation

## Contribuicao

Contribuicoes sao bem-vindas! Veja o [guia de traducoes](docs/guia-traducoes.md) para contribuir com novos idiomas - **nao e necessario saber C# ou TypeScript**.

## Licenca

A definir.

---

Projeto de TCC (Trabalho de Conclusao de Curso).
