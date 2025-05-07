using Microsoft.AspNetCore.Mvc;
using SGS.API.Controllers.Base;
using SGS.API.DTOs;
using SGS.API.Extensions;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController(IDocumentService documentService) : BaseApiController
{
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto apiDto)
    {
        if (apiDto.File is null || apiDto.File.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        // TODO: Store it somewhere
        if (apiDto.File.Length > 10485760) // 10MB limit
            return BadRequest(new { error = "File size exceeds limit of 10MB." });

        byte[] fileContent;
        using (var memoryStream = new MemoryStream())
        {
            await apiDto.File.CopyToAsync(memoryStream);
            fileContent = memoryStream.ToArray();
        }

        var command = new UploadDocumentCommand
        {
            UserId = apiDto.UserId,
            AssignmentId = apiDto.AssignmentId,
            FileName = apiDto.File.FileName,
            FileContent = fileContent
        };

        var result = await documentService.UploadDocumentAsync(command);
        return HandleCreatedResult(result, nameof(GetDocumentById), new { id = result.Data?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentById([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;

        var result = await documentService.GetDocumentByIdAsync(id);
        return HandleResult(result);
    }
    
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentsByUserId(Guid userId)
    {
        if (!this.IsValidGuid(userId, out var errorResult))
            return errorResult;

        var result = await documentService.GetDocumentsByUserIdAsync(userId);
        return HandleCollectionResult(result);
    }
}