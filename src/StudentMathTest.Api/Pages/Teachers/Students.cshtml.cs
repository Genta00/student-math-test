using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Api.Pages.Teachers;

public class StudentsModel(ITeacherService teacherService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int TeacherId { get; set; }

    public Teacher? Teacher { get; set; }
    public IEnumerable<Student> Students { get; set; } = [];

    [BindProperty]
    public string NewStudentName { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Teacher = await teacherService.GetByIdAsync(TeacherId);
        if (Teacher is null) return NotFound();
        Students = await teacherService.GetStudentsAsync(TeacherId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(NewStudentName))
        {
            ErrorMessage = "Student name is required.";
            Teacher = await teacherService.GetByIdAsync(TeacherId);
            Students = await teacherService.GetStudentsAsync(TeacherId);
            return Page();
        }
        try
        {
            await teacherService.AddStudentAsync(TeacherId, NewStudentName.Trim());
        }
        catch (KeyNotFoundException ex)
        {
            ErrorMessage = ex.Message;
            Teacher = await teacherService.GetByIdAsync(TeacherId);
            Students = await teacherService.GetStudentsAsync(TeacherId);
            return Page();
        }
        return RedirectToPage(new { teacherId = TeacherId });
    }
}
