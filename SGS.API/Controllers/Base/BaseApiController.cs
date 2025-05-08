using Microsoft.AspNetCore.Mvc;
using SGS.Application.Models;

namespace SGS.API.Controllers.Base;

public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return StatusCode(500, new ProblemDetails { Detail = result.Error });
        
        if (result.Data is null)
            return NotFound(new ProblemDetails { Detail = "Resource not found." });
        
        return Ok(result.Data);
    }

    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (!result.IsSuccess)
            return BadRequest(new ProblemDetails { Detail = result.Error });
        
        return CreatedAtAction(actionName, routeValues, result.Data);
    }

    protected IActionResult HandleCollectionResult<T>(Result<IEnumerable<T>> result)
    {
        if (!result.IsSuccess)
            return StatusCode(500, new ProblemDetails { Detail = result.Error });
        
        if (result.Data is null || !result.Data.Any())
            return NoContent();

        return Ok(result.Data);
    }
}