# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/).
Este projeto segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added
- Suporte a Python como segunda linguagem de programacao (tarefas 052-060)
  - PythonAdapter com tokenizador nativo CPython via subprocesso persistente
  - PythonKeywordMap com 35 hard keywords do Python 3
  - Traducoes Python para 10 idiomas naturais
- TraduAnnotationParser desacoplado do Roslyn (funciona com qualquer adapter)
- AdapterHelpers com CollectReplacements compartilhado (preserva aspas originais)
- Registro central de linguagens na extensao VS Code (src/config/languages.ts)
- Testes de consistencia entre registro TS e package.json
- Deteccao de .NET no activate() com mensagem de erro e link de instalacao
- Keywords contextuais C# (var, async, await, yield, etc.) - tarefa 048
- Processo persistente CoreBridge para melhor performance - tarefa 050
- KeywordMapService dinamico (carrega traducoes por linguagem automaticamente)
- 631 testes (494 C#, 137 TypeScript)

### Fixed
- ApplyTraduAnnotations nao salva mais no disco (evita destruir mapeamentos de outros arquivos)
- refreshingPaths cleanup via try/finally (evita path bloqueado permanentemente)
- Syntax highlighting grammar alinhada com traducoes reais
- Regex tradu annotations matcha formato com seletor [idioma]:
- Traducao "e" (is) corrigida para "igual" em pt-br-ascii (evita corrupcao de dados)
- Mensagens de erro padronizadas para ingles
- Warning visivel quando traducao falha
- CSharpAdapter preserva aspas de verbatim (@"") e interpolated ($"") strings no round-trip

## [0.1.0] - 2026-02-25

### Added
- Motor de traducao C# com parsing via Roslyn (Microsoft.CodeAnalysis)
- Traducao de 89 keywords C# para Portugues Brasileiro (PT-BR)
- Sistema de anotacoes `// tradu[idioma]:NomeTraduzido` para identificadores e parametros
- Mapeamento bidirecional de identificadores (IdentifierMapper)
- Traducao de literais string com anotacoes tradu
- Extensao VS Code com traducao visual em tempo real
- Visualizacao side-by-side (codigo original intacto no disco)
- Autocompletar com nomes traduzidos (CompletionProvider)
- Hover com informacoes de traducao (HoverProvider)
- Syntax highlighting customizado para codigo traduzido
- Barra de status com idioma ativo e toggle
- Seletor de idioma via Command Palette
- TranslatedContentProvider com modo editavel e readonly
- AutoTranslateManager para traducao automatica ao abrir arquivos
- Validacao de sintaxe C# integrada
- Exemplos completos (C# e Python)
- Documentacao completa: arquitetura, guia do usuario, guia do desenvolvedor
- CI/CD com GitHub Actions (build, test, coverage, release)
- Traducao reversa de keywords (pre-substituicao antes do Roslyn)
- Comunicacao VS Code <-> Core via processo persistente (stdin/stdout JSON Lines)

### Dependencies
- .NET 8.0
- Python 3.8+ (para suporte Python)
- Microsoft.CodeAnalysis.CSharp 4.8.0 (MIT License)
- VS Code ^1.85.0
- xUnit 2.x (MIT License) - testes
- NSubstitute 5.x (BSD License) - testes
- Vitest 4.x (MIT License) - testes TypeScript
