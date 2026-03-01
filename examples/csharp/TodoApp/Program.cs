using System; // tradu[pt-br]:Sistema

namespace TodoApp // tradu[pt-br]:AplicacaoTarefas
{
    class Program // tradu[pt-br]:Programa
    {
        static void Main(string[] args) // tradu[pt-br]:Principal,args:argumentos
        {
            TodoList list = new TodoList(); // tradu[pt-br]:lista

            list.AddItem("Buy groceries", "Milk, eggs, bread"); // tradu[pt-br]:"Comprar mantimentos","Leite, ovos, pao"
            list.AddItem("Clean house", "Kitchen and bathroom"); // tradu[pt-br]:"Limpar casa","Cozinha e banheiro"
            list.AddItem("Study C#", "Read chapter 5"); // tradu[pt-br]:"Estudar C#","Ler capitulo 5"

            Console.WriteLine("All items:"); // tradu[pt-br]:"Todos os itens:"
            foreach (TodoItem item in list.Items) // tradu[pt-br]:item
            {
                Console.WriteLine(item.ToString());
            }

            list.CompleteItem(1);
            list.CompleteItem(3);

            Console.WriteLine("\nPending items:"); // tradu[pt-br]:"\nItens pendentes:"
            foreach (TodoItem item in list.GetPendingItems()) // tradu[pt-br]:item
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine("\nCompleted items:"); // tradu[pt-br]:"\nItens concluidos:"
            foreach (TodoItem item in list.GetCompletedItems()) // tradu[pt-br]:item
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine($"\nTotal: {list.GetTotalCount()}, Pending: {list.GetPendingCount()}");
        }
    }
}
