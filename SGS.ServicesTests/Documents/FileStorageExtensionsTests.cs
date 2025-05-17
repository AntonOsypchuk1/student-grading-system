using Moq;
using SGS.Application.Extensions;
using SGS.Application.Interfaces;

namespace SGS.ServicesTests.Documents;

public class FileStorageExtensionsTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IDocumentTextExtractionManager> _extractionManagerMock = new();

    [Fact]
    public async Task GetFileTextAsync_ShouldExtractTextFromFile()
    {
        // Arrange
        var filePath = "test.pdf";
        var fileBytes = new byte[] { 1, 2, 3 };
        var extractedText = "Test document content";

        _fileStorageServiceMock.Setup(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileBytes);
        _extractionManagerMock.Setup(x => x.ExtractTextAsync(filePath, fileBytes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedText);

        // Act
        var result = await _fileStorageServiceMock.Object.GetFileTextAsync(
            filePath, _extractionManagerMock.Object);

        // Assert
        Assert.Equal(extractedText, result);
        _fileStorageServiceMock.Verify(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()), Times.Once);
        _extractionManagerMock.Verify(x => x.ExtractTextAsync(filePath, fileBytes, It.IsAny<CancellationToken>()), Times.Once);
    }
}