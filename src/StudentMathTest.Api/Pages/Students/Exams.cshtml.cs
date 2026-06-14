using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Pages.Students;

public class ExamsModel(IExamService examService) : PageModel
{
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int StudentId { get; set; }

    public IEnumerable<ExamSummaryDto> Exams { get; set; } = [];

    public async Task OnGetAsync()
    {
        var exams = await examService.GetExamsByStudentAsync(StudentId);
        Exams = exams.Select(e => new ExamSummaryDto(
            e.Id, e.XmlExamId, e.StudentId, e.SubmittedAt,
            e.Tasks.Count, e.Tasks.Count(t => t.IsCorrect),
            e.Tasks.Count == 0 ? 0 : Math.Round((double)e.Tasks.Count(t => t.IsCorrect) / e.Tasks.Count * 100, 1)
        ));
    }
}
