// Exemplo Portugol Studio: calculadora simples
// Demonstra: estrutura programa/funcao, tipos, condicionais, escolha-caso

programa
{
   funcao real somar(real a, real b)
   {
      retorne a + b
   }

   funcao real subtrair(real a, real b)
   {
      retorne a - b
   }

   funcao real multiplicar(real a, real b)
   {
      retorne a * b
   }

   funcao real dividir(real a, real b)
   {
      se (b == 0)
      {
         escreva("Divisao por zero nao permitida\n")
         retorne 0
      }
      retorne a / b
   }

   funcao inicio()
   {
      real x = 10.5
      real y = 3.0
      inteiro operacao

      escreva("1) Somar\n")
      escreva("2) Subtrair\n")
      escreva("3) Multiplicar\n")
      escreva("4) Dividir\n\n")
      escreva("Escolha uma operacao: ")
      leia(operacao)

      escolha (operacao)
      {
         caso 1:
            escreva("Resultado: ", somar(x, y), "\n")
            pare
         caso 2:
            escreva("Resultado: ", subtrair(x, y), "\n")
            pare
         caso 3:
            escreva("Resultado: ", multiplicar(x, y), "\n")
            pare
         caso 4:
            escreva("Resultado: ", dividir(x, y), "\n")
            pare
         caso contrario:
            escreva("Operacao desconhecida\n")
      }
   }
}
