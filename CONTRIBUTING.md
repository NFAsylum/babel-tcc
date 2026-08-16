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

O projeto usa quatro linguagens, e as regras de cada uma derivam dos mesmos
quatro princípios. Quando uma linguagem nova entrar, derive as regras dela
destes princípios em vez de copiar a lista de outra linguagem — o idioma muda,
a intenção não.

1. **Erro é valor, não fluxo de controle.** A falha é devolvida e tratada
   explicitamente por quem chamou, em vez de subir sozinha pela pilha.
2. **Falha de runtime degrada; falha de build grita.** Em runtime o arquivo do
   usuário é sagrado: se a tradução falha, mostra-se o original intacto. No
   build vale o oposto — abortar alto em vez de empacotar artefato quebrado.
3. **Costuras de teste declaradas no código.** A testabilidade vem de pontos de
   substituição explícitos, não de mocking mágico.
4. **Explícito acima de esperto.** Entre a forma curta e a forma óbvia, o
   projeto escolhe a óbvia.

As regras por linguagem abaixo são a tradução desses princípios para cada
idioma, não uma cópia literal de uma linguagem para outra.

### C# (Core)

- Não usar `var` - tipos explícitos sempre
- Não usar `private` ou `internal` - tudo `public`
- Não usar `throw` - usar `OperationResult` para erros
- Evitar nullable (`?`, `??`) — permitido em boundaries com APIs .NET que retornam null (ex: `Environment.GetEnvironmentVariable`, `Path.GetDirectoryName`, `Version.TryParse`)
- Uma classe por arquivo
- Nomes de testes: `MetodoTestado_Cenario_ResultadoEsperado`

### TypeScript (VS Code Extension)

- Strict mode habilitado
- Usar `const` em vez de `let` quando possível
- Nunca usar `var`
- Tipos explícitos em parâmetros e retornos
- Aspas simples
- Arquivos em camelCase, classes em PascalCase

### Kotlin (Plugin IntelliJ)

O Kotlin **não** copia a lista do C# ao pé da letra: várias daquelas regras
existem para contornar limitações que o Kotlin não tem. O que se preserva é a
intenção de cada uma.

- **Erro não atravessa a borda do serviço.** `CoreBridge` lança
  `CoreBridgeException` internamente, mas todo serviço que o consome captura e
  degrada — `TranslationService` devolve o código original quando o Core falha.
  Um Core quebrado nunca pode corromper o arquivo em disco. É o equivalente do
  `OperationResult` do C# (princípios 1 e 2).
- **Exponha a costura, não a classe inteira.** É o equivalente do "tudo
  `public`" do C#: em vez de abrir todos os membros, declare o ponto exato de
  substituição — a interface `CoreTransport`, `var transportFactory`,
  `var timeoutMs`. `private` é o padrão para o resto (princípio 3)
- **Agrupar declarações relacionadas no mesmo arquivo** quando formam uma
  unidade: `CoreBridge.kt` reúne o serviço, seu transporte, o envelope de
  resposta e a exceção. O C# exige uma classe por arquivo; o Kotlin idiomático
  agrupa, e aqui o idioma da linguagem prevalece
- Tipos inferidos são aceitáveis em locais (`val log = Logger.getInstance(...)`);
  explícitos na assinatura pública de funções
- Um `Logger` por classe, via `Logger.getInstance(Classe::class.java)`. Registre
  falha real em `error`, degradação em `warn`, e nunca descarte a exceção quando
  o overload aceitar o throwable
- Nomes de teste: frase entre crases descrevendo o comportamento observável —
  ``fun `timeout message carries what the Core wrote to stderr`()``

### Python (tokenizer do adapter)

`tokenizer_service.py` roda no interpretador **do usuário**, num ambiente que o
projeto não controla. Isso dita quase todas as regras.

- **Somente biblioteca padrão. Nunca dependência de terceiros.** Ninguém deve
  precisar rodar `pip install` para a extensão funcionar. Hoje o script importa
  apenas `io`, `json`, `keyword`, `sys`, `token` e `tokenize`
- **Compatível com Python 3.8+**, que é o piso verificado por
  `PythonTokenizerService.MinimumPythonVersion`. Nada de sintaxe mais nova
- **Erro é valor, transportado pelo protocolo.** Devolva
  `{"ok": false, "error": "..."}` pelo stdout — é o `OperationResult` do C#
  atravessando a fronteira de processo. Nenhuma exceção pode escapar do laço de
  `main()` (princípio 1)
- **stdout é canal de protocolo; stderr é diagnóstico.** Nunca escreva mensagem
  livre no stdout: ela seria lida como resposta JSON e quebraria o
  enquadramento. O lado C# drena e registra o stderr
- `sys.stdout.flush()` após cada resposta — o processo é persistente e o outro
  lado bloqueia esperando a linha
- Uma responsabilidade por script: tokenizar. Regra de tradução mora no C#

### JavaScript (scripts de build)

Rodam no Node durante o empacotamento, nunca em produção.

- **Somente biblioteca padrão do Node** (`fs`, `path`), sem dependências
- **CommonJS** (`require`), não ESM
- **Falhar alto, sem fallback silencioso.** Ao contrário do runtime, aqui um
  fallback é pior que um erro: empacotar um `.vsix` sem traduções ou sem README
  é publicar produto quebrado. Explique o motivo em `console.error` e encerre
  com `process.exit(1)` (princípio 2)
- `const` por padrão, nunca `var`
- Indentação e encoding seguem o `.editorconfig`

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
