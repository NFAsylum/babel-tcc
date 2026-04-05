using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples
{
    public class Student // tradu[pt-br]:Aluno
    {
        public string Name { get; set; } // tradu[pt-br]:Nome
        public int Age { get; set; } // tradu[pt-br]:Idade
        public double Grade { get; set; } // tradu[pt-br]:Nota

        public override string ToString()
        {
            return $"{Name} (Age: {Age}, Grade: {Grade})";
        }
    }

    public class StudentManager // tradu[pt-br]:GerenciadorAlunos
    {
        private readonly List<Student> students; // tradu[pt-br]:alunos

        public StudentManager() // tradu[pt-br]:GerenciadorAlunos
        {
            students = new List<Student>();
        }

        public void AddStudent(Student student) // tradu[pt-br]:AdicionarAluno,student:aluno
        {
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student));
            }
            students.Add(student);
        }

        public Student FindByName(string name) // tradu[pt-br]:BuscarPorNome,name:nome
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

        public List<Student> GetPassingStudents(double minimumGrade) // tradu[pt-br]:ObterAprovados,minimumGrade:notaMinima
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

        public double GetAverageGrade() // tradu[pt-br]:ObterMediaNotas
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

        public void PrintReport() // tradu[pt-br]:ImprimirRelatorio
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
