# Tarefa 083 - Cleanup incremental (LOW-001 a LOW-007)

## Prioridade: LOW

## Problemas

### LOW-001: Node.js 18 vs 20 na documentacao
CONTRIBUTING.md e setup-ambiente.md dizem Node.js 18+ mas CI usa 20.
Fix: atualizar docs para Node.js 20+.

### LOW-002: Exemplos antigos com violacoes
Arquivos em examples/csharp/ (Calculator/, HelloWorld/, TodoApp/):
- Falta `public` em classes/metodos (viola CONTRIBUTING.md)
- Constructors, ternarios, nullable (viola padroes-codigo.md)
- TodoItem sem `using System`
- Calculator/README.md parametros em ingles vs codigo em portugues
- Sem .csproj
- Formato `tradu[pt-br]:` nos exemplos vs `tradu:` nos docs (inconsistente)

Fix: atualizar exemplos para seguir convencoes e formato consistente.

### LOW-003: Tarefa 050 pendente mas implementada
Diretorio tarefas/tarefa050-processo-persistente-corebridge.pendente/
9 referencias a startProcess/requestQueue/crashCount em coreBridge.ts mostram que a funcionalidade foi implementada.
Fix: renomear para .finalizada.

### LOW-004: Encapsulacao TypeScript
Quase todos os campos em quase todas as classes TS sao public. Campos internos expostos porque testes acessam diretamente.
Fix: avaliar quais campos devem ser private e ajustar testes para usar metodos publicos. Pode ser feito incrementalmente.

### LOW-005: Double lookup pattern (ContainsKey + indexer)
Arquivos: CSharpKeywordMap, PythonKeywordMap, IdentifierMap, KeywordTable, LanguageTable, LanguageRegistry, JsonLoader.
Usar ContainsKey seguido de indexer em vez de TryGetValue. Em JsonLoader com ConcurrentDictionary, ha race condition teorica.
Fix: trocar para TryGetValue. Nota: CONTRIBUTING.md proibe nullable, mas TryGetValue com `out` requer nullable. Avaliar excecao na convencao.

### LOW-006: Formato tradu inconsistente na documentacao
Docs/README mostram: `// tradu:NomeTraduzido` (sem seletor de idioma)
Exemplos usam: `// tradu[pt-br]:NomeTraduzido` (com seletor)
Fix: padronizar documentacao para mostrar formato com seletor.

### LOW-007: Problemas diversos
- adding-new-ide.md: falta documentar modo persistente do protocolo
- eslintrc.json: test/ excluido do lint
- (removido: validate-translations em ci.yml ja usa setup-python@v5)
- keywordMap.test.ts:67: teste passa por parse error, nao por "unsupported language"
- translatedContentProvider.ts:112: log hardcoded "saved original C#" para Python
- coreBridge.ts:118: stdout! sem null check (stderr tem check)
- coreBridge.ts:254: resolveTranslationsPath public mas so usado no constructor
- package.json: 2 titulos em portugues, 3 em ingles; falta acentos; falta icon
