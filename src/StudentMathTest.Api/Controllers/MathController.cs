using Microsoft.AspNetCore.Mvc;
using StudentMathTest.Domain.Interfaces;

namespace StudentMathTest.Api.Controllers;

[ApiController]
[Route("api/math")]
[Produces("application/json")]
public class MathController(IMathEngine mathEngine) : ControllerBase
{
    /// <summary>Evaluates an arithmetic expression. Example: 6*2+3-4 = 11</summary>
    [HttpGet("evaluate")]
    public IActionResult Evaluate([FromQuery] string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return BadRequest(new { error = "Expression is required." });

        try
        {
            var result = mathEngine.Evaluate(expression);
            return Ok(new { expression, result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
