# Tarefa 059 - Atualizar extensao VS Code para suportar arquivos Python

## Fase
7 - Suporte a Python

## Objetivo
Atualizar a extensao VS Code para detectar, monitorar e processar arquivos Python (.py) alem de C# (.cs).

## Escopo

### 1. package.json — activationEvents, languages e grammars
O `package.json` da extensao tem varios hardcodes para C# que precisam ser expandidos:

**activationEvents** (linha 15-18):
```json
// Atual:
"activationEvents": ["onLanguage:csharp"]

// Novo:
"activationEvents": ["onLanguage:csharp", "onLanguage:python"]
```

**languages** (linha 76-82) — registrar linguagem traduzida para Python:
```json
// Atual:
"languages": [{ "id": "mlc-csharp", "aliases": ["C# Traduzido", "mlc-csharp"] }]

// Novo:
"languages": [
  { "id": "mlc-csharp", "aliases": ["C# Traduzido", "mlc-csharp"] },
  { "id": "mlc-python", "aliases": ["Python Traduzido", "mlc-python"] }
]
```

**grammars** (linha 69-75) — registrar gramatica TextMate para Python traduzido:
```json
// Atual:
"grammars": [{ "language": "mlc-csharp", "scopeName": "source.mlc-csharp", "path": "./syntaxes/mlc-csharp.tmLanguage.json" }]

// Novo:
"grammars": [
  { "language": "mlc-csharp", "scopeName": "source.mlc-csharp", "path": "./syntaxes/mlc-csharp.tmLanguage.json" },
  { "language": "mlc-python", "scopeName": "source.mlc-python", "path": "./syntaxes/mlc-python.tmLanguage.json" }
]
```

### 2. Criar syntaxes/mlc-python.tmLanguage.json
Criar gramatica TextMate para Python traduzido, baseada no `mlc-csharp.tmLanguage.json`.
Deve fornecer syntax highlighting basico para arquivos Python traduzidos (keywords traduzidas, strings, comentarios `#`, numeros, etc.).

### 3. languageDetector.ts
Adicionar Python ao dicionario de extensoes suportadas:
```typescript
// Atual:
const SUPPORTED_EXTENSIONS: Record<string, string> = { '.cs': 'CSharp' };

// Novo:
const SUPPORTED_EXTENSIONS: Record<string, string> = { '.cs': 'CSharp', '.py': 'Python' };
```

### 4. extension.ts (~linha 101)
File watcher hardcoded para `.cs`:
```typescript
// Atual:
const fileWatcher = vscode.workspace.createFileSystemWatcher('**/*.cs');

// Novo (opcao A — glob expandido):
const fileWatcher = vscode.workspace.createFileSystemWatcher('**/*.{cs,py}');

// Novo (opcao B — dinamico, baseado em languageDetector):
const extensions = languageDetector.getSupportedExtensions();
```

Decidir entre opcao A (simples) e opcao B (mais extensivel para futuras linguagens).

### 5. hoverProvider.ts
Language hint hardcoded no hover tooltip:
```typescript
// Atual:
markdown.appendCodeblock(`${originalKeyword}`, 'csharp');

// Novo:
const vscodeLangId = getVSCodeLanguageId(detectedLanguage);
markdown.appendCodeblock(`${originalKeyword}`, vscodeLangId);
```

Mapeamento: `'CSharp'` -> `'csharp'`, `'Python'` -> `'python'`

### 6. Verificar autoTranslateManager.ts
Pesquisa anterior encontrou referencias a `.cs` em comentarios. Verificar se ha logica hardcoded que precisa ser atualizada.

### 7. Verificar coreBridge.ts
Verificar se o coreBridge passa a extensao do arquivo ao invocar o Core .NET. O Core precisa da extensao para resolver o adapter correto via LanguageRegistry.
