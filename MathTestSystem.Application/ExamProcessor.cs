using MathTestSystem.Domain;
using MathTestSystem.MathProcessor;

public class ExamProcessor
{
    private readonly MathEngine _engine = new();

    public void Process(Teacher teacher)
    {
        foreach (var student in teacher.Students)
        {
            foreach (var exam in student.Exams)
            {
                foreach (var task in exam.Tasks)
                {
                    task.CorrectAnswer =
                        _engine.Evaluate(task.Expression);

                    task.IsCorrect =
                        Math.Round(task.CorrectAnswer, 2)
                        ==
                        Math.Round(task.StudentAnswer, 2);
                }
            }
        }
    }
}
