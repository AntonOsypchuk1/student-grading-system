namespace SGS.API.DTOs;

public class UploadDocumentDto
{
    public IFormFile File { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid AssignmentId { get; set; }
}