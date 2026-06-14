namespace StudentMathTest.Domain.Models;

public sealed class Exam
{
    public int Id { get; set; }
    public string XmlExamId { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TaskResult> Tasks { get; set; } = new List<TaskResult>();
}
