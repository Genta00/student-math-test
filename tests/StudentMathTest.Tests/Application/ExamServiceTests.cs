using Microsoft.EntityFrameworkCore;
using Moq;
using StudentMathTest.Application.Data;
using StudentMathTest.Application.Services;
using StudentMathTest.Domain.Interfaces;
using StudentMathTest.Domain.Models;

namespace StudentMathTest.Tests.Application;

public sealed class ExamServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<IXmlProcessor> _xmlProcessor;
    private readonly Mock<IMathEngine> _mathEngine;
    private readonly ExamService _service;

    public ExamServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _xmlProcessor = new Mock<IXmlProcessor>();
        _mathEngine = new Mock<IMathEngine>();
        _service = new ExamService(_db, _xmlProcessor.Object, _mathEngine.Object);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task ProcessXmlAsync_CorrectAnswer_MarksTaskAsCorrect()
    {
        // Arrange
        var exam = BuildExam(studentId: 1, teacherId: 10, expression: "2+3", studentAnswer: 5m);
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns([exam]);
        _mathEngine.Setup(x => x.Evaluate("2+3")).Returns(5m);

        // Act
        var results = (await _service.ProcessXmlAsync("<xml/>")).ToList();

        // Assert
        Assert.Single(results);
        Assert.True(results[0].Tasks.First().IsCorrect);
        Assert.Equal(5m, results[0].Tasks.First().ComputedResult);
    }

    [Fact]
    public async Task ProcessXmlAsync_WrongAnswer_MarksTaskAsIncorrect()
    {
        // Arrange
        var exam = BuildExam(studentId: 1, teacherId: 10, expression: "2+3", studentAnswer: 99m);
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns([exam]);
        _mathEngine.Setup(x => x.Evaluate("2+3")).Returns(5m);

        // Act
        var results = (await _service.ProcessXmlAsync("<xml/>")).ToList();

        // Assert
        Assert.False(results[0].Tasks.First().IsCorrect);
    }

    [Fact]
    public async Task ProcessXmlAsync_MalformedExpression_MarksTaskIncorrectWithoutThrowing()
    {
        // Arrange
        var exam = BuildExam(studentId: 1, teacherId: 10, expression: "2+x", studentAnswer: 5m);
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns([exam]);
        _mathEngine.Setup(x => x.Evaluate("2+x")).Throws<InvalidOperationException>();

        // Act
        var results = (await _service.ProcessXmlAsync("<xml/>")).ToList();

        // Assert
        Assert.Single(results);
        Assert.False(results[0].Tasks.First().IsCorrect);
        Assert.Equal(0m, results[0].Tasks.First().ComputedResult);
    }

    [Fact]
    public async Task ProcessXmlAsync_NewTeacherAndStudent_AreCreatedInDatabase()
    {
        // Arrange
        var exam = BuildExam(studentId: 42, teacherId: 99, expression: "1+1", studentAnswer: 2m);
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns([exam]);
        _mathEngine.Setup(x => x.Evaluate(It.IsAny<string>())).Returns(2m);

        // Act
        await _service.ProcessXmlAsync("<xml/>");

        // Assert
        Assert.True(await _db.Teachers.AnyAsync(t => t.Id == 99));
        Assert.True(await _db.Students.AnyAsync(s => s.Id == 42));
    }

    [Fact]
    public async Task ProcessXmlAsync_ExistingTeacherAndStudent_AreNotDuplicated()
    {
        // Arrange
        _db.Teachers.Add(new Teacher { Id = 10, Name = "Existing Teacher" });
        _db.Students.Add(new Student { Id = 1, Name = "Existing Student", TeacherId = 10 });
        await _db.SaveChangesAsync();

        var exam = BuildExam(studentId: 1, teacherId: 10, expression: "1+1", studentAnswer: 2m);
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns([exam]);
        _mathEngine.Setup(x => x.Evaluate(It.IsAny<string>())).Returns(2m);

        // Act
        await _service.ProcessXmlAsync("<xml/>");

        // Assert
        Assert.Equal(1, await _db.Teachers.CountAsync(t => t.Id == 10));
        Assert.Equal(1, await _db.Students.CountAsync(s => s.Id == 1));
    }

    [Fact]
    public async Task ProcessXmlAsync_MultipleExams_PersistsAllToDatabase()
    {
        // Arrange
        var exams = new[]
        {
            BuildExam(studentId: 1, teacherId: 10, expression: "1+1", studentAnswer: 2m),
            BuildExam(studentId: 2, teacherId: 10, expression: "3*3", studentAnswer: 9m)
        };
        _xmlProcessor.Setup(x => x.Parse(It.IsAny<string>())).Returns(exams);
        _mathEngine.Setup(x => x.Evaluate(It.IsAny<string>())).Returns(2m);

        // Act
        await _service.ProcessXmlAsync("<xml/>");

        // Assert
        Assert.Equal(2, await _db.Exams.CountAsync());
    }

    [Fact]
    public async Task GetExamsByStudentAsync_ReturnsOnlyThatStudentsExams()
    {
        // Arrange
        _db.Teachers.Add(new Teacher { Id = 1, Name = "T" });
        _db.Students.Add(new Student { Id = 10, Name = "S10", TeacherId = 1 });
        _db.Students.Add(new Student { Id = 20, Name = "S20", TeacherId = 1 });
        _db.Exams.Add(new Exam { XmlExamId = "e1", StudentId = 10, TeacherId = 1, SubmittedAt = DateTime.UtcNow });
        _db.Exams.Add(new Exam { XmlExamId = "e2", StudentId = 20, TeacherId = 1, SubmittedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        // Act
        var exams = (await _service.GetExamsByStudentAsync(10)).ToList();

        // Assert
        Assert.Single(exams);
        Assert.Equal(10, exams[0].StudentId);
    }

    [Fact]
    public async Task GetExamAsync_ExistingId_ReturnsExamWithTasks()
    {
        // Arrange
        _db.Teachers.Add(new Teacher { Id = 1, Name = "T" });
        _db.Students.Add(new Student { Id = 10, Name = "S", TeacherId = 1 });
        var exam = new Exam { XmlExamId = "e1", StudentId = 10, TeacherId = 1, SubmittedAt = DateTime.UtcNow };
        exam.Tasks.Add(new TaskResult { XmlTaskId = "t1", RawExpression = "1+1", StudentAnswer = 2m, IsCorrect = true });
        _db.Exams.Add(exam);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.GetExamAsync(exam.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tasks);
    }

    [Fact]
    public async Task GetExamAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _service.GetExamAsync(9999);

        // Assert
        Assert.Null(result);
    }

    private static Exam BuildExam(int studentId, int teacherId, string expression, decimal studentAnswer) =>
        new()
        {
            XmlExamId = "1",
            StudentId = studentId,
            TeacherId = teacherId,
            SubmittedAt = DateTime.UtcNow,
            Tasks =
            [
                new TaskResult
                {
                    XmlTaskId = "1",
                    RawExpression = expression,
                    StudentAnswer = studentAnswer
                }
            ]
        };
}
