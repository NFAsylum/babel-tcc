using System; // tradu[pt-br]:Sistema

namespace CalculatorApp // tradu[pt-br]:AplicacaoCalculadora
{
    class Program // tradu[pt-br]:Programa
    {
        static void Main(string[] args) // tradu[pt-br]:Principal,args:argumentos
        {
            Calculator calc = new Calculator(); // tradu[pt-br]:calculadora

            double result = calc.Add(10, 5); // tradu[pt-br]:resultado
            Console.WriteLine($"10 + 5 = {result}");

            result = calc.Subtract(10, 5);
            Console.WriteLine($"10 - 5 = {result}");

            result = calc.Multiply(10, 5);
            Console.WriteLine($"10 * 5 = {result}");

            result = calc.Divide(10, 5);
            Console.WriteLine($"10 / 5 = {result}");

            result = calc.Divide(10, 0);
            Console.WriteLine($"10 / 0 = {result}");

            string summary = calc.GetSummary(); // tradu[pt-br]:resumo
            Console.WriteLine(summary);
        }
    }
}
