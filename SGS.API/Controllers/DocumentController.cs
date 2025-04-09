using Microsoft.AspNetCore.Mvc;
using SGS.API.DTOs;
using SGS.Application.DTOs;
using SGS.Application.Interfaces;

namespace SGS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController(IDocumentService documentService) : ControllerBase
{
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid document ID." });

        var document = await documentService.GetDocumentByIdAsync(id);
        if (document is null)
            return NotFound(new { error = "Document not found." });
        return Ok(document);
    }
    
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentsByUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID." });

        var documents = await documentService.GetDocumentsByUserIdAsync(userId);
        if (documents is null) return NotFound(new { error = "User not found." });
        if (!documents.Any()) return NoContent();
        return Ok(documents);
    }
}