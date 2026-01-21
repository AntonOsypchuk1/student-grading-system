using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SGS.Application.Interfaces;
using SGS.Application.Services.Document;
using SGS.Domain.Entities;
using SGS.DbContext.Repositories.Interfaces;
using SGS.DbContext.Repositories.UnitOfWork;
using System.Text;

namespace SGS.ServicesTests.Documents
{
    public class DocumentServiceTests : IDisposable
    {
        private readonly Mock<IDocumentTextExtractionManager> _extractionManagerMock = new();
        private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<DocumentService>> _loggerMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly DocumentService _documentService;
        private readonly string _testFilesPath;

        public DocumentServiceTests()
        {
            _documentService = new DocumentService(
                _extractionManagerMock.Object,
                _fileStorageServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _mapperMock.Object
            );

            // Setup test directory and files
            _testFilesPath = Path.Combine(Path.GetTempPath(), "SGS_DocumentService_Tests");
            Directory.CreateDirectory(_testFilesPath);
            CreateTestFiles();
        }

        [Fact]
        public async Task GetDocumentContentTextAsync_WithRealTestDocument_ShouldReturnContent()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var filePath = "test_document.txt";
            var expectedContent = "This is a real test document for document service testing.\r\nIt has multiple lines to ensure proper extraction.";
            var document = new Document { Id = documentId, FilePath = filePath };

            var txtBytes = File.ReadAllBytes(Path.Combine(_testFilesPath, "test_document.txt"));

            var documentRepositoryMock = new Mock<IDocumentRepository>();
            documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            _unitOfWorkMock.Setup(x => x.Documents).Returns(documentRepositoryMock.Object);

            // Instead of trying to mock the extension method, mock the underlying methods
            _fileStorageServiceMock.Setup(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(txtBytes);

            _extractionManagerMock.Setup(x => x.ExtractTextAsync(filePath, txtBytes, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContent);

            // Act
            var result = await _documentService.GetDocumentContentTextAsync(documentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedContent, result.Data);
            _unitOfWorkMock.Verify(x => x.Documents.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
            _fileStorageServiceMock.Verify(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()), Times.Once);
            _extractionManagerMock.Verify(x => x.ExtractTextAsync(filePath, txtBytes, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentContentBytesAsync_WithRealTestDocument_ShouldReturnBytes()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var filePath = "test_document.txt";
            var document = new Document { Id = documentId, FilePath = filePath };

            var expectedBytes = File.ReadAllBytes(Path.Combine(_testFilesPath, "test_document.txt"));

            var documentRepositoryMock = new Mock<IDocumentRepository>();
            documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            _unitOfWorkMock.Setup(x => x.Documents).Returns(documentRepositoryMock.Object);
            _fileStorageServiceMock.Setup(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await _documentService.GetDocumentContentBytesAsync(documentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedBytes, result.Data);
            _unitOfWorkMock.Verify(x => x.Documents.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
            _fileStorageServiceMock.Verify(x => x.GetFileAsync(filePath, It.IsAny<CancellationToken>()), Times.Once);

            // Verify we can convert the bytes back to the original text
            var retrievedText = Encoding.UTF8.GetString(result.Data);
            Assert.Contains("This is a real test document", retrievedText);
        }

        // Keep the existing tests...

        [Fact]
        public async Task GetDocumentContentTextAsync_WhenDocumentDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            var documentRepositoryMock = new Mock<IDocumentRepository>();
            documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Document)null!);

            _unitOfWorkMock.Setup(x => x.Documents).Returns(documentRepositoryMock.Object);

            // Act
            var result = await _documentService.GetDocumentContentTextAsync(documentId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Document not found", result.Error);
            _unitOfWorkMock.Verify(x => x.Documents.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
            _fileStorageServiceMock.Verify(x => x.GetFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WhenDocumentExists_ShouldDeleteAndReturnSuccess()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var filePath = "test.pdf";
            var document = new Document { Id = documentId, FilePath = filePath };

            var documentRepositoryMock = new Mock<IDocumentRepository>();
            documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            _unitOfWorkMock.Setup(x => x.Documents).Returns(documentRepositoryMock.Object);
            _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(filePath, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(documentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data);
            _unitOfWorkMock.Verify(x => x.Documents.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
            _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(filePath, It.IsAny<CancellationToken>()), Times.Once);
            documentRepositoryMock.Verify(x => x.Delete(document), Times.Once);
        }

        // Create test files that our tests will use
        private void CreateTestFiles()
        {
            // Create a TXT test document
            string txtContent = "This is a real test document for document service testing.\r\nIt has multiple lines to ensure proper extraction.";
            File.WriteAllText(Path.Combine(_testFilesPath, "test_document.txt"), txtContent);

            // Here you could also create a PDF test file if needed
            // For comprehensive testing, we should have a real small PDF file in test resources
        }

        // Clean up test files after all tests run
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
                    // Log but continue if we can't delete (might be locked)
                    Console.WriteLine($"Could not delete test directory {_testFilesPath}");
                }
            }
        }
    }
}
