namespace StudentMathTest.Application.DTOs;

public record UploadResultDto(
    int TeacherId,
    int TotalExams,
    IEnumerable<ExamSummaryDto> Exams
);
