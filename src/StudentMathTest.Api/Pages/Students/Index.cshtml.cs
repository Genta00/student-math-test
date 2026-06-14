using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentMathTest.Api.Pages.Students;

public class IndexModel : PageModel
{
    [BindProperty]
    public int StudentId { get; set; }

    public void OnGet() { }

    public IActionResult OnPost() =>
        StudentId <= 0 ? Page() : RedirectToPage("/Students/Exams", new { studentId = StudentId });
}
