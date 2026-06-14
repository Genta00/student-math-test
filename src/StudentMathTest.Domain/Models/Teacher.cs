namespace StudentMathTest.Domain.Models;

public sealed class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
