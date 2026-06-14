using Microsoft.AspNetCore.Mvc;
using StudentMathTest.Application.DTOs;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Controllers;

[ApiController]
[Route("api/teachers")]
[Produces("application/json")]
public class TeachersController(ITeacherService teacherService) : ControllerBase
{
    /// <summary>Returns all registered teachers.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teachers = await teacherService.GetAllAsync();
        return Ok(teachers.Select(t => new TeacherDto(t.Id, t.Name)));
    }

    /// <summary>Creates a new teacher.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeacherRequest request)
    {
        var teacher = await teacherService.CreateAsync(request.Name);
        return CreatedAtAction(nameof(GetAll), new TeacherDto(teacher.Id, teacher.Name));
    }

    /// <summary>Returns all students for a teacher.</summary>
    [HttpGet("{teacherId}/students")]
    public async Task<IActionResult> GetStudents(int teacherId)
    {
        var students = await teacherService.GetStudentsAsync(teacherId);
        return Ok(students.Select(s => new StudentDto(s.Id, s.Name, s.TeacherId)));
    }

    /// <summary>Adds a student to a teacher.</summary>
    [HttpPost("{teacherId}/students")]
    public async Task<IActionResult> AddStudent(int teacherId, [FromBody] CreateStudentRequest request)
    {
        try
        {
            var student = await teacherService.AddStudentAsync(teacherId, request.Name);
            return CreatedAtAction(nameof(GetStudents), new { teacherId },
                new StudentDto(student.Id, student.Name, student.TeacherId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
