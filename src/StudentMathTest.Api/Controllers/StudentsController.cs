using Microsoft.AspNetCore.Mvc;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public class StudentsController(IExamService examService) : ControllerBase
{
    /// <summary>Returns all exams for a student with scores.</summary>
    [HttpGet("{studentId}/exams")]
    public async Task<IActionResult> GetExams(int studentId)
    {
        var exams = await examService.GetExamsByStudentAsync(studentId);
        return Ok(exams.Select(e => new ExamSummaryDto(
            e.Id, e.XmlExamId, e.StudentId, e.SubmittedAt,
            e.Tasks.Count, e.Tasks.Count(t => t.IsCorrect),
            e.Tasks.Count == 0 ? 0 : Math.Round((double)e.Tasks.Count(t => t.IsCorrect) / e.Tasks.Count * 100, 1)
        )));
    }

    /// <summary>Returns per-task results for a student's exam, including correct answers.</summary>
    [HttpGet("{studentId}/exams/{examId}")]
    public async Task<IActionResult> GetExamDetail(int studentId, int examId)
    {
        var exam = await examService.GetExamAsync(examId);
        if (exam is null || exam.StudentId != studentId)
            return NotFound(new { error = $"Exam {examId} not found for student {studentId}." });

        return Ok(new ExamResultDto(
            exam.Id, exam.XmlExamId, exam.StudentId, exam.TeacherId, exam.SubmittedAt,
            exam.Tasks.Count, exam.Tasks.Count(t => t.IsCorrect),
            exam.Tasks.Select(t => new TaskResultDto(t.XmlTaskId, t.RawExpression, t.StudentAnswer, t.ComputedResult, t.IsCorrect))
        ));
    }
}
