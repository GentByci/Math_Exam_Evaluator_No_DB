using System;
using System.Collections.Generic;
using System.Text;

namespace MathTestSystem.Domain
{ 
    public class Student
    {
    public string Id { get; set; }

    public List<Exam> Exams { get; set; } = new();
    }
}