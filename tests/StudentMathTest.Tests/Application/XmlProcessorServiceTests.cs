using StudentMathTest.Application.Services;

namespace StudentMathTest.Tests.Application;

public sealed class XmlProcessorServiceTests
{
    private readonly XmlProcessorService _service = new();

    private const string ValidXml = """
        <Teacher ID="11111">
          <Students>
            <Student ID="12345">
              <Exam Id="1">
                <Task id="1">2+3 = 5</Task>
                <Task id="2">6*2+3-4 = 11</Task>
              </Exam>
            </Student>
            <Student ID="54321">
              <Exam Id="1">
                <Task id="1">10*2-5 = 15</Task>
              </Exam>
            </Student>
          </Students>
        </Teacher>
        """;

    [Fact]
    public void Parse_ValidXml_ReturnsCorrectExamCount()
    {
        // Act
        var exams = _service.Parse(ValidXml).ToList();

        // Assert
        Assert.Equal(2, exams.Count);
    }

    [Fact]
    public void Parse_ValidXml_MapsTeacherIdCorrectly()
    {
        // Act
        var exams = _service.Parse(ValidXml).ToList();

        // Assert
        Assert.All(exams, e => Assert.Equal(11111, e.TeacherId));
    }

    [Fact]
    public void Parse_ValidXml_MapsStudentIdsCorrectly()
    {
        // Act
        var exams = _service.Parse(ValidXml).ToList();

        // Assert
        Assert.Equal(12345, exams[0].StudentId);
        Assert.Equal(54321, exams[1].StudentId);
    }

    [Fact]
    public void Parse_ValidXml_MapsTaskExpressionsAndAnswers()
    {
        // Act
        var exams = _service.Parse(ValidXml).ToList();
        var tasks = exams[0].Tasks.ToList();

        // Assert
        Assert.Equal(2, tasks.Count);
        Assert.Equal("2+3", tasks[0].RawExpression);
        Assert.Equal(5m, tasks[0].StudentAnswer);
        Assert.Equal("6*2+3-4", tasks[1].RawExpression);
        Assert.Equal(11m, tasks[1].StudentAnswer);
    }

    [Fact]
    public void Parse_ValidXml_MapsXmlExamId()
    {
        // Act
        var exams = _service.Parse(ValidXml).ToList();

        // Assert
        Assert.Equal("1", exams[0].XmlExamId);
    }

    [Fact]
    public void Parse_MultipleStudentsMultipleExams_ParsesAll()
    {
        // Arrange
        const string xml = """
                           <Teacher ID="1">
                             <Students>
                               <Student ID="100">
                                 <Exam Id="1"><Task id="1">1+1 = 2</Task></Exam>
                                 <Exam Id="2"><Task id="1">2+2 = 4</Task></Exam>
                               </Student>
                             </Students>
                           </Teacher>
                           """;

        // Act
        var exams = _service.Parse(xml).ToList();

        // Assert
        Assert.Equal(2, exams.Count);
        Assert.Equal("1", exams[0].XmlExamId);
        Assert.Equal("2", exams[1].XmlExamId);
    }

    [Fact]
    public void Parse_MissingTeacherId_ThrowsInvalidOperationException()
    {
        // Arrange
        const string xml = "<Teacher><Students></Students></Teacher>";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Parse(xml).ToList());
    }

    [Fact]
    public void Parse_MissingStudentId_ThrowsInvalidOperationException()
    {
        // Arrange
        const string xml = """
                           <Teacher ID="1">
                             <Students>
                               <Student>
                                 <Exam Id="1"><Task id="1">1+1 = 2</Task></Exam>
                               </Student>
                             </Students>
                           </Teacher>
                           """;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Parse(xml).ToList());
    }

    [Fact]
    public void Parse_TaskWithNonNumericAnswer_ThrowsInvalidOperationException()
    {
        // Arrange
        const string xml = """
                           <Teacher ID="1">
                             <Students>
                               <Student ID="1">
                                 <Exam Id="1"><Task id="1">1+1 = abc</Task></Exam>
                               </Student>
                             </Students>
                           </Teacher>
                           """;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Parse(xml).ToList());
    }

    [Fact]
    public void Parse_TaskWithNoEqualSign_ThrowsInvalidOperationException()
    {
        // Arrange
        const string xml = """
                           <Teacher ID="1">
                             <Students>
                               <Student ID="1">
                                 <Exam Id="1"><Task id="1">1+1</Task></Exam>
                               </Student>
                             </Students>
                           </Teacher>
                           """;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Parse(xml).ToList());
    }

    [Fact]
    public void Parse_EmptyStudentsList_ReturnsEmptyCollection()
    {
        // Arrange
        const string xml = "<Teacher ID=\"1\"><Students></Students></Teacher>";

        // Act
        var exams = _service.Parse(xml).ToList();

        // Assert
        Assert.Empty(exams);
    }
}
