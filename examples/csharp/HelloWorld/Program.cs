using System; // tradu[pt-br]:Sistema

namespace HelloWorld // tradu[pt-br]:OlaMundo
{
    class Program // tradu[pt-br]:Programa
    {
        static void Main(string[] args) // tradu[pt-br]:Principal,args:argumentos
        {
            string greeting = "Hello, World!"; // tradu[pt-br]:saudacao
            Console.WriteLine(greeting); // tradu[pt-br]:"Ola, Mundo!"

            string name = "Developer"; // tradu[pt-br]:nome
            Console.WriteLine($"Welcome, {name}!"); // tradu[pt-br]:"Bem-vindo, {nome}!"

            int count = 5; // tradu[pt-br]:contagem
            for (int i = 0; i < count; i++) // tradu[pt-br]:i:indice
            {
                Console.WriteLine($"Iteration {i + 1}"); // tradu[pt-br]:"Iteracao {indice + 1}"
            }

            if (count > 3) // tradu[pt-br]:contagem
            {
                Console.WriteLine("Count is greater than 3"); // tradu[pt-br]:"Contagem e maior que 3"
            }
            else
            {
                Console.WriteLine("Count is 3 or less"); // tradu[pt-br]:"Contagem e 3 ou menos"
            }
        }
    }
}
