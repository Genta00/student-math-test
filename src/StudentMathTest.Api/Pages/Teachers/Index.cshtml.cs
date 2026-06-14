using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Api.Pages.Teachers;

public class IndexModel(ITeacherService teacherService) : PageModel
{
    public IEnumerable<Teacher> Teachers { get; set; } = [];

    [BindProperty]
    public string NewTeacherName { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync() => Teachers = await teacherService.GetAllAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTeacherName))
        {
            ErrorMessage = "Teacher name is required.";
            Teachers = await teacherService.GetAllAsync();
            return Page();
        }
        await teacherService.CreateAsync(NewTeacherName.Trim());
        return RedirectToPage();
    }
}
