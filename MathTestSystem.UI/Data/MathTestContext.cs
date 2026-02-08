using Microsoft.EntityFrameworkCore;
using MathTestSystem.UI.Models;
using System.IO;

namespace MathTestSystem.UI.Data
{
    public class MathTestContext : DbContext
    {
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Database file will be created in the application directory
            string dbPath = Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "MathTestSystem.db"
            );

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Teacher>()
                .HasMany(t => t.Students)
                .WithOne(s => s.Teacher)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.Exams)
                .WithOne(e => e.Student)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Tasks)
                .WithOne(t => t.Exam)
                .HasForeignKey(t => t.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes for performance
            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.TeacherExternalId)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentExternalId);

            modelBuilder.Entity<Exam>()
                .HasIndex(e => new { e.StudentId, e.ExamExternalId });
        }
    }
}