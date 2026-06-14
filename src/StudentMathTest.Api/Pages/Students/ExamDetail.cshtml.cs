using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Pages.Students;

public class ExamDetailModel(IExamService examService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int ExamId { get; set; }

    public ExamResultDto? Exam { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var exam = await examService.GetExamAsync(ExamId);
        if (exam is null) return NotFound();

        Exam = new ExamResultDto(
            exam.Id, exam.XmlExamId, exam.StudentId, exam.TeacherId, exam.SubmittedAt,
            exam.Tasks.Count, exam.Tasks.Count(t => t.IsCorrect),
            exam.Tasks.Select(t => new TaskResultDto(t.XmlTaskId, t.RawExpression, t.StudentAnswer, t.ComputedResult, t.IsCorrect))
        );
        return Page();
    }
}
