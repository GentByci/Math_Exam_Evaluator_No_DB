using System;
using System.Collections.Generic;
using System.Text;

namespace MathTestSystem.Domain
{
    public class Exam
    {
        public string Id { get; set; }

        public List<TaskItem> Tasks { get; set; } = new();
    }
}
