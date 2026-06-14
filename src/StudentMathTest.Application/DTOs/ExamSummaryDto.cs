namespace StudentMathTest.Application.DTOs;

public record ExamSummaryDto(
    int ExamId,
    string XmlExamId,
    int StudentId,
    DateTime SubmittedAt,
    int TotalTasks,
    int CorrectTasks,
    double Score
);
