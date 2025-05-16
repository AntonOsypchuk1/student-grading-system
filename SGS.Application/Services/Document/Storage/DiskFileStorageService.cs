using Microsoft.Extensions.Options;
using SGS.Application.Common.Options;
using SGS.Application.Interfaces;

namespace SGS.Application.Services.Document.Storage;

public class DiskFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    
    public DiskFileStorageService(IOptions<StorageOptions> options)
    {
        _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
        if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] fileContent,
        CancellationToken cancellationToken = default)
    {
        var fileId = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, fileId);

        await File.WriteAllBytesAsync(filePath, fileContent, cancellationToken);
        return fileId;
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        
        if (!File.Exists(fullPath)) 
            throw new FileNotFoundException("File not found", filePath);
        
        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, filePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
        
        return Task.CompletedTask;
    }
}