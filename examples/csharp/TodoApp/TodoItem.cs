namespace TodoApp // tradu[pt-br]:AplicacaoTarefas
{
    public class TodoItem // tradu[pt-br]:ItemTarefa
    {
        public int Id; // tradu[pt-br]:Identificador
        public string Title; // tradu[pt-br]:Titulo
        public string Description; // tradu[pt-br]:Descricao
        public bool IsCompleted; // tradu[pt-br]:EstaConcluido
        public DateTime CreatedAt; // tradu[pt-br]:CriadoEm

        public TodoItem(int id, string title, string description) // tradu[pt-br]:ItemTarefa,id:identificador,title:titulo,description:descricao
        {
            Id = id;
            Title = title;
            Description = description;
            IsCompleted = false;
            CreatedAt = DateTime.Now;
        }

        public void MarkAsCompleted() // tradu[pt-br]:MarcarComoConcluido
        {
            IsCompleted = true;
        }

        public override string ToString() // tradu[pt-br]:ParaTexto
        {
            string status = IsCompleted ? "Done" : "Pending"; // tradu[pt-br]:estado,"Concluido","Pendente"
            return $"[{status}] {Title}: {Description}";
        }
    }
}
