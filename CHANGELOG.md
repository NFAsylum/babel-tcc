# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).
Este projeto segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added
- Keywords contextuais C# (var, async, await, yield, etc.) - tarefa 048
- Processo persistente CoreBridge para melhor performance - tarefa 050

## [0.1.0] - 2026-02-25

### Added
- Motor de traducao C# com parsing via Roslyn (Microsoft.CodeAnalysis)
- Traducao de 77 keywords C# reservadas para Portugues Brasileiro (PT-BR)
- Sistema de anotacoes `// tradu:NomeTraduzido` para identificadores e parametros
- Mapeamento bidirecional de identificadores (IdentifierMapper)
- Traducao de literais string com anotacoes `// tradu-literal:"original":"traduzido"`
- Extensao VS Code com traducao visual em tempo real
- Visualizacao side-by-side (codigo original intacto no disco)
- Autocompletar com nomes traduzidos (CompletionProvider)
- Hover com informacoes de traducao (HoverProvider)
- Syntax highlighting customizado para codigo traduzido
- Barra de status com idioma ativo e toggle
- Seletor de idioma via Command Palette
- Interceptacao de edicoes no arquivo traduzido (EditInterceptor)
- Salvamento reverso automatico (SaveHandler)
- Validacao de sintaxe C# integrada
- 3 exemplos completos (HelloWorld, Calculator, TodoApp)
- Documentacao completa: arquitetura, guia do usuario, guia do desenvolvedor
- 338 testes (unitarios, integracao, performance, seguranca)
- CI/CD com GitHub Actions (build, test)
- Traducao reversa de keywords (pre-substituicao antes do Roslyn)
- Propagacao de erros em LoadTranslationTableAsync
- Comunicacao VS Code <-> Core via CLI JSON (stdin/stdout)

### Fixed
- Traducao reversa de keywords: Roslyn nao reconhecia keywords traduzidas (usando, classe) como tokens C# - resolvido com pre-substituicao textual antes do parse
- Remocao de fallbacks silenciosos de idioma no Host (erro claro quando targetLanguage/sourceLanguage ausentes)
- Cast para tipo concreto NaturalLanguageProvider removido (novo metodo GetOriginalKeyword na interface)

### Dependencies
- .NET 8.0
- Microsoft.CodeAnalysis.CSharp 4.8.0 (MIT License)
- VS Code ^1.85.0
- xUnit 2.x (MIT License) - testes
- NSubstitute 5.x (BSD License) - testes
