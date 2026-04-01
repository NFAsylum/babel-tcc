# Tarefa 059 - Atualizar extensao VS Code para suportar arquivos Python

## Fase
7 - Suporte a Python

## Objetivo
Atualizar a extensao VS Code para detectar, monitorar e processar arquivos Python (.py) alem de C# (.cs).

## Escopo

### 1. languageDetector.ts
Adicionar Python ao dicionario de extensoes suportadas:
```typescript
// Atual:
const SUPPORTED_EXTENSIONS: Record<string, string> = {
  '.cs': 'CSharp'
};

// Novo:
const SUPPORTED_EXTENSIONS: Record<string, string> = {
  '.cs': 'CSharp',
  '.py': 'Python'
};
```

### 2. extension.ts (~linha 101)
File watcher hardcoded para `.cs`:
```typescript
// Atual:
const fileWatcher = vscode.workspace.createFileSystemWatcher('**/*.cs');

// Novo (opcao A — glob expandido):
const fileWatcher = vscode.workspace.createFileSystemWatcher('**/*.{cs,py}');

// Novo (opcao B — dinamico, baseado em languageDetector):
const extensions = languageDetector.getSupportedExtensions();
// Criar watchers para cada extensao
```

Decidir entre opcao A (simples) e opcao B (mais extensivel para futuras linguagens).

### 3. hoverProvider.ts
Language hint hardcoded no hover tooltip:
```typescript
// Atual:
markdown.appendCodeblock(`${originalKeyword}`, 'csharp');

// Novo:
const vscodeLangId = getVSCodeLanguageId(detectedLanguage);
markdown.appendCodeblock(`${originalKeyword}`, vscodeLangId);
```

Mapeamento: `'CSharp'` -> `'csharp'`, `'Python'` -> `'python'`

### 4. Verificar autoTranslateManager.ts
Pesquisa anterior encontrou referencias a `.cs` em comentarios. Verificar se ha logica hardcoded que precisa ser atualizada.

### 5. Verificar coreBridge.ts
Verificar se o coreBridge passa a extensao do arquivo ao invocar o Core .NET. O Core precisa da extensao para resolver o adapter correto via LanguageRegistry.
