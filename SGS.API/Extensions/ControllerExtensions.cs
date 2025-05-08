using Microsoft.AspNetCore.Mvc;

namespace SGS.API.Extensions;

public static class ControllerExtensions
{
    public static bool IsValidGuid(this ControllerBase controller, Guid id, out IActionResult errorResult)
    {
        if (id != Guid.Empty)
        {
            errorResult = null!;
            return true;
        }
        
        errorResult = controller.BadRequest(new ProblemDetails { Detail = "Invalid ID." });
        return false;
    }
}