using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ListaUczniow
{
    public partial class StudentsPage : ContentPage
    {
        private Dictionary<string, List<Student>> studentsByClass;
        public string ClassName { get; set; }
        private string filePath;
        private HashSet<int> luckyNumbers = new HashSet<int>();

        public StudentsPage(string className, Dictionary<string, List<Student>> studentsByClass)
        {
            InitializeComponent();
            ClassName = className;
            this.studentsByClass = studentsByClass;
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{className}.txt");

            LoadStudentsFromFile();

            StudentList.ItemsSource = this.studentsByClass[className];
        }

        private void LoadStudentsFromFile()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);
                    List<Student> students = new List<Student>();

                    foreach (var line in lines)
                    {
                        var studentData = line.Split(',');
                        if (studentData.Length == 2 && int.TryParse(studentData[0], out int id))
                        {
                            students.Add(new Student(id, studentData[1]));
                        }
                    }

                    if (students.Any())
                    {
                        this.studentsByClass[ClassName] = students;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("B��d wczytywania uczni�w z pliku: " + ex.Message);
            }
        }



        private void AddStudentButton_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(StudentEntry.Text))
            {
                var students = studentsByClass[ClassName];
                int id = students.Count + 1;
                students.Add(new Student(id, StudentEntry.Text));
                SaveStudents();
                StudentList.ItemsSource = null;
                StudentList.ItemsSource = students;
                StudentEntry.Text = string.Empty;
            }
        }

        private void RemoveStudentButton_Clicked(object sender, EventArgs e)
        {
            var student = (sender as Button)?.BindingContext as Student;
            if (student != null)
            {
                studentsByClass[ClassName].Remove(student);
                RemoveStudentFromTextFile(student);
                StudentList.ItemsSource = null;
                StudentList.ItemsSource = studentsByClass[ClassName];
            }
        }

        private void RemoveStudentFromTextFile(Student student)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();
                var studentLine = $"{student.Id},{student.Name}";
                lines.RemoveAll(line => line == studentLine);
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Blad zapisu ucznia do pliku: " + ex.Message);
            }
        }

        private void SaveStudents()
        {
            try
            {
                foreach (var classEntry in studentsByClass)
                {
                    string classFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{classEntry.Key}.txt");

                    using (StreamWriter writer = new StreamWriter(classFilePath))
                    {
                        writer.WriteLine(classEntry.Key); // Zapisz nazw� klasy

                        // Zapisz uczni�w dla danej klasy
                        foreach (var student in classEntry.Value)
                        {
                            writer.WriteLine($"{student.Id},{student.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("B��d zapisu uczni�w: " + ex.Message);
            }
        }

           

        private void StudentList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedStudent = e.SelectedItem as Student;
            DisplayAlert("Wybranie ucznia", $"Wybra�es: {selectedStudent.Name}", "OK");
            (sender as ListView).SelectedItem = null;
        }

        private void RandomStudentButton_Clicked(object sender, EventArgs e)
        {
            var students = studentsByClass[ClassName];
            if (students.Count > 0)
            {
                var random = new Random();
                var availableStudents = students.Where(student => !luckyNumbers.Contains(student.Id)).ToList();
                if (availableStudents.Count > 0)
                {
                    var randomIndex = random.Next(0, availableStudents.Count);
                    var randomStudent = availableStudents[randomIndex];
                    DisplayAlert("Odpowiedz", $"Ucze� wybrany do odpowiedzi: {randomStudent.Name} (Numer: {randomStudent.Id})", "OK");
                }

            }
            else
            {
                DisplayAlert("Nie ma uczni�w", "Nie ma uczni�w w klasie.", "OK");
            }
        }
        private async void EditStudentButton_Clicked(object sender, EventArgs e)
        {
            var student = (sender as Button)?.BindingContext as Student;
            if (student != null)
            {
                var newName = await DisplayPromptAsync("Edycja ucznia", "Wprowad� nowe imi� ucznia:", "Zapisz", "Anuluj", student.Name);
                if (newName != null)
                {
                    student.Name = newName;
                    SaveStudents();
                    StudentList.ItemsSource = null;
                    StudentList.ItemsSource = studentsByClass[ClassName];
                }
            }
        }

        

    }
}
