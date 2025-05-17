using SGS.Application.Services.Document.Extraction;
using System.Text;

namespace SGS.ServicesTests.Documents;

public class TxtDocumentTextExtractorTests : IDisposable
{
    private readonly string _testFilesPath;
        
    public TxtDocumentTextExtractorTests()
    {
        _testFilesPath = Path.Combine(Path.GetTempPath(), "SGS_TxtExtractor_Tests");
        Directory.CreateDirectory(_testFilesPath);
        CreateTestTxtFile();
    }
        
    [Fact]
    public void CanExtract_WhenTxtFile_ReturnsTrue()
    {
        // Arrange
        var extractor = new TxtDocumentTextExtractor();

        // Act
        var result = extractor.CanExtract("test.txt");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExtract_WhenNonTxtFile_ReturnsFalse()
    {
        // Arrange
        var extractor = new TxtDocumentTextExtractor();

        // Act
        var result = extractor.CanExtract("test.pdf");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExtractTextAsync_WithRealTxtFile_ReturnsCorrectText()
    {
        // Arrange
        var extractor = new TxtDocumentTextExtractor();
        var txtFilePath = Path.Combine(_testFilesPath, "real_test.txt");
        var expectedContent = "This is a real test file.\r\nIt contains multiple lines.\r\nThe extractor should correctly handle line breaks.";
        var fileBytes = await File.ReadAllBytesAsync(txtFilePath);

        // Act
        var result = await extractor.ExtractTextAsync(fileBytes);

        // Assert
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task ExtractTextAsync_WithUtf8BomFile_HandlesEncodingCorrectly()
    {
        // Arrange
        var extractor = new TxtDocumentTextExtractor();
        var txtFilePath = Path.Combine(_testFilesPath, "utf8_bom_test.txt");
        var expectedContent = "UTF-8 BOM test file content";
            
        // Create a UTF-8 file with BOM
        await using (var fs = new FileStream(txtFilePath, FileMode.Create))
        {
            byte[] bom = { 0xEF, 0xBB, 0xBF };
            fs.Write(bom, 0, bom.Length);
                
            byte[] content = Encoding.UTF8.GetBytes(expectedContent);
            fs.Write(content, 0, content.Length);
        }
            
        var fileBytes = await File.ReadAllBytesAsync(txtFilePath);

        // Act
        var result = await extractor.ExtractTextAsync(fileBytes);

        // Assert
        Assert.Equal(expectedContent, result);
    }
        
    private void CreateTestTxtFile()
    {
        var content = "This is a real test file.\r\nIt contains multiple lines.\r\nThe extractor should correctly handle line breaks.";
        File.WriteAllText(Path.Combine(_testFilesPath, "real_test.txt"), content);
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
                // Log but continue
            }
        }
    }
}