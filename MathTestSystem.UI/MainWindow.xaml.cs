using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace MathTestSystem.UI
{
    public partial class MainWindow : Window
    {
        private List<TestResult> allResults;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenStudentView_Click(object sender, RoutedEventArgs e)
        {
            StudentView studentView = new StudentView();
            studentView.Show();
        }

        private void LoadXML_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select Test Results XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Load and parse XML
                    LoadXMLFile(openFileDialog.FileName);

                    // Display grouped results
                    DisplayGroupedResults();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading XML: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadXMLFile(string filePath)
        {
            allResults = new List<TestResult>();

            XDocument doc = XDocument.Load(filePath);

            // Navigate through the XML structure
            var teacher = doc.Element("Teacher");
            var students = teacher?.Element("Students")?.Elements("Student");

            if (students == null)
            {
                MessageBox.Show("No student data found in XML.", "Warning",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var student in students)
            {
                string studentId = student.Attribute("ID")?.Value ?? "Unknown";

                var exams = student.Elements("Exam");

                foreach (var exam in exams)
                {
                    var tasks = exam.Elements("Task");

                    foreach (var task in tasks)
                    {
                        string taskContent = task.Value.Trim();

                        // Parse "2+3/6-4 = 74" format
                        var parts = taskContent.Split('=');

                        if (parts.Length == 2)
                        {
                            string expression = parts[0].Trim();
                            string studentAnswer = parts[1].Trim();

                            // Calculate correct answer
                            string correctAnswer = EvaluateExpression(expression);

                            // Check if correct
                            bool isCorrect = studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);

                            allResults.Add(new TestResult
                            {
                                Id = studentId,
                                Expression = expression,
                                StudentAnswer = studentAnswer,
                                CorrectAnswer = correctAnswer,
                                IsCorrect = isCorrect
                            });
                        }
                    }
                }
            }
        }

        private string EvaluateExpression(string expression)
        {
            try
            {
                // Use DataTable to compute the expression
                var table = new DataTable();
                var result = table.Compute(expression, string.Empty);

                // Format the result
                if (result is double d)
                {
                    // Remove unnecessary decimals
                    return d % 1 == 0 ? ((int)d).ToString() : d.ToString("G");
                }

                return result.ToString();
            }
            catch
            {
                return "Error";
            }
        }

        private void DisplayGroupedResults()
        {
            StudentResultsPanel.Children.Clear();

            if (allResults == null || !allResults.Any())
            {
                MessageBox.Show("No results to display.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Group by Student ID
            var groupedByStudent = allResults.GroupBy(r => r.Id)
                                             .OrderBy(g => g.Key);

            int totalQuestions = 0;
            int totalCorrect = 0;

            foreach (var studentGroup in groupedByStudent)
            {
                string studentId = studentGroup.Key;
                var studentResults = studentGroup.ToList();
                int correctCount = studentResults.Count(r => r.IsCorrect);
                int totalCount = studentResults.Count;

                totalQuestions += totalCount;
                totalCorrect += correctCount;

                // Create student section
                var studentSection = CreateStudentSection(studentId, studentResults, correctCount, totalCount);
                StudentResultsPanel.Children.Add(studentSection);
            }

            // Update overall summary
            double overallPercentage = totalQuestions > 0 ? (double)totalCorrect / totalQuestions * 100 : 0;
            OverallSummaryText.Text = $"Total Students: {groupedByStudent.Count()} | " +
                                     $"Total Questions: {totalQuestions} | " +
                                     $"Total Correct: {totalCorrect} | " +
                                     $"Overall Accuracy: {overallPercentage:F1}%";
        }

        private Border CreateStudentSection(string studentId, List<TestResult> results, int correctCount, int totalCount)
        {
            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(10),
                Background = System.Windows.Media.Brushes.White
            };

            var stackPanel = new StackPanel();

            // Student header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240))
            };
            headerPanel.Height = 40;

            var studentIdText = new TextBlock
            {
                Text = $"Student ID: {studentId}",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(10, 10, 20, 10),
                VerticalAlignment = VerticalAlignment.Center
            };

            double percentage = totalCount > 0 ? (double)correctCount / totalCount * 100 : 0;
            var scoreText = new TextBlock
            {
                Text = $"Score: {correctCount}/{totalCount} ({percentage:F1}%)",
                FontSize = 14,
                Foreground = percentage >= 70 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 10, 10, 10)
            };

            headerPanel.Children.Add(studentIdText);
            headerPanel.Children.Add(scoreText);
            stackPanel.Children.Add(headerPanel);

            // DataGrid for this student
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                ItemsSource = results,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                CanUserResizeColumns = true,
                CanUserSortColumns = false,
                RowHeight = 30
            };

            // Define columns
            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Expression",
                Binding = new System.Windows.Data.Binding("Expression"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Student Answer",
                Binding = new System.Windows.Data.Binding("StudentAnswer"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Correct Answer",
                Binding = new System.Windows.Data.Binding("CorrectAnswer"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            dataGrid.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = "Is Correct",
                Binding = new System.Windows.Data.Binding("IsCorrect"),
                Width = new DataGridLength(100)
            });

            stackPanel.Children.Add(dataGrid);
            border.Child = stackPanel;

            return border;
        }
    }

    // TestResult class
    public class TestResult
    {
        public string Id { get; set; }
        public string Expression { get; set; }
        public string StudentAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }
}