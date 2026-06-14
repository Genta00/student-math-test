using Microsoft.EntityFrameworkCore;
using StudentMathTest.Application.Data;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Application.Services;

public sealed class ExamService : IExamService
{
    private readonly AppDbContext _db;
    private readonly IXmlProcessor _xmlProcessor;
    private readonly IMathEngine _mathEngine;

    public ExamService(AppDbContext db, IXmlProcessor xmlProcessor, IMathEngine mathEngine)
    {
        _db = db;
        _xmlProcessor = xmlProcessor;
        _mathEngine = mathEngine;
    }

    public async Task<IEnumerable<Exam>> ProcessXmlAsync(string xmlContent)
    {
        var parsedExams = _xmlProcessor.Parse(xmlContent);
        var savedExams = new List<Exam>();

        foreach (var exam in parsedExams)
        {
            await UpsertTeacherAsync(exam.TeacherId);
            await UpsertStudentAsync(exam.StudentId, exam.TeacherId);

            foreach (var task in exam.Tasks)
            {
                try
                {
                    task.ComputedResult = _mathEngine.Evaluate(task.RawExpression);
                    task.IsCorrect = task.ComputedResult == task.StudentAnswer;
                }
                catch (Exception)
                {
                    task.ComputedResult = 0;
                    task.IsCorrect = false;
                }
            }

            _db.Exams.Add(exam);
            savedExams.Add(exam);
        }

        await _db.SaveChangesAsync();
        return savedExams;
    }

    public async Task<Exam?> GetExamAsync(int examId) =>
        await _db.Exams
            .Include(e => e.Tasks)
            .FirstOrDefaultAsync(e => e.Id == examId);

    public async Task<IEnumerable<Exam>> GetExamsByStudentAsync(int studentId) =>
        await _db.Exams
            .Include(e => e.Tasks)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync();

    private async Task UpsertTeacherAsync(int teacherId)
    {
        if (!await _db.Teachers.AnyAsync(t => t.Id == teacherId))
        {
            _db.Teachers.Add(new Teacher { Id = teacherId, Name = $"Teacher {teacherId}" });
            await _db.SaveChangesAsync();
        }
    }

    private async Task UpsertStudentAsync(int studentId, int teacherId)
    {
        if (!await _db.Students.AnyAsync(s => s.Id == studentId))
        {
            _db.Students.Add(new Student { Id = studentId, Name = $"Student {studentId}", TeacherId = teacherId });
            await _db.SaveChangesAsync();
        }
    }
}
