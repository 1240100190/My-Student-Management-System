using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudentManagementSystem
{
    enum Grade { A, B, C, D, F }

    class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RollNumber { get; set; }
        public string Department { get; set; }
        public double GPA { get; set; }
        public int Attendance { get; set; }
        public Grade LetterGrade { get; set; }
        public DateTime EnrollmentDate { get; set; }

        public string Status => GPA >= 2.0 && Attendance >= 75 ? "Active" : "At Risk";

        public Student() { EnrollmentDate = DateTime.Now; }

        public string Serialize()
        {
            return $"{Id}|{Name}|{RollNumber}|{Department}|{GPA}|{Attendance}|{LetterGrade}|{EnrollmentDate:O}";
        }

        public static Student Deserialize(string line)
        {
            var p = line.Split('|');
            return new Student
            {
                Id = int.Parse(p[0]),
                Name = p[1],
                RollNumber = p[2],
                Department = p[3],
                GPA = double.Parse(p[4]),
                Attendance = int.Parse(p[5]),
                LetterGrade = (Grade)Enum.Parse(typeof(Grade), p[6]),
                EnrollmentDate = DateTime.Parse(p[7])
            };
        }

        public void Display()
        {
            Console.WriteLine($"\n  {"─────────────────────────────────────────"}");
            Console.WriteLine($"  ID           : {Id}");
            Console.WriteLine($"  Name         : {Name}");
            Console.WriteLine($"  Roll Number  : {RollNumber}");
            Console.WriteLine($"  Department   : {Department}");
            Console.WriteLine($"  GPA          : {GPA:F2}");
            Console.WriteLine($"  Grade        : {LetterGrade}");
            Console.WriteLine($"  Attendance   : {Attendance}%");
            Console.WriteLine($"  Enrolled     : {EnrollmentDate:dd-MM-yyyy}");
            Console.WriteLine($"  Status       : {Status}");
        }
    }

    class StudentRepository
    {
        private List<Student> students = new List<Student>();
        private int nextId = 1;
        private const string filePath = "students.txt";

        public StudentRepository()
        {
            LoadData();
        }

        private void LoadData()
        {
            if (!File.Exists(filePath)) return;
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var s = Student.Deserialize(line);
                students.Add(s);
                if (s.Id >= nextId) nextId = s.Id + 1;
            }
        }

        private void SaveData()
        {
            File.WriteAllLines(filePath, students.Select(s => s.Serialize()));
        }

        public void Add(Student s)
        {
            s.Id = nextId++;
            students.Add(s);
            SaveData();
        }

        public List<Student> GetAll() => new List<Student>(students);

        public Student FindById(int id) => students.FirstOrDefault(s => s.Id == id);

        public List<Student> FindByName(string name) =>
            students.Where(s => s.Name.ToLower().Contains(name.ToLower())).ToList();

        public List<Student> FindByDepartment(string dept) =>
            students.Where(s => s.Department.ToLower() == dept.ToLower()).ToList();

        public bool Update(Student updated)
        {
            var existing = FindById(updated.Id);
            if (existing == null) return false;
            existing.Name = updated.Name;
            existing.Department = updated.Department;
            existing.GPA = updated.GPA;
            existing.Attendance = updated.Attendance;
            existing.LetterGrade = updated.LetterGrade;
            SaveData();
            return true;
        }

        public bool Delete(int id)
        {
            var s = FindById(id);
            if (s == null) return false;
            students.Remove(s);
            SaveData();
            return true;
        }

        public List<Student> GetAtRisk() => students.Where(s => s.Status == "At Risk").ToList();

        public double GetAverageGPA() => students.Count == 0 ? 0 : students.Average(s => s.GPA);

        public double GetAverageAttendance() => students.Count == 0 ? 0 : students.Average(s => s.Attendance);

        public int GetTotalCount() => students.Count;
    }

    class StudentService
    {
        private readonly StudentRepository repo;

        public StudentService()
        {
            repo = new StudentRepository();
        }

        public void AddStudent(string name, string roll, string dept, double gpa, int attendance, Grade grade)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Student name cannot be empty.");
            if (string.IsNullOrWhiteSpace(roll))
                throw new ArgumentException("Roll number cannot be empty.");
            if (gpa < 0.0 || gpa > 4.0)
                throw new ArgumentOutOfRangeException("GPA must be between 0.0 and 4.0.");
            if (attendance < 0 || attendance > 100)
                throw new ArgumentOutOfRangeException("Attendance must be between 0 and 100.");

            var student = new Student
            {
                Name = name,
                RollNumber = roll,
                Department = dept,
                GPA = gpa,
                Attendance = attendance,
                LetterGrade = grade
            };
            repo.Add(student);
            Console.WriteLine("\n  Student added successfully.");
        }

        public void DisplayAll()
        {
            var list = repo.GetAll();
            if (list.Count == 0)
            {
                Console.WriteLine("\n  No students found.");
                return;
            }
            Console.WriteLine($"\n  Total Students: {list.Count}");
            foreach (var s in list) s.Display();
        }

        public void SearchByName(string name)
        {
            var results = repo.FindByName(name);
            if (results.Count == 0) { Console.WriteLine("\n  No students found."); return; }
            foreach (var s in results) s.Display();
        }

        public void SearchByDepartment(string dept)
        {
            var results = repo.FindByDepartment(dept);
            if (results.Count == 0) { Console.WriteLine("\n  No students found in that department."); return; }
            foreach (var s in results) s.Display();
        }

        public void UpdateStudent(int id, string name, string dept, double gpa, int attendance, Grade grade)
        {
            var s = repo.FindById(id);
            if (s == null) { Console.WriteLine("\n  Student not found."); return; }
            s.Name = name;
            s.Department = dept;
            s.GPA = gpa;
            s.Attendance = attendance;
            s.LetterGrade = grade;
            repo.Update(s);
            Console.WriteLine("\n  Student updated successfully.");
        }

        public void DeleteStudent(int id)
        {
            if (repo.Delete(id))
                Console.WriteLine("\n  Student deleted successfully.");
            else
                Console.WriteLine("\n  Student not found.");
        }

        public void ShowAtRisk()
        {
            var list = repo.GetAtRisk();
            if (list.Count == 0) { Console.WriteLine("\n  No at-risk students."); return; }
            Console.WriteLine($"\n  At-Risk Students ({list.Count}):");
            foreach (var s in list) s.Display();
        }

        public void ShowStatistics()
        {
            Console.WriteLine("\n  ── Statistics ─────────────────────────");
            Console.WriteLine($"  Total Students    : {repo.GetTotalCount()}");
            Console.WriteLine($"  Average GPA       : {repo.GetAverageGPA():F2}");
            Console.WriteLine($"  Average Attendance: {repo.GetAverageAttendance():F1}%");
            Console.WriteLine($"  At-Risk Count     : {repo.GetAtRisk().Count}");
        }
    }

    class InputHelper
    {
        public static string ReadString(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                string value = Console.ReadLine()?.Trim() ?? "";
                if (allowEmpty || !string.IsNullOrWhiteSpace(value)) return value;
                Console.WriteLine("  This field cannot be empty. Please try again.");
            }
        }

        public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                if (int.TryParse(Console.ReadLine(), out int val) && val >= min && val <= max)
                    return val;
                Console.WriteLine($"  Invalid input. Please enter a number between {min} and {max}.");
            }
        }

        public static double ReadDouble(string prompt, double min = double.MinValue, double max = double.MaxValue)
        {
            while (true)
            {
                Console.Write($"  {prompt}: ");
                if (double.TryParse(Console.ReadLine(), out double val) && val >= min && val <= max)
                    return val;
                Console.WriteLine($"  Invalid input. Please enter a value between {min} and {max}.");
            }
        }

        public static Grade ReadGrade(string prompt)
        {
            while (true)
            {
                Console.Write($"  {prompt} (A/B/C/D/F): ");
                string input = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (Enum.TryParse(input, out Grade g)) return g;
                Console.WriteLine("  Invalid grade. Please enter A, B, C, D, or F.");
            }
        }
    }

    class Program
    {
        static readonly StudentService service = new StudentService();

        static void Main()
        {
            Console.Clear();
            PrintBanner();

            bool running = true;
            while (running)
            {
                PrintMenu();
                string choice = Console.ReadLine()?.Trim() ?? "";
                Console.WriteLine();

                switch (choice)
                {
                    case "1": HandleAdd(); break;
                    case "2": service.DisplayAll(); break;
                    case "3": HandleSearchName(); break;
                    case "4": HandleSearchDept(); break;
                    case "5": HandleUpdate(); break;
                    case "6": HandleDelete(); break;
                    case "7": service.ShowAtRisk(); break;
                    case "8": service.ShowStatistics(); break;
                    case "0": running = false; Console.WriteLine("  Goodbye!"); break;
                    default: Console.WriteLine("  Invalid option. Please try again."); break;
                }

                if (running)
                {
                    Console.Write("\n  Press Enter to continue...");
                    Console.ReadLine();
                    Console.Clear();
                    PrintBanner();
                }
            }
        }

        static void PrintBanner()
        {
            Console.WriteLine("  ╔══════════════════════════════════════════╗");
            Console.WriteLine("  ║       Student Management System          ║");
            Console.WriteLine("  ║       University of Sialkot (USKT)       ║");
            Console.WriteLine("  ╚══════════════════════════════════════════╝");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n  ── Main Menu ──────────────────────────────");
            Console.WriteLine("  [1]  Add New Student");
            Console.WriteLine("  [2]  View All Students");
            Console.WriteLine("  [3]  Search by Name");
            Console.WriteLine("  [4]  Search by Department");
            Console.WriteLine("  [5]  Update Student");
            Console.WriteLine("  [6]  Delete Student");
            Console.WriteLine("  [7]  View At-Risk Students");
            Console.WriteLine("  [8]  Statistics");
            Console.WriteLine("  [0]  Exit");
            Console.Write("\n  Enter choice: ");
        }

        static void HandleAdd()
        {
            Console.WriteLine("  ── Add New Student ────────────────────────");
            try
            {
                string name = InputHelper.ReadString("Full Name");
                string roll = InputHelper.ReadString("Roll Number");
                string dept = InputHelper.ReadString("Department");
                double gpa = InputHelper.ReadDouble("GPA (0.0 - 4.0)", 0.0, 4.0);
                int att = InputHelper.ReadInt("Attendance % (0-100)", 0, 100);
                Grade grade = InputHelper.ReadGrade("Letter Grade");
                service.AddStudent(name, roll, dept, gpa, att, grade);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        static void HandleSearchName()
        {
            string name = InputHelper.ReadString("Enter name to search");
            service.SearchByName(name);
        }

        static void HandleSearchDept()
        {
            string dept = InputHelper.ReadString("Enter department name");
            service.SearchByDepartment(dept);
        }

        static void HandleUpdate()
        {
            Console.WriteLine("  ── Update Student ─────────────────────────");
            int id = InputHelper.ReadInt("Enter Student ID", 1);
            string name = InputHelper.ReadString("New Name");
            string dept = InputHelper.ReadString("New Department");
            double gpa = InputHelper.ReadDouble("New GPA (0.0 - 4.0)", 0.0, 4.0);
            int att = InputHelper.ReadInt("New Attendance % (0-100)", 0, 100);
            Grade grade = InputHelper.ReadGrade("New Letter Grade");
            service.UpdateStudent(id, name, dept, gpa, att, grade);
        }

        static void HandleDelete()
        {
            int id = InputHelper.ReadInt("Enter Student ID to delete", 1);
            Console.Write("  Are you sure? (yes/no): ");
            if (Console.ReadLine()?.Trim().ToLower() == "yes")
                service.DeleteStudent(id);
            else
                Console.WriteLine("\n  Deletion cancelled.");
        }
    }
}
