using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MathTestSystem.UI.Services;

namespace MathTestSystem.UI
{
    public partial class StudentView : Window
    {
        private DatabaseService dbService;
        private List<StudentTaskResult> studentTasks;
        private bool isLoggedIn;

        // Constructor when opened from login (with student ID)
        public StudentView(string studentId)
        {
            InitializeComponent();
            dbService = new DatabaseService();
            isLoggedIn = true;
            
            // Update window title
            this.Title = $"Student Results - {studentId}";
            
            // Hide the input section and show logout button
            InputSection.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            
            // Auto-load results
            StudentIdTextBox.Text = studentId;
            LoadStudentResults();
        }

        // Constructor when opened from teacher view (without student ID)
        public StudentView()
        {
            InitializeComponent();
            dbService = new DatabaseService();
            isLoggedIn = false;
            
            // Show input section, hide logout button
            InputSection.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Collapsed;
            
            // Check if database has data
            if (!dbService.HasData())
            {
                NoResultsText.Text = "No exam data available in database.\n\nPlease ask your teacher to load exam results first.";
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

        private void LoadStudentResults()
        {
            string studentId = StudentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Please enter a Student ID.", "Missing Information",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dbService.HasData())
            {
                MessageBox.Show("No exam data available in database.\n\nPlease ask your teacher to load exam results first.",
                                "No Data",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                studentTasks = dbService.GetStudentResults(studentId);
                DisplayStudentResults(studentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading results: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayStudentResults(string studentId)
        {
            if (studentTasks == null || !studentTasks.Any())
            {
                NoResultsText.Text = $"No results found for Student ID: {studentId}\n\n" +
                                   $"Available Student IDs:\n" +
                                   string.Join(", ", dbService.GetAllStudentIds());
                NoResultsText.Visibility = Visibility.Visible;
                ResultsDataGrid.Visibility = Visibility.Collapsed;
                ScoreSummaryBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Calculate statistics
            int totalTasks = studentTasks.Count;
            int correctCount = studentTasks.Count(t => t.IsCorrect);
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
            ResultsDataGrid.ItemsSource = studentTasks;
            ResultsDataGrid.Visibility = Visibility.Visible;
            ScoreSummaryBorder.Visibility = Visibility.Visible;
            NoResultsText.Visibility = Visibility.Collapsed;
        }
    }
}
