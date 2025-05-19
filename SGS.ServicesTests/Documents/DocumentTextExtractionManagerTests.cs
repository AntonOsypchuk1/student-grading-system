using Microsoft.Extensions.Logging;
using Moq;
using SGS.Application.Interfaces;
using SGS.Application.Services.Document.Extraction;

namespace SGS.ServicesTests.Documents;

public class DocumentTextExtractionManagerTests
{
    private readonly Mock<ILogger<DocumentTextExtractionManager>> _loggerMock;
    private readonly Mock<IDocumentTextExtractor> _pdfExtractorMock;
    private readonly Mock<IDocumentTextExtractor> _txtExtractorMock;
    private readonly List<IDocumentTextExtractor> _extractors;

    public DocumentTextExtractionManagerTests()
    {
        _loggerMock = new Mock<ILogger<DocumentTextExtractionManager>>();
            
        // Setup PDF extractor mock
        _pdfExtractorMock = new Mock<IDocumentTextExtractor>();
        _pdfExtractorMock.Setup(e => e.CanExtract(It.Is<string>(s => s.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))))
            .Returns(true);
        _pdfExtractorMock.Setup(e => e.CanExtract(It.Is<string>(s => !s.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))))
            .Returns(false);
        _pdfExtractorMock.Setup(e => e.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("PDF Content");
            
        // Setup TXT extractor mock
        _txtExtractorMock = new Mock<IDocumentTextExtractor>();
        _txtExtractorMock.Setup(e => e.CanExtract(It.Is<string>(s => s.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))))
            .Returns(true);
        _txtExtractorMock.Setup(e => e.CanExtract(It.Is<string>(s => !s.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))))
            .Returns(false);
        _txtExtractorMock.Setup(e => e.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TXT Content");
            
        _extractors = new List<IDocumentTextExtractor>
        {
            _pdfExtractorMock.Object,
            _txtExtractorMock.Object
        };
    }

    [Fact]
    public async Task ExtractTextAsync_WhenPdfFile_ShouldUsePdfExtractor()
    {
        // Arrange
        var manager = new DocumentTextExtractionManager(_extractors, _loggerMock.Object);
        var filePath = "test.pdf";
        var fileBytes = new byte[] { 1, 2, 3 };

        // Act
        var result = await manager.ExtractTextAsync(filePath, fileBytes);

        // Assert
        Assert.Equal("PDF Content", result);
        _pdfExtractorMock.Verify(e => e.ExtractTextAsync(fileBytes, It.IsAny<CancellationToken>()), Times.Once);
        _txtExtractorMock.Verify(e => e.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractTextAsync_WhenTxtFile_ShouldUseTxtExtractor()
    {
        // Arrange
        var manager = new DocumentTextExtractionManager(_extractors, _loggerMock.Object);
        var filePath = "test.txt";
        var fileBytes = new byte[] { 1, 2, 3 };

        // Act
        var result = await manager.ExtractTextAsync(filePath, fileBytes);

        // Assert
        Assert.Equal("TXT Content", result);
        _txtExtractorMock.Verify(e => e.ExtractTextAsync(fileBytes, It.IsAny<CancellationToken>()), Times.Once);
        _pdfExtractorMock.Verify(e => e.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExtractTextAsync_WhenUnsupportedFileType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var manager = new DocumentTextExtractionManager(_extractors, _loggerMock.Object);
        var filePath = "test.docx";
        var fileBytes = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => 
            manager.ExtractTextAsync(filePath, fileBytes));
    }
}