using System; // tradu:Sistema

namespace CalculatorApp // tradu:AplicacaoCalculadora
{
    class Program // tradu:Programa
    {
        static void Main(string[] args) // tradu:Principal,args:argumentos
        {
            Calculator calc = new Calculator(); // tradu:calculadora

            double result = calc.Add(10, 5); // tradu:resultado
            Console.WriteLine($"10 + 5 = {result}");

            result = calc.Subtract(10, 5);
            Console.WriteLine($"10 - 5 = {result}");

            result = calc.Multiply(10, 5);
            Console.WriteLine($"10 * 5 = {result}");

            result = calc.Divide(10, 5);
            Console.WriteLine($"10 / 5 = {result}");

            result = calc.Divide(10, 0);
            Console.WriteLine($"10 / 0 = {result}");

            string summary = calc.GetSummary(); // tradu:resumo
            Console.WriteLine(summary);
        }
    }
}
