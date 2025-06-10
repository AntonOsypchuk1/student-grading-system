namespace SGS.Application.DTOs;

public class UploadDocumentCommand
{
    public Guid UserId { get; set; }
    public Guid AssignmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = [];
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AssignmentId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public double? Grade { get; set; }
    public bool? IsFinalized { get; set; }
}