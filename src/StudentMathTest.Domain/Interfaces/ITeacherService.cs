using StudentMathTest.Domain.Models;

namespace StudentMathTest.Domain.Interfaces;

public interface ITeacherService
{
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<Teacher?> GetByIdAsync(int id);
    Task<Teacher> CreateAsync(string name);
    Task<Student> AddStudentAsync(int teacherId, string studentName);
    Task<IEnumerable<Student>> GetStudentsAsync(int teacherId);
}
