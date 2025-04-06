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
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto apiDto)
    {
        if (apiDto.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

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
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDocumentById([FromRoute] Guid id)
    {
        var document = await documentService.GetDocumentByIdAsync(id);
        if (document is null)
            return NotFound();
        return Ok(document);
    }
}
