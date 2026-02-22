using System; // tradu:Sistema

namespace HelloWorld // tradu:OlaMundo
{
    class Program // tradu:Programa
    {
        static void Main(string[] args) // tradu:Principal,args:argumentos
        {
            string greeting = "Hello, World!"; // tradu:saudacao
            Console.WriteLine(greeting); // tradu:"Ola, Mundo!"

            string name = "Developer"; // tradu:nome
            Console.WriteLine($"Welcome, {name}!"); // tradu:"Bem-vindo, {nome}!"

            int count = 5; // tradu:contagem
            for (int i = 0; i < count; i++) // tradu:i:indice
            {
                Console.WriteLine($"Iteration {i + 1}"); // tradu:"Iteracao {indice + 1}"
            }

            if (count > 3) // tradu:contagem
            {
                Console.WriteLine("Count is greater than 3"); // tradu:"Contagem e maior que 3"
            }
            else
            {
                Console.WriteLine("Count is 3 or less"); // tradu:"Contagem e 3 ou menos"
            }
        }
    }
}
