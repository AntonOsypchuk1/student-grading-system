using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using SGS.Application.Interfaces;

namespace SGS.Application.Services.Document.Storage;

public class BlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("AzureStorage");
        var connectionString = settings["ConnectionString"];
        var containerName = settings["ContainerName"];

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] fileContent, CancellationToken cancellationToken = default)
    {
        var blobName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = _container.GetBlobClient(blobName);
        
        using var stream = new MemoryStream(fileContent);
        var httpHeaders = new BlobHttpHeaders { ContentType = GetContentType(fileName) };
        var options = new BlobUploadOptions { HttpHeaders = httpHeaders };
        
        await blobClient.UploadAsync(stream, options, cancellationToken);
        return blobName;
    }

    public async Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(filePath);
        
        if (!await blobClient.ExistsAsync(cancellationToken))
            throw new FileNotFoundException($"File {filePath} not found in blob storage");
            
        var download = await blobClient.DownloadAsync(cancellationToken);
        using var memoryStream = new MemoryStream();
        await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var blobClient = _container.GetBlobClient(filePath);
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}