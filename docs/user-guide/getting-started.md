# Primeiros Passos

## Índice

- [Abrir um projeto](#abrir-um-projeto)
- [Ativar a tradução](#ativar-a-tradução)
- [Atalhos de teclado](#atalhos-de-teclado)
- [Entender a interface](#entender-a-interface)
- [Workflow básico](#workflow-básico)
- [Usando anotações tradu](#usando-anotações-tradu)

## Abrir um projeto

1. Abrir VS Code
2. Abrir uma pasta que contenha arquivos de uma linguagem suportada — `.cs`, `.py`, `.js`, `.alg` ou `.por` (`File > Open Folder`)
3. A extensão Babel TCC ativa automaticamente ao detectar um desses arquivos

## Ativar a tradução

Com um arquivo suportado aberto, há quatro caminhos para a visualização traduzida — todos levam ao mesmo lugar:

- **Botão na barra de título do editor:** o ícone de globo, no canto superior direito do editor, abre a visualização traduzida editável. Dentro dela, o mesmo lugar mostra o botão de voltar ao código original.
- **Menu de contexto:** clicar com o botão direito dentro do editor e escolher `Abrir Visualização Traduzida (Editável)` ou `Abrir Visualização Traduzida (Somente Leitura)`. Dentro da visualização traduzida, o menu oferece `Mostrar Código Original`.
- **Atalho de teclado:** `Ctrl+Alt+U` e depois `V` (ver [Atalhos de teclado](#atalhos-de-teclado)).
- **Command Palette:** `Ctrl+Shift+P` e executar `Babel TCC: Abrir Visualização Traduzida (Editável)` ou `Babel TCC: Abrir Visualização Traduzida (Somente Leitura)`.

O código traduzido abre num editor ao lado (`ViewColumn.Beside`).

O botão e os itens do menu só aparecem onde fazem sentido: num arquivo de linguagem não suportada (um `README.md`, por exemplo) eles não aparecem, e com a extensão desativada (`Alternar Tradução`) os itens de abrir somem. Com o editor dividido, cada barra de título responde pelo próprio arquivo.

Também é possível:
- Usar `Babel TCC: Alternar Tradução` para ativar/desativar
- Clicar no idioma na barra de status para mudar o idioma alvo

## Atalhos de teclado

Os atalhos são em duas etapas (*chord*): primeiro `Ctrl+Alt+U` (`Cmd+Alt+U` no macOS), solta, e então a letra.

| Atalho | Ação | Onde funciona |
|---|---|---|
| `Ctrl+Alt+U` `T` | Alternar Tradução | Em qualquer lugar |
| `Ctrl+Alt+U` `I` | Selecionar Idioma | Em qualquer lugar |
| `Ctrl+Alt+U` `V` | Abrir Visualização Traduzida (Editável) | Com o foco num arquivo suportado |
| `Ctrl+Alt+U` `O` | Mostrar Código Original | Com o foco na visualização traduzida |

O prefixo `Ctrl+Alt+U` não ocupa nenhum atalho padrão do VS Code e não colide com o `AltGr` do teclado ABNT2 — no Windows, `AltGr` chega aos programas como `Ctrl+Alt`, então um atalho na tecla errada dispararia enquanto você digita um caractere. Para trocar algum atalho, use `File > Preferences > Keyboard Shortcuts` e procure por `Babel TCC`.

## Entender a interface

### Barra de Status
No canto inferior direito aparece o idioma ativo (ex: `PT-BR`). Clicar abre o seletor de idioma.

### Visualização Traduzida
O editor ao lado mostra o código com keywords traduzidas. O arquivo original (no disco) não é alterado.

### Hover
Ao passar o mouse sobre uma keyword traduzida (ex: `classe`), um tooltip mostra a keyword original (`class`).

### Autocomplete
Ao digitar no painel traduzido, sugestões de keywords traduzidas aparecem automaticamente.

## Workflow básico

1. **Ler:** Abrir arquivo `.cs` ou `.py` e usar `Babel TCC: Abrir Visualização Traduzida (Editável)` para ver em PT-BR
2. **Editar:** Editar no painel traduzido usando keywords PT-BR
3. **Salvar:** Ao salvar, o código é automaticamente traduzido de volta para a linguagem original e gravado no disco
4. **Compilar/Executar:** O arquivo no disco é código válido da linguagem original — compilar ou executar normalmente

## Usando anotações tradu

Para traduzir identificadores customizados (nomes de classes, métodos, variáveis), usar comentários `// tradu[pt-br]:`:

```csharp
// Formato simples - traduz o identificador da linha
public class Student // tradu[pt-br]:Aluno

// Formato explícito - diz QUAL identificador traduzir
protected readonly ShapeKind kind; // tradu[pt-br]:kind=tipo

// Formato método+params - traduz método e parâmetros
public int Add(int a, int b) // tradu[pt-br]:Somar,a:primeiro,b:segundo

// Formato literal - traduz string literal
string label = "Total: "; // tradu[pt-br]:"Total: "
```

As anotações são processadas automaticamente pela extensão.

### Quando usar o formato explícito

O formato simples traduz **o primeiro identificador da linha**. Isso resolve o caso
comum, em que a linha declara um nome só:

```csharp
public class Student // tradu[pt-br]:Aluno      → o primeiro identificador é Student
```

Mas quando a linha tem mais de um identificador, o primeiro pode não ser o que você
quer. Em C# a declaração vem depois do tipo:

```csharp
protected readonly ShapeKind kind; // tradu[pt-br]:tipo
//                 ^^^^^^^^^ o primeiro identificador é o TIPO, não o campo
```

Aí a tradução cai no tipo `ShapeKind`, e o campo `kind` fica sem traduzir. O formato
`origem=traducao` remove a adivinhação:

```csharp
protected readonly ShapeKind kind; // tradu[pt-br]:kind=tipo
```

Use o formato explícito sempre que a linha tiver mais de um identificador —
declarações de campo e de variável local, `foreach`, e `using` de namespace:

```csharp
using System.Collections.Generic; // tradu[pt-br]:System.Collections.Generic=Sistema.Colecoes.Generico
foreach (TodoItem item in lista)  // tradu[pt-br]:item=tarefa
TodoItem item = new TodoItem();   // tradu[pt-br]:item=tarefa
```

Duas anotações que adivinham o mesmo alvo se sobrescrevem, então o formato explícito
também é o que permite anotar dois `using` do mesmo namespace raiz.

Ele combina com o mapeamento de parâmetros:

```csharp
static void Main(string[] args) // tradu[pt-br]:Main=Principal,args:argumentos
```

Com vários idiomas, a origem se repete em cada segmento, porque cada `[idioma]:valor`
é lido de forma independente:

```csharp
protected readonly ShapeKind kind; // tradu[pt-br]:kind=tipo|[es-es]:kind=tipo
```

O separador é `=`, e não `:`, porque `:` já separa o parâmetro da sua tradução depois
da vírgula — reusá-lo tornaria `Somar,a:primeiro` ambíguo.
