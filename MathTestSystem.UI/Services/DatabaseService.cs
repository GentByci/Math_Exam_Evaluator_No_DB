using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using MathTestSystem.UI.Data;
using MathTestSystem.UI.Models;

namespace MathTestSystem.UI.Services
{
    public class DatabaseService
    {
        public DatabaseService()
        {
            // Ensure database is created
            using (var context = new MathTestContext())
            {
                context.Database.EnsureCreated();
            }
        }

        /// <summary>
        /// Load XML file and save to database. Only adds new exams that don't exist.
        /// </summary>
        public (int newStudents, int newExams, int newTasks) LoadXMLToDatabase(string xmlFilePath)
        {
            int newStudentsCount = 0;
            int newExamsCount = 0;
            int newTasksCount = 0;

            using (var context = new MathTestContext())
            {
                XDocument doc = XDocument.Load(xmlFilePath);
                var teacherElement = doc.Element("Teacher");
                string teacherExternalId = teacherElement?.Attribute("ID")?.Value;

                if (string.IsNullOrEmpty(teacherExternalId))
                    throw new Exception("Teacher ID not found in XML.");

                // Get or create teacher
                var teacher = context.Teachers
                    .FirstOrDefault(t => t.TeacherExternalId == teacherExternalId);

                if (teacher == null)
                {
                    teacher = new Teacher { TeacherExternalId = teacherExternalId };
                    context.Teachers.Add(teacher);
                    context.SaveChanges();
                }

                var studentsElement = teacherElement?.Element("Students")?.Elements("Student");
                if (studentsElement == null)
                    throw new Exception("No students found in XML.");

                foreach (var studentElement in studentsElement)
                {
                    string studentExternalId = studentElement.Attribute("ID")?.Value;
                    if (string.IsNullOrEmpty(studentExternalId))
                        continue;

                    // Get or create student
                    var student = context.Students
                        .Include(s => s.Exams)
                        .FirstOrDefault(s => s.StudentExternalId == studentExternalId && s.TeacherId == teacher.TeacherId);

                    if (student == null)
                    {
                        student = new Student
                        {
                            StudentExternalId = studentExternalId,
                            TeacherId = teacher.TeacherId
                        };
                        context.Students.Add(student);
                        context.SaveChanges();
                        newStudentsCount++;
                    }

                    // Process exams
                    var examElements = studentElement.Elements("Exam");
                    foreach (var examElement in examElements)
                    {
                        string examExternalId = examElement.Attribute("Id")?.Value;
                        if (string.IsNullOrEmpty(examExternalId))
                            continue;

                        // Check if exam already exists for this student
                        var existingExam = context.Exams
                            .FirstOrDefault(e => e.StudentId == student.StudentId && e.ExamExternalId == examExternalId);

                        if (existingExam != null)
                        {
                            // Exam already loaded, skip
                            continue;
                        }

                        // Create new exam
                        var exam = new Exam
                        {
                            ExamExternalId = examExternalId,
                            StudentId = student.StudentId,
                            LoadedDate = DateTime.Now
                        };
                        context.Exams.Add(exam);
                        context.SaveChanges();
                        newExamsCount++;

                        // Process tasks
                        var taskElements = examElement.Elements("Task");
                        foreach (var taskElement in taskElements)
                        {
                            string taskExternalId = taskElement.Attribute("id")?.Value;
                            string taskContent = taskElement.Value.Trim();

                            var parts = taskContent.Split('=');
                            if (parts.Length != 2)
                                continue;

                            string expression = parts[0].Trim();
                            string studentAnswer = parts[1].Trim();
                            string correctAnswer = EvaluateExpression(expression);
                            bool isCorrect = studentAnswer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase);

                            var task = new Models.Task
                            {
                                TaskExternalId = taskExternalId,
                                Expression = expression,
                                StudentAnswer = studentAnswer,
                                CorrectAnswer = correctAnswer,
                                IsCorrect = isCorrect,
                                ExamId = exam.ExamId
                            };
                            context.Tasks.Add(task);
                            newTasksCount++;
                        }

                        context.SaveChanges();
                    }
                }
            }

            return (newStudentsCount, newExamsCount, newTasksCount);
        }

        /// <summary>
        /// Get all results for teacher view grouped by student
        /// </summary>
        public List<TestResult> GetAllResults()
        {
            var results = new List<TestResult>();

            using (var context = new MathTestContext())
            {
                var tasks = context.Tasks
                    .Include(t => t.Exam)
                    .ThenInclude(e => e.Student)
                    .OrderBy(t => t.Exam.Student.StudentExternalId)
                    .ThenBy(t => t.Exam.ExamExternalId)
                    .ThenBy(t => t.TaskExternalId)
                    .ToList();

                foreach (var task in tasks)
                {
                    results.Add(new TestResult
                    {
                        Id = task.Exam.Student.StudentExternalId,
                        Expression = task.Expression,
                        StudentAnswer = task.StudentAnswer,
                        CorrectAnswer = task.CorrectAnswer,
                        IsCorrect = task.IsCorrect
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Get results for a specific student
        /// </summary>
        public List<StudentTaskResult> GetStudentResults(string studentExternalId)
        {
            var results = new List<StudentTaskResult>();

            using (var context = new MathTestContext())
            {
                var student = context.Students
                    .FirstOrDefault(s => s.StudentExternalId == studentExternalId);

                if (student == null)
                    return results;

                var tasks = context.Tasks
                    .Include(t => t.Exam)
                    .Where(t => t.Exam.StudentId == student.StudentId)
                    .OrderBy(t => t.Exam.ExamExternalId)
                    .ThenBy(t => t.TaskExternalId)
                    .ToList();

                int taskNumber = 1;
                foreach (var task in tasks)
                {
                    results.Add(new StudentTaskResult
                    {
                        TaskNumber = taskNumber++,
                        Expression = task.Expression,
                        StudentAnswer = task.StudentAnswer,
                        CorrectAnswer = task.CorrectAnswer,
                        IsCorrect = task.IsCorrect
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Check if any data exists in the database
        /// </summary>
        public bool HasData()
        {
            using (var context = new MathTestContext())
            {
                return context.Students.Any();
            }
        }

        /// <summary>
        /// Get list of all student IDs in the database
        /// </summary>
        public List<string> GetAllStudentIds()
        {
            using (var context = new MathTestContext())
            {
                return context.Students
                    .Select(s => s.StudentExternalId)
                    .OrderBy(id => id)
                    .ToList();
            }
        }

        /// <summary>
        /// Clear all data from the database
        /// </summary>
        public void ClearDatabase()
        {
            using (var context = new MathTestContext())
            {
                context.Teachers.RemoveRange(context.Teachers);
                context.SaveChanges();
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
    }

    // Result classes (keep existing ones from previous code)
    public class TestResult
    {
        public string Id { get; set; }
        public string Expression { get; set; }
        public string StudentAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class StudentTaskResult
    {
        public int TaskNumber { get; set; }
        public string Expression { get; set; }
        public string StudentAnswer { get; set; }
        public string CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }

        public string ResultText => IsCorrect ? "✓ Correct" : "✗ Wrong";

        public System.Windows.Media.Brush ResultColor => IsCorrect
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));
    }
}