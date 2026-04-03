using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples
{
    public class Student // tradu:Aluno
    {
        public string Name { get; set; } // tradu:Nome
        public int Age { get; set; } // tradu:Idade
        public double Grade { get; set; } // tradu:Nota

        public override string ToString()
        {
            return $"{Name} (Age: {Age}, Grade: {Grade})";
        }
    }

    public class StudentManager // tradu:GerenciadorAlunos
    {
        private readonly List<Student> students; // tradu:alunos

        public StudentManager() // tradu:GerenciadorAlunos
        {
            students = new List<Student>();
        }

        public void AddStudent(Student student) // tradu:AdicionarAluno,student:aluno
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }
            students.Add(student);
        }

        public Student FindByName(string name) // tradu:BuscarPorNome,name:nome
        {
            foreach (var student in students)
            {
                if (student.Name == name)
                {
                    return student;
                }
            }
            return null;
        }

        public List<Student> GetPassingStudents(double minimumGrade) // tradu:ObterAprovados,minimumGrade:notaMinima
        {
            var passing = new List<Student>();
            for (int i = 0; i < students.Count; i++)
            {
                if (students[i].Grade >= minimumGrade)
                {
                    passing.Add(students[i]);
                }
            }
            return passing;
        }

        public double GetAverageGrade() // tradu:ObterMediaNotas
        {
            if (students.Count == 0)
            {
                return 0;
            }

            double total = 0;
            foreach (var student in students)
            {
                total += student.Grade;
            }
            return total / students.Count;
        }

        public void PrintReport() // tradu:ImprimirRelatorio
        {
            Console.WriteLine("=== Student Report ===");
            Console.WriteLine($"Total: {students.Count}");
            Console.WriteLine($"Average: {GetAverageGrade():F1}");

            foreach (var student in students)
            {
                bool passed = student.Grade >= 7.0;
                string status = passed ? "PASSED" : "FAILED";
                Console.WriteLine($"  {student.Name}: {student.Grade:F1} - {status}");
            }
        }
    }
}
