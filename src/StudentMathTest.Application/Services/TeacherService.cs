using Microsoft.EntityFrameworkCore;
using StudentMathTest.Application.Data;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Application.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly AppDbContext _db;

    public TeacherService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Teacher>> GetAllAsync() =>
        await _db.Teachers.Include(t => t.Students).ToListAsync();

    public async Task<Teacher?> GetByIdAsync(int id) =>
        await _db.Teachers.Include(t => t.Students).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Teacher> CreateAsync(string name)
    {
        var teacher = new Teacher { Name = name };
        _db.Teachers.Add(teacher);
        await _db.SaveChangesAsync();
        return teacher;
    }

    public async Task<Student> AddStudentAsync(int teacherId, string studentName)
    {
        var teacher = await _db.Teachers.FindAsync(teacherId)
            ?? throw new KeyNotFoundException($"Teacher {teacherId} not found.");

        var student = new Student { Name = studentName, TeacherId = teacherId };
        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        return student;
    }

    public async Task<IEnumerable<Student>> GetStudentsAsync(int teacherId) =>
        await _db.Students.Where(s => s.TeacherId == teacherId).ToListAsync();
}
