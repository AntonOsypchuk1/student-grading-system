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
    public async Task<IActionResult> AnalyzeDocument([FromRoute] Guid docId)
    {
        var result = await gradingService.AnalyzeDocumentAsync(docId);
        return Ok(result);
    }

    [HttpGet("results/{docId:guid}")]
    public async Task<IActionResult> GetAnalysisResult([FromRoute] Guid docId)
    {
        var result = await gradingService.GetAnalysisResultByDocumentIdAsync(docId);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPut("edit")]
    public async Task<IActionResult> EditGrade([FromBody] EditGradeCommand command)
    {
        var updatedResult = await gradingService.EditGradeAsync(command);
        return Ok(updatedResult);
    }
    
    [HttpGet("assignments/{assignmentId:guid}")]
    public async Task<IActionResult> GetGradingResultsByAssignment(Guid assignmentId)
    {
        var results = await gradingService.GetGradingResultsByAssignmentAsync(assignmentId);
        return Ok(results);
    }

    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPut("finalize/{gradeId:guid}")]
    public async Task<IActionResult> FinalizeGrade(Guid gradeId)
    {
        var finalizedResult = await gradingService.FinalizeGradeAsync(gradeId);
        return Ok(finalizedResult);
    }
}