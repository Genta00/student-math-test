namespace StudentMathTest.Domain.Models;

public sealed class TaskResult
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public string XmlTaskId { get; set; } = string.Empty;
    public string RawExpression { get; set; } = string.Empty;
    public decimal StudentAnswer { get; set; }
    public decimal ComputedResult { get; set; }
    public bool IsCorrect { get; set; }
}
