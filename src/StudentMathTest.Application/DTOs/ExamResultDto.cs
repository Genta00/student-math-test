namespace StudentMathTest.Application.DTOs;

public record ExamResultDto(
    int ExamId,
    string XmlExamId,
    int StudentId,
    int TeacherId,
    DateTime SubmittedAt,
    int TotalTasks,
    int CorrectTasks,
    IEnumerable<TaskResultDto> Tasks
);
