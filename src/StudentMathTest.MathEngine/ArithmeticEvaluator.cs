using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.MathEngine;

/// <summary>
/// Evaluates arithmetic expressions using a recursive-descent parser.
/// Multiply and divide bind tighter than add and subtract; equal-priority operators evaluate left to right.
/// </summary>
public sealed class ArithmeticEvaluator : IMathEngine
{
    private string _input = string.Empty;
    private int _pos;

    public decimal Evaluate(string expression)
    {
        _input = expression.Replace(" ", "");
        _pos = 0;
        var result = ParseExpr();
        if (_pos != _input.Length)
            throw new InvalidOperationException($"Unexpected character '{_input[_pos]}' at position {_pos}.");
        return result;
    }

    private decimal ParseExpr()
    {
        var left = ParseTerm();
        while (_pos < _input.Length && (_input[_pos] == '+' || _input[_pos] == '-'))
        {
            var op = _input[_pos++];
            var right = ParseTerm();
            left = op == '+' ? left + right : left - right;
        }
        return left;
    }

    private decimal ParseTerm()
    {
        var left = ParseFactor();
        while (_pos < _input.Length && (_input[_pos] == '*' || _input[_pos] == '/'))
        {
            var op = _input[_pos++];
            var right = ParseFactor();
            if (op == '/' && right == 0)
                throw new DivideByZeroException("Division by zero in expression.");
            left = op == '*' ? left * right : left / right;
        }
        return left;
    }

    private decimal ParseFactor()
    {
        if (_pos < _input.Length && _input[_pos] == '(')
        {
            _pos++;
            var val = ParseExpr();
            if (_pos >= _input.Length || _input[_pos] != ')')
                throw new InvalidOperationException("Missing closing parenthesis.");
            _pos++;
            return val;
        }

        var negative = false;
        if (_pos < _input.Length && _input[_pos] == '-')
        {
            negative = true;
            _pos++;
        }

        if (_pos >= _input.Length || (!char.IsDigit(_input[_pos]) && _input[_pos] != '.'))
            throw new InvalidOperationException($"Expected number at position {_pos}.");

        var start = _pos;
        while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '.'))
            _pos++;

        var number = decimal.Parse(_input[start.._pos], System.Globalization.CultureInfo.InvariantCulture);
        return negative ? -number : number;
    }
}
