shiyou System;
shiyou System.Collections.Generic;

meimaikuukan Examples
{
    koukai kurasu Calculator // tradu[pt-br]:Calculadora
    {
        shiyuu List<nibai> history; // tradu[pt-br]:historico

        koukai Calculator() // tradu[pt-br]:Calculadora
        {
            history = atarashii List<nibai>();
        }

        koukai nibai Add(nibai a, nibai b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
        {
            nibai result = a + b;
            history.Add(result);
            modoru result;
        }

        koukai nibai Subtract(nibai a, nibai b) // tradu[pt-br]:Subtrair,a:primeiroNumero,b:segundoNumero
        {
            nibai result = a - b;
            history.Add(result);
            modoru result;
        }

        koukai nibai Divide(nibai a, nibai b) // tradu[pt-br]:Dividir,a:dividendo,b:divisor
        {
            moshi (b == 0)
            {
                nageru atarashii DivideByZeroException("Cannot divide by zero");
            }
            nibai result = a / b;
            history.Add(result);
            modoru result;
        }

        koukai kuuhaku PrintHistory() // tradu[pt-br]:ImprimirHistorico
        {
            subetekurikaeshi (var item naka history)
            {
                Console.WriteLine(item);
            }
        }

        koukai kuuhaku ClearHistory() // tradu[pt-br]:LimparHistorico
        {
            history.Clear();
        }
    }
}
