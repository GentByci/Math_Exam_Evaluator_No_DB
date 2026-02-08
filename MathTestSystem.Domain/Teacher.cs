using System;
using System.Collections.Generic;
using System.Text;

namespace MathTestSystem.Domain
{
    public class Teacher
    {
        public string Id { get; set; }

        public List<Student> Students { get; set; } = new();
    }
}