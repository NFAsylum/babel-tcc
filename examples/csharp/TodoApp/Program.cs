using System; // tradu:Sistema

namespace TodoApp // tradu:AplicacaoTarefas
{
    class Program // tradu:Programa
    {
        static void Main(string[] args) // tradu:Principal,args:argumentos
        {
            TodoList list = new TodoList(); // tradu:lista

            list.AddItem("Buy groceries", "Milk, eggs, bread"); // tradu:"Comprar mantimentos","Leite, ovos, pao"
            list.AddItem("Clean house", "Kitchen and bathroom"); // tradu:"Limpar casa","Cozinha e banheiro"
            list.AddItem("Study C#", "Read chapter 5"); // tradu:"Estudar C#","Ler capitulo 5"

            Console.WriteLine("All items:"); // tradu:"Todos os itens:"
            foreach (TodoItem item in list.Items) // tradu:item
            {
                Console.WriteLine(item.ToString());
            }

            list.CompleteItem(1);
            list.CompleteItem(3);

            Console.WriteLine("\nPending items:"); // tradu:"\nItens pendentes:"
            foreach (TodoItem item in list.GetPendingItems()) // tradu:item
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine("\nCompleted items:"); // tradu:"\nItens concluidos:"
            foreach (TodoItem item in list.GetCompletedItems()) // tradu:item
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine($"\nTotal: {list.GetTotalCount()}, Pending: {list.GetPendingCount()}");
        }
    }
}
