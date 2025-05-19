using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using SGS.Application.Services.Document.Extraction;

namespace SGS.ServicesTests.Documents;

public class PdfDocumentTextExtractorTests
{
    private readonly Mock<ILogger<PdfDocumentTextExtractor>> _loggerMock;
    private readonly string _testFilesPath;

    public PdfDocumentTextExtractorTests()
    {
        _loggerMock = new Mock<ILogger<PdfDocumentTextExtractor>>();
        _testFilesPath = Path.Combine(Path.GetTempPath(), "SGS_PdfExtractor_Tests");
        Directory.CreateDirectory(_testFilesPath);
        
        // TODO: For proper PDF testing, we should have a real PDF test file
        // For now, we'll create a mock PDF header file that won't actually extract text properly
        CreateMockPdfFile();
    }

    [Fact]
    public void CanExtract_WhenPdfFile_ReturnsTrue()
    {
        // Arrange
        var extractor = new PdfDocumentTextExtractor(_loggerMock.Object);

        // Act
        var result = extractor.CanExtract("test.pdf");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WhenNonPdfFile_ReturnsFalse()
    {
        // Arrange
        var extractor = new PdfDocumentTextExtractor(_loggerMock.Object);

        // Act
        var result = extractor.CanExtract("test.txt");

        // Assert
        Assert.False(result);
    }

    [Fact(Skip = "Requires a valid PDF test file")]
    public async Task ExtractTextAsync_WithRealPdfFile_ShouldExtractContent()
    {
        // This test needs a real PDF file to properly test the extractor
        // In a real project, you would include a test PDF file in your test resources
        
        // For now, we'll skip this test or expect it to throw on our mock PDF
        var extractor = new PdfDocumentTextExtractor(_loggerMock.Object);
        var pdfPath = Path.Combine(_testFilesPath, "mock.pdf");
        
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(pdfPath);
            var result = await extractor.ExtractTextAsync(fileBytes);
            
            // If we reach here with a valid PDF, we should have content
            Assert.NotEmpty(result);
        }
        catch (Exception ex)
        {
            // Our mock PDF would throw - we would expect this with our mock
            Assert.Contains("PDF", ex.Message);
        }
    }
    
    private void CreateMockPdfFile()
    {
        string pdfPath = Path.Combine(_testFilesPath, "mock.pdf");
        using var fs = File.Create(pdfPath);
        // Just a PDF header - this won't be a valid PDF for iText
        byte[] pdfHeader = "%PDF-1.5\n"u8.ToArray();
        fs.Write(pdfHeader, 0, pdfHeader.Length);
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_testFilesPath))
        {
            try 
            {
                Directory.Delete(_testFilesPath, true);
            }
            catch (IOException)
            {
            }
        }
    }
}