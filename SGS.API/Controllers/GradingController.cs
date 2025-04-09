using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/grading")]
public class GradingController(IGradingService gradingService) : ControllerBase
{
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPost("analyze/{docId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeDocument([FromRoute] Guid docId)
    {
        if (docId == Guid.Empty)
            return BadRequest(new { error = "Invalid document ID." });

        var result = await gradingService.AnalyzeDocumentAsync(docId);
        if (result is null)
            return NotFound(new { error = "Document not found." });
        return Ok(result);
    }

    [HttpGet("results/{docId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAnalysisResult([FromRoute] Guid docId)
    {
        if (docId == Guid.Empty)
            return BadRequest(new { error = "Invalid document ID." });

        var result = await gradingService.GetAnalysisResultByDocumentIdAsync(docId);
        if (result is null)
            return NotFound(new { error = "Analysis result not found." });
        return Ok(result);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPut("edit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditGrade([FromBody] EditGradeCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedResult = await gradingService.EditGradeAsync(command);
        if (updatedResult is null)
            return NotFound(new { error = "Grade not found." });
        return Ok(updatedResult);
    }
    
    [HttpGet("assignments/{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetGradingResultsByAssignment(Guid assignmentId)
    {
        if (assignmentId == Guid.Empty)
            return BadRequest(new { error = "Invalid assignment ID." });

        var results = await gradingService.GetGradingResultsByAssignmentAsync(assignmentId);
        if (!results.Any()) return NoContent();
        return Ok(results);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPut("finalize/{gradeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FinalizeGrade(Guid gradeId)
    {
        if (gradeId == Guid.Empty)
            return BadRequest(new { error = "Invalid grade ID." });

        var finalizedResult = await gradingService.FinalizeGradeAsync(gradeId);
        if (finalizedResult is null)
            return NotFound(new { error = "Grade not found." });
        return Ok(finalizedResult);
    }
}