// Exemplo de JavaScript para o Babel TCC.
// As palavras-chave abaixo (function, const, if, return, class...) aparecem
// no idioma natural escolhido. Já este comentário, mesmo citando return e if,
// deve permanecer intacto.

/*
 * Comentário de bloco: as palavras function e while citadas aqui
 * NÃO devem mudar, pois estão dentro de um comentário.
 */

const PI = 3.14159;
let contador = 0;

class Calculadora extends Object {
  constructor(nome) {
    super();
    this.nome = nome;
  }

  somar(a, b) {
    return a + b;
  }

  dividir(a, b) {
    if (b === 0) {
      throw new Error("Divisao por zero");
    } else {
      return a / b;
    }
  }
}

function executar() {
  const calc = new Calculadora("Principal");

  for (let i = 0; i < 3; i = i + 1) {
    contador = contador + 1;
  }

  while (contador > 0) {
    contador = contador - 1;
  }

  // String com palavra-chave dentro: o 'if' abaixo fica literal, sem mudar.
  const mensagem = "if isto aparecer no texto, permanece igual";

  // Template literal: o 'return' dentro das crases fica literal.
  const rotulo = `resultado return ${calc.somar(2, 2)} fim`;

  switch (contador) {
    case 0:
      break;
    default:
      break;
  }

  let valor = null;
  const ehNumero = typeof PI === "number";
  if (ehNumero === true) {
    valor = calc.dividir(10, 2);
  }

  return valor;
}

export { Calculadora, executar };