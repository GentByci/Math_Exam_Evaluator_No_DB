using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MathTestSystem.UI.Services;

namespace MathTestSystem.UI
{
    public partial class MainWindow : Window
    {
        private DatabaseService dbService;
        private List<TestResult> allResults;
        private string currentTeacherId;

        public MainWindow(string teacherId)
        {
            InitializeComponent();
            dbService = new DatabaseService();
            currentTeacherId = teacherId;

            // Update window title with teacher ID
            this.Title = $"Math Test System - Teacher View (ID: {teacherId})";

            // Load data from database on startup if available
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            if (dbService.HasData())
            {
                allResults = dbService.GetAllResults();
                DisplayGroupedResults();
            }
        }

        private void LoadXML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Select Test Results XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Load XML and save to database
                    var result = dbService.LoadXMLToDatabase(openFileDialog.FileName);
                    int newStudents = result.newStudents;
                    int newExams = result.newExams;
                    int newTasks = result.newTasks;

                    // Show what was added
                    string message = $"XML loaded successfully!\n\n" +
                                   $"New Students: {newStudents}\n" +
                                   $"New Exams: {newExams}\n" +
                                   $"New Tasks: {newTasks}";

                    if (newExams == 0 && newTasks == 0)
                    {
                        message += "\n\nNote: All exams in this file were already loaded previously.";
                    }

                    MessageBox.Show(message, "XML Loaded", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload from database to show updated data
                    LoadFromDatabase();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading XML: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenStudentView_Click(object sender, RoutedEventArgs e)
        {
            StudentView studentView = new StudentView();
            studentView.Show();
        }

        private void ClearDatabase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all data from the database?\n\nThis action cannot be undone.",
                "Confirm Clear Database",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                dbService.ClearDatabase();
                StudentResultsPanel.Children.Clear();
                OverallSummaryText.Text = "Database cleared. Load an XML file to begin.";

                MessageBox.Show("Database cleared successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void DisplayGroupedResults()
        {
            StudentResultsPanel.Children.Clear();

            if (allResults == null || !allResults.Any())
            {
                OverallSummaryText.Text = "No data available. Load an XML file to begin.";
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
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 15),
                Padding = new Thickness(10),
                Background = Brushes.White
            };

            var stackPanel = new StackPanel();

            // Student header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10),
                Background = new SolidColorBrush(Color.FromRgb(240, 240, 240))
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
                Foreground = percentage >= 70 ? Brushes.Green : Brushes.Red,
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
}