using StudentMathTest.MathEngine;

namespace StudentMathTest.Tests.MathEngine;

public sealed class ArithmeticEvaluatorTests
{
    private readonly ArithmeticEvaluator _evaluator = new();

    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("10-4", 6)]
    [InlineData("3*4", 12)]
    [InlineData("10/2", 5)]
    public void Evaluate_BasicOperations_ReturnsCorrectResult(string expression, decimal expected)
    {
        // Act
        var result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2+3/6-4", -1.5)]
    [InlineData("6*2+3-4", 11)]
    [InlineData("10*2-5", 15)]
    [InlineData("8/2+2", 6)]
    [InlineData("2+3*4-1", 13)]
    public void Evaluate_OperatorPrecedence_MultipliesAndDividesBeforeAddingAndSubtracting(string expression, decimal expected)
    {
        // Act
        var result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("(2+3)*4", 20)]
    [InlineData("10/(2+3)", 2)]
    [InlineData("(6+2)*(3-1)", 16)]
    public void Evaluate_Parentheses_OverrideDefaultPrecedence(string expression, decimal expected)
    {
        // Act
        var result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  2 + 3 ", 5)]
    [InlineData(" 6 * 2 + 3 - 4 ", 11)]
    public void Evaluate_ExpressionWithSpaces_IgnoresWhitespace(string expression, decimal expected)
    {
        // Act
        var result = _evaluator.Evaluate(expression);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_NegativeNumber_ReturnsCorrectResult()
    {
        // Act
        var result = _evaluator.Evaluate("-5+10");

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Evaluate_DecimalNumbers_ReturnsCorrectResult()
    {
        // Act
        var result = _evaluator.Evaluate("1.5+2.5");

        // Assert
        Assert.Equal(4.0m, result);
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsDivideByZeroException()
    {
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => _evaluator.Evaluate("10/0"));
    }

    [Fact]
    public void Evaluate_UnexpectedCharacter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _evaluator.Evaluate("2+x"));
    }

    [Fact]
    public void Evaluate_MissingClosingParenthesis_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _evaluator.Evaluate("(2+3"));
    }

    [Fact]
    public void Evaluate_ChainedOperationsSamePrecedence_EvaluatesLeftToRight()
    {
        // Act
        var result = _evaluator.Evaluate("10-3-2");

        // Assert
        Assert.Equal(5, result);
    }
}
