using StudentMathTest.Domain.Models;

namespace StudentMathTest.Domain.Interfaces;

public interface IExamService
{
    Task<IEnumerable<Exam>> ProcessXmlAsync(string xmlContent);
    Task<Exam?> GetExamAsync(int examId);
    Task<IEnumerable<Exam>> GetExamsByStudentAsync(int studentId);
}
