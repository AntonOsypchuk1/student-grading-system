using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
    [Authorize]
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

        if (!this.IsValidFileType(apiDto.File, out var errorMessage))
            return BadRequest(new { error = errorMessage });
        
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

    [Authorize]
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
    
    [Authorize]
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadDocument([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;
            
        var documentResult = await documentService.GetDocumentByIdAsync(id);
        if (!documentResult.IsSuccess || documentResult.Data == null)
            return NotFound(new { error = documentResult.Error });
            
        var contentResult = await documentService.GetDocumentContentBytesAsync(id);
        if (!contentResult.IsSuccess)
            return StatusCode(500, new { error = contentResult.Error });
            
        var contentType = GetContentType(documentResult.Data.FileName);
        return File(
            contentResult.Data!,
            contentType,
            documentResult.Data.FileName
        );
    }
    
    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDocument([FromRoute] Guid id)
    {
        if (!this.IsValidGuid(id, out var errorResult))
            return errorResult;
            
        var result = await documentService.DeleteDocumentAsync(id);
        return HandleResult(result);
    }
    
    [Authorize]
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

    private static string GetContentType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out var contentType))
            contentType = "application/octet-stream"; // Default content type
        
        return contentType;
    }
}