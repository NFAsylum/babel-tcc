using System;
using System.Collections.Generic;

namespace Examples
{
    public class Calculator // tradu[pt-br]:Calculadora
    {
        private List<double> history; // tradu[pt-br]:historico

        public Calculator() // tradu[pt-br]:Calculadora
        {
            history = new List<double>();
        }

        public double Add(double a, double b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
        {
            double result = a + b;
            history.Add(result);
            return result;
        }

        public double Subtract(double a, double b) // tradu[pt-br]:Subtrair,a:primeiroNumero,b:segundoNumero
        {
            double result = a - b;
            history.Add(result);
            return result;
        }

        public double Divide(double a, double b) // tradu[pt-br]:Dividir,a:dividendo,b:divisor
        {
            if (b == 0)
            {
                throw new DivideByZeroException("Cannot divide by zero");
            }
            double result = a / b;
            history.Add(result);
            return result;
        }

        public void PrintHistory() // tradu[pt-br]:ImprimirHistorico
        {
            foreach (var item in history)
            {
                Console.WriteLine(item);
            }
        }

        public void ClearHistory() // tradu[pt-br]:LimparHistorico
        {
            history.Clear();
        }
    }
}
