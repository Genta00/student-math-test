using StudentMathTest.Domain.Models;

namespace StudentMathTest.Domain.Interfaces;

public interface IXmlProcessor
{
    IEnumerable<Exam> Parse(string xmlContent);
}
