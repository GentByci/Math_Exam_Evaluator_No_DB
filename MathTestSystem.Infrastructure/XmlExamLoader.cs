
using MathTestSystem.Domain;
using MathTestSystem.MathProcessor;
using System.Xml.Linq;


public class XmlExamLoader
{
    private readonly TaskParser _parser = new();

    public Teacher Load(string filePath)
    {
        var doc = XDocument.Load(filePath);

        var teacher = new Teacher
        {
            Id = doc.Root.Attribute("ID").Value
        };

        foreach (var studentNode in doc.Descendants("Student"))
        {
            var student = new Student
            {
                Id = studentNode.Attribute("ID").Value
            };

            foreach (var examNode in studentNode.Descendants("Exam"))
            {
                var exam = new Exam
                {
                    Id = examNode.Attribute("Id").Value
                };

                foreach (var taskNode in examNode.Descendants("Task"))
                {
                    var task = _parser.Parse(
                        taskNode.Value,
                        taskNode.Attribute("id").Value
                    );

                    exam.Tasks.Add(task);
                }

                student.Exams.Add(exam);
            }

            teacher.Students.Add(student);
        }

        return teacher;
    }
}
