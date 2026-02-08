using System;
using System.Collections.Generic;
using System.Text;

namespace MathTestSystem.Domain
{
    public class TaskItem
    {
        public string Id { get; set; }

        public string Expression { get; set; }

        public double StudentAnswer { get; set; }

        public double CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }
    }
}