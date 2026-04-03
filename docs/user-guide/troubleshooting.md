# Resolucao de Problemas

## Indice

- [Extensao nao ativa](#extensao-nao-ativa)
- [Traducao nao aparece](#traducao-nao-aparece)
- [Keywords nao traduzidas](#keywords-nao-traduzidas)
- [Identificadores nao traduzidos](#identificadores-nao-traduzidos)
- [Erro ao salvar](#erro-ao-salvar)
- [Performance lenta](#performance-lenta)
- [Reportar um bug](#reportar-um-bug)

## Extensao nao ativa

**Sintomas:** Nenhuma opcao "Babel TCC" no Command Palette, sem icone na barra de status.

**Solucoes:**
1. Verificar que o ficheiro aberto tem extensao `.cs` ou `.py`
2. Verificar que a extensao esta instalada: Extensions > procurar "Babel TCC"
3. Verificar o Output Channel: `View > Output` > selecionar "Babel TCC"
4. Reiniciar VS Code

## Traducao nao aparece

**Sintomas:** Painel traduzido mostra codigo original sem traducao.

**Solucoes:**
1. Verificar que `babel-tcc.enabled` esta `true` nas settings
2. Verificar que .NET 8.0 Runtime esta instalado: `dotnet --version`
3. Verificar o Output Channel para erros do CoreBridge
4. Verificar que os binarios Core existem em `<extensao>/bin/`

## Keywords nao traduzidas

**Sintomas:** Algumas keywords aparecem em ingles no painel traduzido.

**Solucoes:**
1. A keyword pode nao ter traducao na tabela do idioma selecionado
2. Verificar o idioma ativo na barra de status
3. Reportar keyword faltante como issue no GitHub

## Identificadores nao traduzidos

**Sintomas:** Nomes de classes/metodos aparecem em ingles.

**Solucoes:**
1. Identificadores so sao traduzidos se mapeados via `// tradu:` ou `identifier-map.json`
2. Verificar a sintaxe da anotacao: `// tradu:NomeTraduzido`
3. Verificar que o ficheiro `.multilingual/identifier-map.json` existe e e valido

## Erro ao salvar

**Sintomas:** Mensagem de erro ao salvar documento traduzido.

**Solucoes:**
1. O TranslatedContentProvider protege o ficheiro original - se a traducao reversa falhar, o ficheiro nao e sobrescrito
2. Verificar o Output Channel para detalhes do erro
3. Verificar que o Core esta acessivel

## Performance lenta

**Sintomas:** Traducao demora mais de 2 segundos.

**Solucoes:**
1. Ficheiros muito grandes (> 2000 linhas) podem demorar mais
2. Verificar que nao ha processos Core pendurados: fechar e reabrir VS Code
3. O cache de traducoes evita retraduzir sem mudancas

## Reportar um bug

Ao reportar um bug, incluir:

1. **Versao do VS Code:** `Help > About`
2. **Versao da extensao:** Extensions > Babel TCC
3. **Versao do .NET:** `dotnet --version`
4. **Sistema operativo:** Windows/macOS/Linux
5. **Passos para reproduzir:** O que fez exatamente
6. **Resultado esperado:** O que devia acontecer
7. **Resultado obtido:** O que aconteceu
8. **Output Channel:** Copiar logs de `View > Output > Babel TCC`

Abrir issue em: https://github.com/NFAsylum/babel-tcc/issues
