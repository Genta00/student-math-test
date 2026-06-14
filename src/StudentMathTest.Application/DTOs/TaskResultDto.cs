namespace StudentMathTest.Application.DTOs;

public record TaskResultDto(
    string TaskId,
    string Expression,
    decimal StudentAnswer,
    decimal CorrectAnswer,
    bool IsCorrect
);
