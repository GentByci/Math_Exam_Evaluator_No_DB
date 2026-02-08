using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MathTestSystem.UI.Data;
using MathTestSystem.UI.Services;

namespace MathTestSystem.UI
{
    public partial class LoginWindow : Window
    {
        private DatabaseService dbService;

        public LoginWindow()
        {
            InitializeComponent();
            dbService = new DatabaseService();
        }

        private void TeacherLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptTeacherLogin();
        }

        private void TeacherIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptTeacherLogin();
            }
        }

        private void AttemptTeacherLogin()
        {
            string teacherId = TeacherIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(teacherId))
            {
                MessageBox.Show("Please enter a Teacher ID.",
                                "Missing Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            using (var context = new MathTestContext())
            {
                bool hasData = context.Teachers.Any();

                if (!hasData)
                {
                    MessageBox.Show($"Welcome, Teacher {teacherId}!\n\nNo data found in database.\nPlease load exam data to get started.",
                                    "Welcome - First Time Setup",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    OpenTeacherView(teacherId);
                    return;
                }

                var teacher = context.Teachers
                    .FirstOrDefault(t => t.TeacherExternalId == teacherId);

                if (teacher == null)
                {
                    var availableTeachers = context.Teachers
                        .Select(t => t.TeacherExternalId)
                        .ToList();

                    MessageBox.Show($"Teacher ID '{teacherId}' not found in the system.\n\n" +
                                    $"Available Teacher ID(s):\n{string.Join(", ", availableTeachers)}\n\n" +
                                    $"Please check your ID or contact your administrator.",
                                    "Teacher Not Found",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                OpenTeacherView(teacherId);
            }
        }

        private void OpenTeacherView(string teacherId)
        {
            MainWindow mainWindow = new MainWindow(teacherId);
            mainWindow.Show();
            this.Close();
        }

        private void StudentLogin_Click(object sender, RoutedEventArgs e)
        {
            AttemptStudentLogin();
        }

        private void StudentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptStudentLogin();
            }
        }

        private void AttemptStudentLogin()
        {
            string studentId = StudentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentId))
            {
                MessageBox.Show("Please enter a Student ID.",
                                "Missing Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            using (var context = new MathTestContext())
            {
                bool hasData = context.Students.Any();

                if (!hasData)
                {
                    MessageBox.Show("No student data found in the system.\n\n" +
                                    "Please contact your teacher to load exam data.",
                                    "No Data Available",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                var student = context.Students
                    .FirstOrDefault(s => s.StudentExternalId == studentId);

                if (student == null)
                {
                    var availableStudents = context.Students
                        .Select(s => s.StudentExternalId)
                        .OrderBy(id => id)
                        .Take(10)
                        .ToList();

                    string studentList = string.Join(", ", availableStudents);
                    if (context.Students.Count() > 10)
                    {
                        studentList += $", ... ({context.Students.Count() - 10} more)";
                    }

                    MessageBox.Show($"Student ID '{studentId}' not found in the system.\n\n" +
                                    $"Example Student IDs:\n{studentList}\n\n" +
                                    $"Please check your ID or contact your teacher.",
                                    "Student Not Found",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                OpenStudentView(studentId);
            }
        }

        private void OpenStudentView(string studentId)
        {
            StudentView studentView = new StudentView(studentId);
            studentView.Show();
            this.Close();
        }
    }
}