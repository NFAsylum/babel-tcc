# Resolução de Problemas

## Índice

- [Extensão não ativa](#extensão-não-ativa)
- [Tradução não aparece](#tradução-não-aparece)
- [Keywords não traduzidas](#keywords-não-traduzidas)
- [Identificadores não traduzidos](#identificadores-não-traduzidos)
- [Erro ao salvar](#erro-ao-salvar)
- [Performance lenta](#performance-lenta)
- [Reportar um bug](#reportar-um-bug)

## Extensão não ativa

**Sintomas:** Nenhuma opção "Babel TCC" no Command Palette, sem ícone na barra de status.

**Soluções:**
1. Verificar que o ficheiro aberto tem extensão `.cs`, `.py`, `.alg` ou `.por`
2. Verificar que a extensão está instalada: Extensions > procurar "Babel TCC"
3. Verificar o Output Channel: `View > Output` > selecionar "Babel TCC"
4. Reiniciar VS Code

## Tradução não aparece

**Sintomas:** A visualização traduzida mostra o código original sem tradução.

**Soluções:**
1. Verificar que `babel-tcc.enabled` está `true` nas settings
2. Verificar que .NET 8.0 Runtime está instalado: `dotnet --version`
3. Verificar o Output Channel para erros do CoreBridge
4. Verificar que os binários Core existem em `<extensão>/bin/`

## Keywords não traduzidas

**Sintomas:** Algumas keywords aparecem em inglês na visualização traduzida.

**Soluções:**
1. A keyword pode não ter tradução na tabela do idioma selecionado
2. Verificar o idioma ativo na barra de status
3. Reportar keyword faltante como issue no GitHub

## Identificadores não traduzidos

**Sintomas:** Nomes de classes/métodos aparecem em inglês.

**Soluções:**
1. Identificadores só são traduzidos se anotados com `// tradu[lang]:` no próprio código
2. Verificar a sintaxe da anotação: `// tradu[pt-br]:NomeTraduzido`
3. Lembrar que a anotação precisa estar no arquivo — não há mapa de identificadores global ou persistente

## Erro ao salvar

**Sintomas:** Mensagem de erro ao salvar documento traduzido.

**Soluções:**
1. A extensão protege o ficheiro original — se a tradução reversa falhar, o ficheiro não é sobrescrito
2. Verificar o Output Channel para detalhes do erro
3. Verificar que o Core está acessível

## Performance lenta

**Sintomas:** A tradução demora mais de 2 segundos.

**Soluções:**
1. Ficheiros muito grandes (> 2000 linhas) podem demorar mais
2. Verificar que não há processos Core pendurados: fechar e reabrir VS Code
3. O cache de traduções evita retraduzir sem mudanças

## Reportar um bug

Ao reportar um bug, incluir:

1. **Versão do VS Code:** `Help > About`
2. **Versão da extensão:** Extensions > Babel TCC
3. **Versão do .NET:** `dotnet --version`
4. **Sistema operativo:** Windows/macOS/Linux
5. **Passos para reproduzir:** O que fez exatamente
6. **Resultado esperado:** O que devia acontecer
7. **Resultado obtido:** O que aconteceu
8. **Output Channel:** Copiar logs de `View > Output > Babel TCC`

Abrir issue em: https://github.com/NFAsylum/babel-tcc/issues
