using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace ListaUczniow
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<string> classNames = new ObservableCollection<string>(); 
        private Dictionary<string, List<Student>> studentsByClass = new Dictionary<string, List<Student>>();
        private string filePath;

        public MainPage()
        {
            InitializeComponent();

            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "classes.txt");

            LoadClassesAndStudents();

            
            ClassPicker.ItemsSource = classNames;
        }

        private void ClassPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedClass = ClassPicker.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedClass))
            {
                Navigation.PushAsync(new StudentsPage(selectedClass, studentsByClass));
            }
        }

        private void AddClass(string className)
        {
            if (!string.IsNullOrWhiteSpace(className) && !studentsByClass.ContainsKey(className))
            {
                studentsByClass.Add(className, new List<Student>());
                classNames.Add(className); 

                SaveClassesAndStudents();
            }
            else
            {
                DisplayAlert("Blad", "Nazwa klasy jest pusta lub juz istnieje", "OK");
            }
        }

        private void RemoveClass(string className)
        {
            if (!string.IsNullOrWhiteSpace(className) && studentsByClass.ContainsKey(className))
            {
                studentsByClass.Remove(className);
                classNames.Remove(className); 

                SaveClassesAndStudents();
            }
            else
            {
                DisplayAlert("Blad", "Nazwa klasy jest pusta lub juz istnieje", "OK");
            }
        }

        private void SaveClassesAndStudents()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var classEntry in studentsByClass)
                    {
                        writer.WriteLine(classEntry.Key);
                        foreach (var student in classEntry.Value)
                        {
                            writer.WriteLine($"{student.Id},{student.Name}");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Blad zapisu ucznia i klasy: " + ex.Message);
            }
        }

        private void LoadClassesAndStudents()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string className;
                        while ((className = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(className))
                            {
                                List<Student> students = new List<Student>();
                                string studentLine;
                                while (!string.IsNullOrWhiteSpace(studentLine = reader.ReadLine()))
                                {
                                    string[] studentData = studentLine.Split(',');
                                    if (studentData.Length == 2 && int.TryParse(studentData[0], out int id))
                                    {
                                        students.Add(new Student(id, studentData[1]));
                                    }
                                }
                                studentsByClass.Add(className, students);
                                classNames.Add(className); 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Blad wczytania ucznia i klasy: " + ex.Message);
            }
        }

        private void AddClassButton_Clicked(object sender, EventArgs e)
        {
            AddClass(ClassEntry.Text);
        }

        private void RemoveClassButton_Clicked(object sender, EventArgs e)
        {
            RemoveClass(ClassEntry.Text);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ClassPicker.SelectedItem = null;
        }
    }

}
