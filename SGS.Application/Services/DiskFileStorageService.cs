using SGS.Application.Interfaces;

namespace SGS.Application.Services;

public class DiskFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public DiskFileStorageService(string basePath)
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] fileContent,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(_basePath, uniqueFileName);

        await File.WriteAllBytesAsync(fullPath, fileContent, cancellationToken);
        return fullPath;
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);
        return await File.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath)) await Task.Run(() => File.Delete(filePath), cancellationToken);
    }
}