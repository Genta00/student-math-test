using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Pages.Teachers;

public class UploadModel(IExamService examService, ITeacherService teacherService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int TeacherId { get; set; }

    public string? TeacherName { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<ExamSummaryDto>? Results { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var teacher = await teacherService.GetByIdAsync(TeacherId);
        if (teacher is null && TeacherId != 0) return NotFound();
        TeacherName = teacher?.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? file)
    {
        var teacher = await teacherService.GetByIdAsync(TeacherId);
        TeacherName = teacher?.Name;

        if (file is null || file.Length == 0)
        {
            ErrorMessage = "Please select an XML file.";
            return Page();
        }

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
            file.ContentType is not ("text/xml" or "application/xml"))
        {
            ErrorMessage = "Only XML files are accepted.";
            return Page();
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var exams = await examService.ProcessXmlAsync(await reader.ReadToEndAsync());
            Results = exams.Select(e => new ExamSummaryDto(
                e.Id, e.XmlExamId, e.StudentId, e.SubmittedAt,
                e.Tasks.Count, e.Tasks.Count(t => t.IsCorrect),
                e.Tasks.Count == 0 ? 0 : Math.Round((double)e.Tasks.Count(t => t.IsCorrect) / e.Tasks.Count * 100, 1)
            )).ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Processing failed: {ex.Message}";
        }

        return Page();
    }
}
