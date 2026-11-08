# Resolução de Problemas

## Índice

- [Extensão não ativa](#extensão-não-ativa)
- [Pasta aberta em Modo Restrito](#pasta-aberta-em-modo-restrito)
- [Tradução não aparece](#tradução-não-aparece)
- [Keywords não traduzidas](#keywords-não-traduzidas)
- [Identificadores não traduzidos](#identificadores-não-traduzidos)
- [Erro ao salvar](#erro-ao-salvar)
- [Performance lenta](#performance-lenta)
- [Reportar um bug](#reportar-um-bug)

## Extensão não ativa

**Sintomas:** Nenhuma opção "Babel TCC" no Command Palette, sem ícone na barra de status.

**Soluções:**
1. Verificar que o arquivo aberto tem extensão `.cs`, `.py`, `.js`, `.alg` ou `.por`
2. Verificar que a extensão está instalada: Extensions > procurar "Babel TCC"
3. Verificar o Output Channel: `View > Output` > selecionar "Babel TCC"
4. Reiniciar VS Code

## Pasta aberta em Modo Restrito

**Sintomas:** o VS Code mostra "Modo Restrito" na barra de título ou na barra de status, normalmente
numa pasta baixada da internet ou recebida de outra pessoa.

**O que acontece:** a extensão continua funcionando, com as tabelas de tradução que vêm com ela. Duas
coisas só valem depois que você confia na pasta:
- o `babel-tcc.translationsPath` definido nas configurações **da pasta** (`.vscode/settings.json`). O
  definido nas suas configurações de usuário continua valendo;
- a detecção automática de uma pasta `babel-tcc-translations` ao lado da pasta aberta.

O motivo é proteger o arquivo original: ao salvar a visão traduzida, a extensão usa as tabelas de
tradução para reescrever o original, e uma pasta desconhecida poderia trazer tabelas adulteradas.

**Para usar suas traduções:** confie na pasta (Command Palette > `Workspaces: Manage Workspace Trust`)
e recarregue a janela (`Developer: Reload Window`). O caminho das traduções é escolhido quando a
extensão inicia.

**Palavras traduzidas dentro de caixinhas:** em Modo Restrito, o VS Code destaca com uma caixa todo
caractere fora do alfabeto latino básico (configuração `editor.unicodeHighlight.nonBasicASCII`, que vem
como `inUntrustedWorkspace`). É uma proteção do próprio VS Code contra caracteres que imitam letras
comuns, e vale para qualquer arquivo da pasta, não só para a visão traduzida. Os caracteres do idioma
da interface do VS Code ficam liberados, por isso o português aparece normal e idiomas como chinês,
japonês ou árabe aparecem com caixas. A extensão não desliga essa proteção; confiar na pasta remove as
caixas.

**Workspaces virtuais** (github.dev, vscode.dev): a extensão não funciona, porque precisa de arquivos
reais no disco e roda o motor de tradução como processo local.

## Tradução não aparece

**Sintomas:** A visualização traduzida mostra o código original sem tradução.

**Soluções:**
1. Verificar que `babel-tcc.enabled` está `true` nas settings
2. Se estiver no pacote universal, verificar que o .NET 8.0 **Runtime** está instalado: `dotnet --list-runtimes` (procure `Microsoft.NETCore.App 8.x`). Os pacotes por plataforma embutem o runtime e dispensam isso.
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
1. A extensão protege o arquivo original — se a tradução reversa falhar, o arquivo não é sobrescrito
2. Verificar o Output Channel para detalhes do erro
3. Verificar que o Core está acessível

## Performance lenta

**Sintomas:** A tradução demora mais de 2 segundos.

**Soluções:**
1. Arquivos muito grandes (> 2000 linhas) podem demorar mais
2. Verificar que não há processos Core pendurados: fechar e reabrir VS Code
3. O cache de traduções evita retraduzir sem mudanças

## Reportar um bug

Ao reportar um bug, incluir:

1. **Versão do VS Code:** `Help > About`
2. **Versão da extensão:** Extensions > Babel TCC
3. **Runtime .NET:** `dotnet --list-runtimes` (ou "nenhum", se estiver usando um pacote por plataforma)
4. **Sistema operacional:** Windows/macOS/Linux
5. **Passos para reproduzir:** O que fez exatamente
6. **Resultado esperado:** O que devia acontecer
7. **Resultado obtido:** O que aconteceu
8. **Output Channel:** Copiar logs de `View > Output > Babel TCC`

Abrir issue em: https://github.com/NFAsylum/babel-tcc/issues
