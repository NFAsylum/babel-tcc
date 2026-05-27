# Primeiros Passos

## Índice

- [Abrir um projeto](#abrir-um-projeto)
- [Ativar a tradução](#ativar-a-tradução)
- [Entender a interface](#entender-a-interface)
- [Workflow básico](#workflow-básico)
- [Usando anotações tradu](#usando-anotações-tradu)

## Abrir um projeto

1. Abrir VS Code
2. Abrir uma pasta que contenha ficheiros `.cs` ou `.py` (`File > Open Folder`)
3. A extensão Babel TCC ativa automaticamente ao detectar ficheiros C# ou Python

## Ativar a tradução

1. Abrir um ficheiro `.cs` ou `.py`
2. Pressionar `Ctrl+Shift+P` para abrir o Command Palette
3. Executar `Babel TCC: Abrir Visualização Traduzida (Editável)` ou `Babel TCC: Abrir Visualização Traduzida (Somente Leitura)`
4. O código traduzido abre num editor ao lado (`ViewColumn.Beside`)

Alternativamente:
- Usar `Babel TCC: Alternar Tradução` para ativar/desativar
- Usar `Babel TCC: Mostrar Código Original` para voltar ao código original
- Clicar no idioma na barra de status para mudar o idioma alvo

## Entender a interface

### Barra de Status
No canto inferior direito aparece o idioma ativo (ex: `PT-BR`). Clicar abre o seletor de idioma.

### Visualização Traduzida
O editor ao lado mostra o código com keywords traduzidas. O ficheiro original (no disco) não é alterado.

### Hover
Ao passar o mouse sobre uma keyword traduzida (ex: `classe`), um tooltip mostra a keyword original (`class`).

### Autocomplete
Ao digitar no painel traduzido, sugestões de keywords traduzidas aparecem automaticamente.

## Workflow básico

1. **Ler:** Abrir ficheiro `.cs` ou `.py` e usar `Babel TCC: Abrir Visualização Traduzida (Editável)` para ver em PT-BR
2. **Editar:** Editar no painel traduzido usando keywords PT-BR
3. **Salvar:** Ao salvar, o código é automaticamente traduzido de volta para a linguagem original e gravado no disco
4. **Compilar/Executar:** O ficheiro no disco é código válido da linguagem original — compilar ou executar normalmente

## Usando anotações tradu

Para traduzir identificadores customizados (nomes de classes, métodos, variáveis), usar comentários `// tradu[pt-br]:`:

```csharp
// Formato simples - traduz o identificador da linha
public class Student // tradu[pt-br]:Aluno

// Formato método+params - traduz método e parâmetros
public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo

// Formato literal - traduz string literal
string label = "Total: "; // tradu[pt-br]:"Total: "
```

As anotações são processadas automaticamente pela extensão.
