using Microsoft.AspNetCore.Mvc;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Controllers;

[ApiController]
[Route("api/exams")]
[Produces("application/json")]
public class ExamsController(IExamService examService) : ControllerBase
{
    /// <summary>Uploads a teacher XML file, grades all tasks, and persists results.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        using var reader = new StreamReader(file.OpenReadStream());
        var xmlContent = await reader.ReadToEndAsync();

        try
        {
            var exams = await examService.ProcessXmlAsync(xmlContent);
            var summaries = exams.Select(e => new ExamSummaryDto(
                e.Id, e.XmlExamId, e.StudentId, e.SubmittedAt,
                e.Tasks.Count, e.Tasks.Count(t => t.IsCorrect),
                e.Tasks.Count == 0 ? 0 : Math.Round((double)e.Tasks.Count(t => t.IsCorrect) / e.Tasks.Count * 100, 1)
            ));
            return Ok(new UploadResultDto(exams.FirstOrDefault()?.TeacherId ?? 0, exams.Count(), summaries));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Returns full graded results for a single exam by its database ID.</summary>
    [HttpGet("{examId}")]
    public async Task<IActionResult> GetExam(int examId)
    {
        var exam = await examService.GetExamAsync(examId);
        if (exam is null)
            return NotFound(new { error = $"Exam {examId} not found." });

        return Ok(new ExamResultDto(
            exam.Id, exam.XmlExamId, exam.StudentId, exam.TeacherId, exam.SubmittedAt,
            exam.Tasks.Count, exam.Tasks.Count(t => t.IsCorrect),
            exam.Tasks.Select(t => new TaskResultDto(t.XmlTaskId, t.RawExpression, t.StudentAnswer, t.ComputedResult, t.IsCorrect))
        ));
    }
}
