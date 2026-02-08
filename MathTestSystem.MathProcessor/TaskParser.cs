using MathTestSystem.Domain;
namespace MathTestSystem.MathProcessor;

public class TaskParser
{
    public TaskItem Parse(string rawTask, string taskId)
    {
        var parts = rawTask.Split('=');

        return new TaskItem
        {
            Id = taskId,
            Expression = parts[0].Trim(),
            StudentAnswer = double.Parse(parts[1].Trim())
        };
    }
}
