using System.Collections.Generic; // tradu[pt-br]:Sistema.Colecoes.Generico
using System.Linq; // tradu[pt-br]:Sistema.Linq

namespace TodoApp // tradu[pt-br]:AplicacaoTarefas
{
    public class TodoList // tradu[pt-br]:ListaTarefas
    {
        public List<TodoItem> Items; // tradu[pt-br]:Itens
        public int NextId; // tradu[pt-br]:ProximoId

        public TodoList() // tradu[pt-br]:ListaTarefas
        {
            Items = new List<TodoItem>();
            NextId = 1;
        }

        public TodoItem AddItem(string title, string description) // tradu[pt-br]:AdicionarItem,title:titulo,description:descricao
        {
            TodoItem item = new TodoItem(NextId, title, description); // tradu[pt-br]:item
            Items.Add(item);
            NextId++;
            return item;
        }

        public bool RemoveItem(int id) // tradu[pt-br]:RemoverItem,id:identificador
        {
            TodoItem item = Items.FirstOrDefault(i => i.Id == id); // tradu[pt-br]:item
            if (item == null)
            {
                return false;
            }

            Items.Remove(item);
            return true;
        }

        public bool CompleteItem(int id) // tradu[pt-br]:ConcluirItem,id:identificador
        {
            TodoItem item = Items.FirstOrDefault(i => i.Id == id); // tradu[pt-br]:item
            if (item == null)
            {
                return false;
            }

            item.MarkAsCompleted();
            return true;
        }

        public List<TodoItem> GetPendingItems() // tradu[pt-br]:ObterItensPendentes
        {
            return Items.Where(i => !i.IsCompleted).ToList();
        }

        public List<TodoItem> GetCompletedItems() // tradu[pt-br]:ObterItensConcluidos
        {
            return Items.Where(i => i.IsCompleted).ToList();
        }

        public int GetTotalCount() // tradu[pt-br]:ObterContagemTotal
        {
            return Items.Count;
        }

        public int GetPendingCount() // tradu[pt-br]:ObterContagemPendentes
        {
            return Items.Count(i => !i.IsCompleted);
        }
    }
}
