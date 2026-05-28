# Definition of Done - Tarefa 111

- [x] `stat()` devolve o `size` do conteudo traduzido + `mtime` que avanca
- [x] Troca de idioma re-traduz no lugar via `invalidatePath` (sem abrir/fechar/refocar aba)
- [x] Foco nao muda e nenhuma aba traduzida anterior sobra ao trocar de idioma
- [x] Uma visao traduzida por arquivo (handleActiveEditorChange via isAnyTranslatedTabOpenForPath)
- [x] Fechamentos de aba (toggle on/off e readonly) usam closeTab(uri) por busca fresca
- [x] doWriteFile descarta todos os idiomas em cache do arquivo ao salvar
- [x] file-watcher chama invalidatePath quando o original muda no disco
- [x] Testes da extensao passam (vitest) - 195 passam
- [x] Build da extensao passa (esbuild + eslint)
- [x] DT-011 registrado/atualizado em docs/decisoes-tecnicas.md
- [x] Validacao manual no editor: trocar idioma com 2+ arquivos abertos atualiza todas as abas no
      lugar, sem criar aba nova nem deixar aba antiga

