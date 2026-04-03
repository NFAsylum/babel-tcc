# Tarefa 074 - Eliminar hardcodes e duplicacao de strings na extensao VS Code

## Prioridade: HIGH

## Problema
Adicionar uma linguagem requer mudancas em 7 lugares na extensao VS Code. Muitas dessas sao strings duplicadas (extensoes de arquivo, nomes de linguagem, patterns de file watcher). Esquecer qualquer um causa falha silenciosa.

Hardcodes atuais:
1. `languageDetector.ts` — `SUPPORTED_EXTENSIONS = { '.cs': 'CSharp', '.py': 'Python' }`
2. `package.json` — `activationEvents: ["onLanguage:csharp", "onLanguage:python"]`
3. `package.json` — `languages: [{ id: "mlc-csharp" }, { id: "mlc-python" }]`
4. `package.json` — `grammars: [{ language: "mlc-csharp" }, { language: "mlc-python" }]`
5. `extension.ts` — `createFileSystemWatcher('**/*.{cs,py}')`
6. `hoverProvider.ts` — `VSCODE_LANGUAGE_MAP = { 'CSharp': 'csharp', 'Python': 'python' }`
7. Cada linguagem precisa de um arquivo `.tmLanguage.json` separado

## Escopo

### 1. Centralizar configuracao de linguagens
Criar um unico arquivo/constante que define todas as linguagens suportadas:
```typescript
const SUPPORTED_LANGUAGES = [
  { name: 'CSharp', extensions: ['.cs'], vscodeLangId: 'csharp', activation: 'onLanguage:csharp' },
  { name: 'Python', extensions: ['.py'], vscodeLangId: 'python', activation: 'onLanguage:python' },
];
```

### 2. Derivar tudo do registro central
- `languageDetector.ts` — construir SUPPORTED_EXTENSIONS a partir de SUPPORTED_LANGUAGES
- `extension.ts` — file watcher pattern construido dinamicamente a partir de extensoes registradas
- `hoverProvider.ts` — VSCODE_LANGUAGE_MAP construido a partir de SUPPORTED_LANGUAGES

### 3. Gerar package.json contributions dinamicamente (ou documentar checklist)
- activationEvents, languages, grammars no package.json nao podem ser gerados em runtime (VS Code le estaticamente)
- Opcao A: script que gera as secoes do package.json a partir da config
- Opcao B: documentar checklist obrigatoria no adding-new-language.md
- Opcao C: teste que verifica consistencia entre registro TypeScript e package.json

### 4. Criar teste de consistencia
Teste que verifica que todas as linguagens em SUPPORTED_LANGUAGES:
- Tem entrada em activationEvents no package.json
- Tem entrada em languages no package.json
- Tem entrada em grammars no package.json
- Tem arquivo .tmLanguage.json correspondente
- Tem mapeamento no VSCODE_LANGUAGE_MAP
