namespace CalculatorApp // tradu:AplicacaoCalculadora
{
    public class Calculator // tradu:Calculadora
    {
        public int OperationCount; // tradu:ContagemOperacoes

        public Calculator() // tradu:Calculadora
        {
            OperationCount = 0;
        }

        public double Add(double a, double b) // tradu:Somar,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a + b;
        }

        public double Subtract(double a, double b) // tradu:Subtrair,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a - b;
        }

        public double Multiply(double a, double b) // tradu:Multiplicar,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a * b;
        }

        public double Divide(double a, double b) // tradu:Dividir,a:dividendo,b:divisor
        {
            if (b == 0)
            {
                return double.NaN;
            }

            OperationCount++;
            return a / b;
        }

        public string GetSummary() // tradu:ObterResumo
        {
            string message = $"Total operations: {OperationCount}"; // tradu:mensagem,"Total de operacoes: {ContagemOperacoes}"
            return message;
        }
    }
}
