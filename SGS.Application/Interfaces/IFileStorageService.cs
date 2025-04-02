namespace SGS.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken = default);
    Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}