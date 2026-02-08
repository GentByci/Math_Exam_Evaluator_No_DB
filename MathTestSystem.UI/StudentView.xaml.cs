using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Win32;

namespace MathTestSystem.UI
{
    public partial class StudentView : Window
    {
        private string loadedXmlFilePath;
        private List<StudentTaskResult> allTasks;

        public StudentView()
        {
            InitializeComponent();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select Test Results XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                loadedXmlFilePath = openFileDialog.FileName;
                FilePathText.Text = $"Loaded: {System.IO.Path.GetFileName(loadedXmlFilePath)}";
                FilePathText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));

                // If student ID is already entered, load results automatically
                if (!string.IsNullOrWhiteSpace(StudentIdTextBox.Text))
                {
                    LoadStudentResults();
                }
            }
        }

        private void LoadResults_Click(object sender, RoutedEventArgs e)
        {
            LoadStudentResults();
        }

        private void StudentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadStudentResults();
            }
        }

        private void LoadStudentResults()
        {
            string studentId = StudentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Please enter a Student ID.", "Missing Information",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(loadedXmlFilePath))
            {
                MessageBox.Show("Please load an XML file first.", "No File Loaded",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LoadXMLDataForStudent(studentId);
                DisplayStudentResults(studentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading results: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadXMLDataForStudent(string studentId)
        {
            allTasks = new List<StudentTaskResult>();

            XDocument doc = XDocument.Load(loadedXmlFilePath);

            var teacher = doc.Element("Teacher");
            var students = teacher?.Element("Students")?.Elements("Student");

            if (students == null)
            {
                throw new Exception("No student data found in XML file.");
            }

            // Find the specific student
            var student = students.FirstOrDefault(s => s.Attribute("ID")?.Value == studentId);

            if (student == null)
            {
                throw new Exception($"Student ID '{studentId}' not found in the XML file.");
            }

            var exams = student.Elements("Exam");
            int taskCounter = 1;

            foreach (var exam in exams)
            {
                var tasks = exam.Elements("Task");

                foreach (var task in tasks)
                {
                    string taskContent = task.Value.Trim();
                    var parts = taskContent.Split('=');

                    if (parts.Length == 2)
                    {
                        string expression = parts[0].Trim();
                        string studentAnswer = parts[1].Trim();
                        string correctAnswer = EvaluateExpression(expression);
                        bool isCorrect = studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);

                        allTasks.Add(new StudentTaskResult
                        {
                            TaskNumber = taskCounter++,
                            Expression = expression,
                            StudentAnswer = studentAnswer,
                            CorrectAnswer = correctAnswer,
                            IsCorrect = isCorrect
                        });
                    }
                }
            }
        }

        private string EvaluateExpression(string expression)
        {
            try
            {
                var table = new DataTable();
                var result = table.Compute(expression, string.Empty);

                if (result is double d)
                {
                    return d % 1 == 0 ? ((int)d).ToString() : d.ToString("G");
                }

                return result.ToString();
            }
            catch
            {
                return "Error";
            }
        }

        private void DisplayStudentResults(string studentId)
        {
            if (allTasks == null || !allTasks.Any())
            {
                NoResultsText.Text = $"No results found for Student ID: {studentId}";
                NoResultsText.Visibility = Visibility.Visible;
                ResultsDataGrid.Visibility = Visibility.Collapsed;
                ScoreSummaryBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate statistics
            int totalTasks = allTasks.Count;
            int correctCount = allTasks.Count(t => t.IsCorrect);
            double percentage = (double)correctCount / totalTasks * 100;

            // Update summary
            StudentInfoText.Text = $"Student ID: {studentId}";
            ScoreText.Text = $"Score: {correctCount} / {totalTasks}";
            PercentageText.Text = $"Percentage: {percentage:F1}%";

            // Color code the percentage
            if (percentage >= 90)
            {
                PercentageText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                ScoreSummaryBorder.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                ScoreSummaryBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else if (percentage >= 70)
            {
                PercentageText.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                ScoreSummaryBorder.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224));
                ScoreSummaryBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0));
            }
            else
            {
                PercentageText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                ScoreSummaryBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
                ScoreSummaryBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }

            // Show results
            ResultsDataGrid.ItemsSource = allTasks;
            ResultsDataGrid.Visibility = Visibility.Visible;
            ScoreSummaryBorder.Visibility = Visibility.Visible;
            NoResultsText.Visibility = Visibility.Collapsed;
        }
    }

    // Model class for student task results
    public class StudentTaskResult
    {
        public int TaskNumber { get; set; }
        public string Expression { get; set; }
        public string StudentAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }

        public string ResultText => IsCorrect ? "✓ Correct" : "✗ Wrong";

        public Brush ResultColor => IsCorrect
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))  // Green
            : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
    }
}