using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.MathEngine;

public sealed class MathEngineAdapter : IMathEngine
{
    private readonly ArithmeticEvaluator _evaluator = new();

    public decimal Evaluate(string expression) => _evaluator.Evaluate(expression);
}
