namespace StudentMathTest.Domain.Models;

public sealed class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
