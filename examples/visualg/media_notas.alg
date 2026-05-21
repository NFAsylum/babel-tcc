// Exemplo VisuAlg: media de notas com classificacao
// Demonstra: leitura, condicionais, estruturas de repeticao, tipos basicos

algoritmo "MediaDeNotas"

var
   quantidade: inteiro
   i: inteiro
   nota: real
   soma: real
   media: real

inicio
   escreva("Quantas notas? ")
   leia(quantidade)

   soma <- 0
   para i de 1 ate quantidade faca
      escreva("Nota ", i, ": ")
      leia(nota)
      soma <- soma + nota
   fimpara

   se quantidade > 0 entao
      media <- soma / quantidade
      escreval("Media: ", media)

      se media >= 7 entao
         escreval("Aprovado")
      senao
         se media >= 5 entao
            escreval("Recuperacao")
         senao
            escreval("Reprovado")
         fimse
      fimse
   senao
      escreval("Nenhuma nota informada")
   fimse
fimalgoritmo
