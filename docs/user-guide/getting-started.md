# Primeiros Passos

## Indice

- [Abrir um projeto](#abrir-um-projeto)
- [Ativar a traducao](#ativar-a-traducao)
- [Entender a interface](#entender-a-interface)
- [Workflow basico](#workflow-basico)
- [Usando anotacoes tradu](#usando-anotacoes-tradu)

## Abrir um projeto

1. Abrir VS Code
2. Abrir uma pasta que contenha ficheiros `.cs` ou `.py` (`File > Open Folder`)
3. A extensao Babel TCC ativa automaticamente ao detectar ficheiros C# ou Python

## Ativar a traducao

1. Abrir um ficheiro `.cs` ou `.py`
2. Pressionar `Ctrl+Shift+P` para abrir o Command Palette
3. Executar `Babel TCC: Traduzir Codigo (Editavel)` ou `Babel TCC: Traduzir Codigo (Readonly)`
4. O codigo traduzido aparece num painel lateral

Alternativamente:
- Usar `Babel TCC: Toggle Translation` para ativar/desativar
- Usar `Babel TCC: Show Original Code` para voltar ao codigo original
- Clicar no idioma na barra de status para mudar o idioma alvo

## Entender a interface

### Barra de Status
No canto inferior direito aparece o idioma ativo (ex: `PT-BR`). Clicar abre o seletor de idioma.

### Painel Traduzido
O painel lateral mostra o codigo com keywords traduzidas. O ficheiro original (no disco) nao e alterado.

### Hover
Ao passar o mouse sobre uma keyword traduzida (ex: `classe`), um tooltip mostra a keyword original (`class`).

### Autocomplete
Ao digitar no painel traduzido, sugestoes de keywords traduzidas aparecem automaticamente.

## Workflow basico

1. **Ler:** Abrir ficheiro `.cs` ou `.py` e usar `Traduzir Codigo` para ver em PT-BR
2. **Editar:** Editar no painel traduzido usando keywords PT-BR
3. **Salvar:** Ao salvar, o codigo e automaticamente traduzido de volta para a linguagem original e gravado no disco
4. **Compilar/Executar:** O ficheiro no disco e codigo valido da linguagem original — compilar ou executar normalmente

## Usando anotacoes tradu

Para traduzir identificadores customizados (nomes de classes, metodos, variaveis), usar comentarios `// tradu:`:

```csharp
// Formato simples - traduz o identificador da linha
public class Student // tradu:Aluno

// Formato metodo+params - traduz metodo e parametros
public int Add(int a, int b) // tradu:Somar,a:primeiro,b:segundo

// Formato literal - traduz string literal
string label = "Total: "; // tradu:"Total: "
```

As anotacoes sao processadas automaticamente pela extensao.
