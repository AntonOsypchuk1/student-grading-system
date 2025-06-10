using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/grading")]
public class GradingController(
    IGradingService gradingService,
    IAnalysisService analysisService) : BaseApiController
{
    [Authorize(Policy = "RequireTeacherRole")]
    [HttpPost("analyze/{docId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeDocument([FromRoute] Guid docId)
    {
        if (!this.IsValidGuid(docId, out var errorResult))
            return errorResult;

        var result = await analysisService.AnalyzeDocumentAsync(docId);
        return HandleCreatedResult(result, nameof(GetAnalysisResult), new { id = result.Data?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAnalysisResult([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;
        
        var result = await analysisService.GetAnalysisResultByIdAsync(id);
        return HandleResult(result);
    }

    [HttpGet("by-document/{docId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAnalysisResultByDocumentId([FromRoute] Guid docId)
    {
        if (!this.IsValidGuid(docId, out var errorResult))
            return errorResult;
        
        var result = await analysisService.GetAnalysisResultByDocumentIdAsync(docId);
        return HandleResult(result);
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

        var result = await gradingService.EditGradeAsync(command);
        return HandleResult(result);
    }
    
    [HttpGet("assignments/{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetGradingResultsByAssignment(Guid assignmentId)
    {
        if (!this.IsValidGuid(assignmentId, out var errorResult))
            return errorResult;
        
        var result = await gradingService.GetGradingResultsByAssignmentAsync(assignmentId);
        return HandleCollectionResult(result);
    }
    
    [HttpGet("course/{courseId:guid}&groupId={groupId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetGradingResultsByCourseAndGroup(Guid courseId, Guid groupId)
    {
        if (!this.IsValidGuid(courseId, out var errorResult) || !this.IsValidGuid(groupId, out errorResult))
            return errorResult;

        var result = await gradingService.GetGradingResultsByCourseAndGroupAsync(courseId, groupId);
        return HandleResult(result);
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
        if (!this.IsValidGuid(gradeId, out var errorResult))
            return errorResult;
        
        var result = await gradingService.FinalizeGradeAsync(gradeId);
        return HandleResult(result);
    }
}