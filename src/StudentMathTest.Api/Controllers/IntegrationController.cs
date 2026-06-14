using Microsoft.AspNetCore.Mvc;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Controllers;

/// <summary>Stateless grading endpoint for third-party integrations. No data is persisted.</summary>
[ApiController]
[Route("api/integration")]
[Produces("application/json")]
public class IntegrationController(IXmlProcessor xmlProcessor, IMathEngine mathEngine) : ControllerBase
{
    /// <summary>Health check.</summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    /// <summary>Grades an uploaded XML file and returns results without saving to the database.</summary>
    [HttpPost("process")]
    [Consumes("multipart/form-data")]
    public IActionResult Process(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        using var reader = new StreamReader(file.OpenReadStream());
        return ProcessXml(reader.ReadToEnd());
    }

    /// <summary>Grades XML sent as a raw request body (text/xml) without saving to the database.</summary>
    [HttpPost("process/raw")]
    [Consumes("text/xml", "application/xml")]
    public async Task<IActionResult> ProcessRaw()
    {
        using var reader = new StreamReader(Request.Body);
        var xmlContent = await reader.ReadToEndAsync();
        return string.IsNullOrWhiteSpace(xmlContent)
            ? BadRequest(new { error = "Request body is empty." })
            : ProcessXml(xmlContent);
    }

    private IActionResult ProcessXml(string xmlContent)
    {
        try
        {
            var parsedExams = xmlProcessor.Parse(xmlContent).ToList();

            var gradedExams = parsedExams.Select(exam =>
            {
                var tasks = exam.Tasks.Select(task =>
                {
                    decimal computed = 0;
                    var isCorrect = false;
                    try
                    {
                        computed = mathEngine.Evaluate(task.RawExpression);
                        isCorrect = computed == task.StudentAnswer;
                    }
                    catch (Exception)
                    {
                        // malformed expression — task marked incorrect, processing continues
                    }
                    return new TaskResultDto(task.XmlTaskId, task.RawExpression, task.StudentAnswer, computed, isCorrect);
                }).ToList();

                return new
                {
                    exam.XmlExamId,
                    exam.StudentId,
                    exam.TeacherId,
                    TotalTasks = tasks.Count,
                    CorrectTasks = tasks.Count(t => t.IsCorrect),
                    Score = tasks.Count == 0 ? 0 : Math.Round((double)tasks.Count(t => t.IsCorrect) / tasks.Count * 100, 1),
                    Tasks = tasks
                };
            });

            return Ok(new { ProcessedAt = DateTime.UtcNow, TotalExams = parsedExams.Count, Exams = gradedExams });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
