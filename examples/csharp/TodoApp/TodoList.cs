using System.Collections.Generic; // tradu:Sistema.Colecoes.Generico
using System.Linq; // tradu:Sistema.Linq

namespace TodoApp // tradu:AplicacaoTarefas
{
    public class TodoList // tradu:ListaTarefas
    {
        public List<TodoItem> Items; // tradu:Itens
        public int NextId; // tradu:ProximoId

        public TodoList() // tradu:ListaTarefas
        {
            Items = new List<TodoItem>();
            NextId = 1;
        }

        public TodoItem AddItem(string title, string description) // tradu:AdicionarItem,title:titulo,description:descricao
        {
            TodoItem item = new TodoItem(NextId, title, description); // tradu:item
            Items.Add(item);
            NextId++;
            return item;
        }

        public bool RemoveItem(int id) // tradu:RemoverItem,id:identificador
        {
            TodoItem item = Items.FirstOrDefault(i => i.Id == id); // tradu:item
            if (item == null)
            {
                return false;
            }

            Items.Remove(item);
            return true;
        }

        public bool CompleteItem(int id) // tradu:ConcluirItem,id:identificador
        {
            TodoItem item = Items.FirstOrDefault(i => i.Id == id); // tradu:item
            if (item == null)
            {
                return false;
            }

            item.MarkAsCompleted();
            return true;
        }

        public List<TodoItem> GetPendingItems() // tradu:ObterItensPendentes
        {
            return Items.Where(i => !i.IsCompleted).ToList();
        }

        public List<TodoItem> GetCompletedItems() // tradu:ObterItensConcluidos
        {
            return Items.Where(i => i.IsCompleted).ToList();
        }

        public int GetTotalCount() // tradu:ObterContagemTotal
        {
            return Items.Count;
        }

        public int GetPendingCount() // tradu:ObterContagemPendentes
        {
            return Items.Count(i => !i.IsCompleted);
        }
    }
}
