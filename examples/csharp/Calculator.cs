using System;
using System.Collections.Generic;

namespace Examples
{
    public class Calculator // tradu:Calculadora
    {
        private List<double> history; // tradu:historico

        public Calculator() // tradu:Calculadora
        {
            history = new List<double>();
        }

        public double Add(double a, double b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
        {
            double result = a + b;
            history.Add(result);
            return result;
        }

        public double Subtract(double a, double b) // tradu:Subtrair,a:primeiroNumero,b:segundoNumero
        {
            double result = a - b;
            history.Add(result);
            return result;
        }

        public double Divide(double a, double b) // tradu:Dividir,a:dividendo,b:divisor
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero");
            }
            double result = a / b;
            history.Add(result);
            return result;
        }

        public void PrintHistory() // tradu:ImprimirHistorico
        {
            foreach (var item in history)
            {
                Console.WriteLine(item);
            }
        }

        public void ClearHistory() // tradu:LimparHistorico
        {
            history.Clear();
        }
    }
}
