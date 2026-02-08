using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathTestSystem.UI.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public string TeacherExternalId { get; set; } // The ID from XML (e.g., "11111")

        public virtual ICollection<Student> Students { get; set; }
    }

    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public string StudentExternalId { get; set; } // The ID from XML (e.g., "12345")

        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }

        public virtual ICollection<Exam> Exams { get; set; }
    }

    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required]
        public string ExamExternalId { get; set; } // The ID from XML (e.g., "1")

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        public DateTime LoadedDate { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }

    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string TaskExternalId { get; set; } // The ID from XML (e.g., "1")

        [Required]
        public string Expression { get; set; }

        [Required]
        public string StudentAnswer { get; set; }

        [Required]
        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public int ExamId { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }
    }
}