class Task: # tradu[pt-br]:Tarefa
    def __init__(self, title, priority): # tradu[pt-br]:titulo,priority:prioridade
        self.title = title
        self.priority = priority
        self.completed = False # tradu[pt-br]:completada

    def complete(self): # tradu[pt-br]:completar
        self.completed = True

    def __str__(self):
        status = "done" if self.completed else "pending"
        return f"[{status}] {self.title} (priority: {self.priority})"


class TaskManager: # tradu[pt-br]:GerenciadorTarefas
    def __init__(self):
        self.tasks = [] # tradu[pt-br]:tarefas

    def add_task(self, title, priority=1): # tradu[pt-br]:adicionar_tarefa,title:titulo,priority:prioridade
        if not title:
            raise ValueError("Title cannot be empty")
        task = Task(title, priority)
        self.tasks.append(task)
        return task

    def remove_task(self, title): # tradu[pt-br]:remover_tarefa,title:titulo
        for i in range(len(self.tasks)):
            if self.tasks[i].title == title:
                del self.tasks[i]
                return True
        return False

    def get_pending(self): # tradu[pt-br]:obter_pendentes
        result = []
        for task in self.tasks:
            if not task.completed:
                result.append(task)
        return result

    def get_by_priority(self, min_priority): # tradu[pt-br]:obter_por_prioridade,min_priority:prioridade_minima
        result = []
        for task in self.tasks:
            if task.priority >= min_priority:
                result.append(task)
        return sorted(result, key=lambda t: t.priority, reverse=True)

    def complete_task(self, title): # tradu[pt-br]:completar_tarefa,title:titulo
        for task in self.tasks:
            if task.title == title:
                task.complete()
                return True
        return False

    def print_summary(self): # tradu[pt-br]:imprimir_resumo
        total = len(self.tasks)
        completed = 0
        for task in self.tasks:
            if task.completed:
                completed += 1
        pending = total - completed

        print(f"Total: {total}")
        print(f"Completed: {completed}")
        print(f"Pending: {pending}")

        for task in self.tasks:
            print(f"  {task}")


if __name__ == "__main__":
    manager = TaskManager()
    manager.add_task("Buy groceries", 2)
    manager.add_task("Write report", 3)
    manager.add_task("Clean house", 1)

    manager.complete_task("Buy groceries")
    manager.print_summary()
