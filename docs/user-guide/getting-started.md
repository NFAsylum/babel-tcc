# Primeiros Passos

## Índice

- [Abrir um projeto](#abrir-um-projeto)
- [Ativar a tradução](#ativar-a-tradução)
- [Entender a interface](#entender-a-interface)
- [Workflow básico](#workflow-básico)
- [Usando anotações tradu](#usando-anotações-tradu)

## Abrir um projeto

1. Abrir VS Code
2. Abrir uma pasta que contenha arquivos `.cs` ou `.py` (`File > Open Folder`)
3. A extensão Babel TCC ativa automaticamente ao detectar arquivos C# ou Python

## Ativar a tradução

1. Abrir um arquivo `.cs` ou `.py`
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
