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
        
        if (result.Data is null || !result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentById([FromRoute] Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid document ID." });

        var result = await documentService.GetDocumentByIdAsync(id);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
            
        if (result.Data is null)
            return NotFound(new { error = "Document not found." });
            
        return Ok(result.Data);
    }
    
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentsByUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "Invalid user ID." });

        var result = await documentService.GetDocumentsByUserIdAsync(userId);
        
        if (!result.IsSuccess)
            return StatusCode(500, new { error = result.Error });
            
        if (result.Data is null) 
            return NotFound(new { error = "User not found." });
            
        if (!result.Data.Any()) 
            return NoContent();
            
        return Ok(result.Data);
    }
}