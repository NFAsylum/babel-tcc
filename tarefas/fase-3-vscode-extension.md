# Fase 3 - VS Code Extension

**Objetivo:** Criar extensao funcional para VS Code que usa o Core Engine.
**Pre-requisito:** Fase 2 concluida (Core + CSharpAdapter funcionais).

---

## Tarefa 3.1 - Setup do projeto TypeScript

**Escopo:**
- [ ] Estrutura final em `packages/ide-adapters/vscode/`
- [ ] Configurar `package.json` com:
  - contributes.commands (toggle, selectLanguage, openTranslated, showOriginal)
  - contributes.configuration (multilingual.enabled, multilingual.language)
  - activationEvents (onLanguage:csharp)
- [ ] Configurar `tsconfig.json` (strict, ES2020, CommonJS)
- [ ] Configurar build com esbuild ou webpack
- [ ] Criar `src/extension.ts` com activate/deactivate basicos
- [ ] Testar: extensao ativa e loga no Output Channel

**Entrega:** Extensao esqueleto instala e ativa no VS Code.

---

## Tarefa 3.2 - CoreBridge: Comunicacao TS <-> C#

**Escopo:**
- [ ] Implementar `CoreBridge` em `src/services/CoreBridge.ts`
- [ ] Protocolo de comunicacao:
  - Spawn processo .NET (`dotnet MultiLingualCode.Core.dll`)
  - Enviar requests via JSON em stdin
  - Receber responses via JSON em stdout
  - Formato: `{ "method": "...", "params": {...} }` -> `{ "result": ..., "error": ... }`
- [ ] Metodos:
  - translateToNaturalLanguage(sourceCode, fileExtension, targetLanguage)
  - translateFromNaturalLanguage(translatedCode, fileExtension, sourceLanguage)
  - validateSyntax(sourceCode, fileExtension)
  - getCompletions(sourceCode, position, fileExtension, language)
- [ ] Gerenciamento de ciclo de vida do processo (start, restart, dispose)
- [ ] Timeout e tratamento de erros
- [ ] Testes unitarios com mock do processo

**Dependencia:** Tarefa 3.1

**Entrega:** CoreBridge comunica com processo .NET e retorna traducoes.

---

## Tarefa 3.3 - TranslatedContentProvider

**Escopo:**
- [ ] Implementar `TranslatedContentProvider` em `src/providers/`
  - Implementar `TextDocumentContentProvider` para esquema `multilingual://`
  - Ler arquivo original do disco
  - Chamar CoreBridge.translateToNaturalLanguage()
  - Retornar codigo traduzido
- [ ] Cache de traducoes (invalidar ao editar)
- [ ] Evento onDidChange para refresh
- [ ] Fallback: se traducao falha, mostrar original

**Dependencia:** Tarefa 3.2

**Entrega:** Abrir arquivo traduzido via URI `multilingual://` funciona.

---

## Tarefa 3.4 - LanguageDetector e ConfigurationService

**Escopo:**
- [ ] Implementar `LanguageDetector` em `src/services/`
  - Detectar linguagem por extensao de arquivo
  - Verificar se linguagem e suportada
- [ ] Implementar `ConfigurationService` em `src/services/`
  - Ler/escrever configuracoes: idioma ativo, extensao habilitada
  - Reagir a mudancas de configuracao (onDidChangeConfiguration)
- [ ] Testes

**Entrega:** Deteccao de linguagem e configuracoes funcionais.

---

## Tarefa 3.5 - EditInterceptor

**Escopo:**
- [ ] Implementar `EditInterceptor` em `src/providers/`
  - Capturar `onDidChangeTextDocument` para documentos multilingual
  - Detectar mudancas relevantes (edicoes de texto vs formatacao)
  - Atualizar traducao incrementalmente (nao retraduzir tudo)
  - Sincronizar mudancas com documento original
- [ ] Testes de edicao

**Dependencia:** Tarefa 3.3

**Entrega:** Edicoes no documento traduzido sao detectadas e processadas.

---

## Tarefa 3.6 - SaveHandler

**Escopo:**
- [ ] Implementar `SaveHandler` em `src/providers/`
  - Capturar `onWillSaveTextDocument` para documentos multilingual
  - Chamar CoreBridge.translateFromNaturalLanguage() com conteudo editado
  - Salvar codigo original (C#) no arquivo real (.cs)
  - Feedback visual: "Arquivo salvo com sucesso"
  - Tratamento de erros: se traducao reversa falha, nao sobrescrever original
- [ ] Testes de save

**Dependencia:** Tarefa 3.3

**Entrega:** Salvar documento traduzido grava C# correto no disco.

---

## Tarefa 3.7 - Commands e UI basica

**Escopo:**
- [ ] Implementar commands:
  - `multilingual.toggleTranslation` - ativar/desativar
  - `multilingual.selectLanguage` - QuickPick para escolher idioma
  - `multilingual.openTranslated` - abrir versao traduzida do arquivo ativo
  - `multilingual.showOriginal` - mostrar versao original
- [ ] Implementar `StatusBar` em `src/ui/`
  - Mostrar idioma ativo na barra de status
  - Clicar abre seletor de idioma
  - Indicador visual quando traducao esta ativa
- [ ] Implementar `LanguageSelector` em `src/ui/`
  - QuickPick com idiomas disponiveis
  - Persistir selecao na configuracao

**Entrega:** Comandos funcionais e UI basica na status bar.

---

## Tarefa 3.8 - CompletionProvider e HoverProvider

**Escopo:**
- [ ] Implementar `CompletionProvider` em `src/providers/`
  - Autocomplete de keywords traduzidas
  - Sugestoes de identificadores comuns traduzidos
  - Integracao com CoreBridge para sugestoes contextuais
- [ ] Implementar `HoverProvider` em `src/providers/`
  - Ao passar mouse sobre keyword traduzida: mostrar keyword original
  - Ao passar sobre identificador: mostrar nome original + tipo
- [ ] Testes

**Dependencia:** Tarefa 3.2

**Entrega:** Autocomplete e hover funcionam com conteudo traduzido.

---

## Tarefa 3.9 - Syntax Highlighting

**Escopo:**
- [ ] Criar `mlc-csharp.tmLanguage.json` em `syntaxes/`
  - Definir gramatica TextMate para C# traduzido
  - Keywords PT-BR com cores corretas (se, senao, classe, etc.)
  - Identificadores, literais, comentarios
  - Destacar anotacoes "tradu" em cor especial
- [ ] Registrar gramatica no package.json
- [ ] Testes visuais

**Entrega:** Syntax highlighting funciona para codigo traduzido.

---

## Tarefa 3.10 - Package e testes end-to-end

**Escopo:**
- [ ] Configurar bundling (esbuild/webpack) para gerar .vsix
  - Incluir binarios do Core C# no pacote
  - Incluir tabelas de traducao
- [ ] Gerar .vsix e testar instalacao local
- [ ] Suite de testes end-to-end:
  - Abrir arquivo C# -> ver traducao PT-BR
  - Editar codigo traduzido -> salvar -> verificar .cs original
  - Trocar idioma -> verificar nova traducao
  - Toggle on/off
- [ ] Correcao de bugs encontrados

**Dependencia:** Todas as tarefas anteriores da Fase 3

**Entrega:** Extensao .vsix funcional e instalavel.

---

## Dependencias

```
3.1 ──> 3.2 ──> 3.3 ──> 3.5
                    ──> 3.6
         3.2 ──> 3.8
3.4 (paralela com 3.2)
3.7 (paralela com 3.3+)
3.9 (paralela)
3.5 + 3.6 + 3.7 + 3.8 + 3.9 ──> 3.10
```
