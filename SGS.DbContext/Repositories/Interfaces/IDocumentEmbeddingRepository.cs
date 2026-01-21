namespace SGS.DbContext.Repositories.Interfaces;

public interface IDocumentEmbeddingRepository
{
    // TODO: Implement the repository methods and the embeddings storage itself
    Task UpsertEmbeddingAsync(Guid documentId, float[] vector);
    Task<float[]?> GetEmbeddingAsync(Guid documentId);
}