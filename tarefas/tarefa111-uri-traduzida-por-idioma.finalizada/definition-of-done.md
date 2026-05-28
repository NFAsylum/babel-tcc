# Definition of Done - Tarefa 111

- [x] URI virtual traduzida codifica o idioma na query (`?lang=<idioma>`)
- [x] Provider le o idioma da URI (provideContent, buildCacheKey, doWriteFile) com fallback p/ config
- [x] Troca de idioma: abre URI do idioma novo, fecha aba do idioma antigo, restaura o foco
- [x] Uma visao traduzida por arquivo (handleActiveEditorChange via isAnyTranslatedTabOpenForPath)
- [x] file-watcher refresca todas as visoes abertas do arquivo via invalidatePath
- [x] Mock do vscode suporta Uri com query
- [x] Testes da extensao passam (vitest) - 197 passam
- [x] Build da extensao passa (esbuild + eslint)
- [x] DT-011 registrado em docs/decisoes-tecnicas.md
