using SGS.Domain.Common;

namespace SGS.Domain.Entities;

public class DocumentEmbedding : BaseEntity
{
    public Guid DocumentId { get; set; }
    public float[] Vector { get; set; }
}