namespace CalculatorApp // tradu[pt-br]:AplicacaoCalculadora
{
    public class Calculator // tradu[pt-br]:Calculadora
    {
        public int OperationCount; // tradu[pt-br]:ContagemOperacoes

        public Calculator() // tradu[pt-br]:Calculadora
        {
            OperationCount = 0;
        }

        public double Add(double a, double b) // tradu[pt-br]:Somar,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a + b;
        }

        public double Subtract(double a, double b) // tradu[pt-br]:Subtrair,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a - b;
        }

        public double Multiply(double a, double b) // tradu[pt-br]:Multiplicar,a:primeiroNumero,b:segundoNumero
        {
            OperationCount++;
            return a * b;
        }

        public double Divide(double a, double b) // tradu[pt-br]:Dividir,a:dividendo,b:divisor
        {
            if (b == 0)
            {
                return double.NaN;
            }

            OperationCount++;
            return a / b;
        }

        public string GetSummary() // tradu[pt-br]:ObterResumo
        {
            string message = $"Total operations: {OperationCount}"; // tradu[pt-br]:mensagem,"Total de operacoes: {ContagemOperacoes}"
            return message;
        }
    }
}
