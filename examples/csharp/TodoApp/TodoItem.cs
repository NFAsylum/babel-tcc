namespace TodoApp // tradu:AplicacaoTarefas
{
    public class TodoItem // tradu:ItemTarefa
    {
        public int Id; // tradu:Identificador
        public string Title; // tradu:Titulo
        public string Description; // tradu:Descricao
        public bool IsCompleted; // tradu:EstaConcluido
        public DateTime CreatedAt; // tradu:CriadoEm

        public TodoItem(int id, string title, string description) // tradu:ItemTarefa,id:identificador,title:titulo,description:descricao
        {
            Id = id;
            Title = title;
            Description = description;
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }

        public void MarkAsCompleted() // tradu:MarcarComoConcluido
        {
            IsCompleted = true;
        }

        public override string ToString() // tradu:ParaTexto
        {
            string status = IsCompleted ? "Done" : "Pending"; // tradu:estado,"Concluido","Pendente"
            return $"[{status}] {Title}: {Description}";
        }
    }
}
