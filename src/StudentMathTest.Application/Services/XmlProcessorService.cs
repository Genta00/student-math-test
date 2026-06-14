using System.Xml.Linq;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Application.Services;

/// <summary>
/// Parses teacher-submitted XML into Exam objects with ungraded tasks.
/// Root element must be Teacher with an ID attribute.
/// </summary>
public sealed class XmlProcessorService : IXmlProcessor
{
    public IEnumerable<Exam> Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var teacherEl = doc.Root ?? throw new InvalidOperationException("XML root element is missing.");

        var teacherId = int.Parse(teacherEl.Attribute("ID")?.Value
            ?? throw new InvalidOperationException("Teacher element is missing 'ID' attribute."));

        var exams = new List<Exam>();

        foreach (var studentEl in teacherEl.Descendants("Student"))
        {
            var studentId = int.Parse(studentEl.Attribute("ID")?.Value
                ?? throw new InvalidOperationException("Student element is missing 'ID' attribute."));

            foreach (var examEl in studentEl.Elements("Exam"))
            {
                var xmlExamId = examEl.Attribute("Id")?.Value
                    ?? throw new InvalidOperationException("Exam element is missing 'Id' attribute.");

                var exam = new Exam
                {
                    XmlExamId = xmlExamId,
                    StudentId = studentId,
                    TeacherId = teacherId,
                    SubmittedAt = DateTime.UtcNow
                };

                foreach (var taskEl in examEl.Elements("Task"))
                {
                    var taskId = taskEl.Attribute("id")?.Value
                        ?? throw new InvalidOperationException("Task element is missing 'id' attribute.");

                    var rawText = taskEl.Value.Trim();
                    var parts = rawText.Split('=', 2);
                    if (parts.Length != 2)
                        throw new InvalidOperationException($"Task '{taskId}' has invalid format. Expected 'expression = answer'.");

                    var expression = parts[0].Trim();
                    if (!decimal.TryParse(parts[1].Trim(),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var studentAnswer))
                        throw new InvalidOperationException($"Task '{taskId}' has non-numeric student answer.");

                    exam.Tasks.Add(new TaskResult
                    {
                        XmlTaskId = taskId,
                        RawExpression = expression,
                        StudentAnswer = studentAnswer
                    });
                }

                exams.Add(exam);
            }
        }

        return exams;
    }
}
