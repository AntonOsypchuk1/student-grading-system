using Microsoft.Extensions.Logging;
using Moq;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Application.Services.Analysis;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;

namespace SGS.ServicesTests.Analysis;

public class SemanticKernelAiServiceTests
{
    private readonly Mock<IPromptServiceExecutor> _promptServiceExecutorMock = new();
    private readonly Mock<IDocumentService> _documentServiceMock = new();
    private readonly Mock<ILogger<SemanticKernelAiService>> _loggerMock = new();
    private readonly SemanticKernelAiService _aiService;

    public SemanticKernelAiServiceTests()
    {
        _aiService = new SemanticKernelAiService(
            _promptServiceExecutorMock.Object,
            _documentServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithValidInputs_ShouldReturnSummaryAndGrade()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var documentText = "This is a sample document text for testing.";
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 0.6f
            },
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Structure", 
                Description = "Document should be well structured", 
                MaxGrade = 100,
                Weight = 0.4f
            }
        };

        // Setup document service mock
        var documentResult = new TestResult<string>(true, documentText, null);
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentResult);

        // Setup evaluateCriteria response
        var criteriaResponse = new EvaluateCriteriaResponse
        {
            CriteriaResults =
            [
                new CriterionResult
                {
                    Id = criteria[0].Id.ToString(),
                    Title = criteria[0].Title,
                    Score = 80,
                    Weight = criteria[0].Weight,
                    Justification = "Good clarity overall",
                    Strengths = ["Clear language"],
                    Improvements = ["Could be more concise"]
                },

                new CriterionResult
                {
                    Id = criteria[1].Id.ToString(),
                    Title = criteria[1].Title,
                    Score = 70,
                    Weight = criteria[1].Weight,
                    Justification = "Decent structure",
                    Strengths = ["Logical flow"],
                    Improvements = ["Better paragraphing needed"]
                }
            ]
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
                It.Is<EvaluateCriteriaRequest>(r => r.DocumentText == documentText),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteriaResponse);

        // Setup summary response
        var summaryResponse = new GenerateSummaryResponse
        {
            Summary = "This is a good document with clear language and logical flow. Score: 76/100"
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(
                It.IsAny<GenerateSummaryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaryResponse);

        // Act
        var result = await _aiService.AnalyzeDocumentAsync(document, criteria);

        // Assert
        Assert.Equal(summaryResponse.Summary, result.summary);
        Assert.Equal(76, result.generatedGrade); // 80*0.6 + 70*0.4 = 76
            
        // Verify method calls
        _documentServiceMock.Verify(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
        _promptServiceExecutorMock.Verify(x => x.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            It.IsAny<EvaluateCriteriaRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _promptServiceExecutorMock.Verify(x => x.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(
            It.IsAny<GenerateSummaryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithEmptyDocumentText_ShouldThrowException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 1.0f
            }
        };

        // Setup document service to return empty text
        var documentResult = new TestResult<string>(true, string.Empty, null);
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _aiService.AnalyzeDocumentAsync(document, criteria));
            
        Assert.Contains("Document text extraction failed or returned empty content", exception.Message);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithEmptyCriteriaResults_ShouldThrowException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var documentText = "This is a sample document text for testing.";
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 1.0f
            }
        };

        // Setup document service mock
        var documentResult = new TestResult<string>(true, documentText, null);
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentResult);

        // Setup evaluateCriteria response with empty results
        var criteriaResponse = new EvaluateCriteriaResponse
        {
            CriteriaResults = []
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
                It.IsAny<EvaluateCriteriaRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteriaResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _aiService.AnalyzeDocumentAsync(document, criteria));
            
        Assert.Contains("Failed to evaluate document criteria or received empty results", exception.Message);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithNullSummaryResponse_ShouldThrowException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var documentText = "This is a sample document text for testing.";
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 1.0f
            }
        };

        // Setup document service mock
        var documentResult = new TestResult<string>(true, documentText, null);
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentResult);

        // Setup evaluateCriteria response
        var criteriaResponse = new EvaluateCriteriaResponse
        {
            CriteriaResults =
            [
                new CriterionResult
                {
                    Id = criteria[0].Id.ToString(),
                    Title = criteria[0].Title,
                    Score = 80,
                    Weight = criteria[0].Weight,
                    Justification = "Good clarity",
                    Strengths = ["Clear language"],
                    Improvements = ["Could be more concise"]
                }
            ]
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
                It.IsAny<EvaluateCriteriaRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteriaResponse);

        // Setup null summary response
        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(
                It.IsAny<GenerateSummaryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GenerateSummaryResponse)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _aiService.AnalyzeDocumentAsync(document, criteria));
            
        Assert.Contains("Failed to generate summary", exception.Message);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithEmptySummary_ShouldThrowException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var documentText = "This is a sample document text for testing.";
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 1.0f
            }
        };

        // Setup document service mock
        var documentResult = new TestResult<string>(true, documentText, null);
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentResult);

        // Setup evaluateCriteria response
        var criteriaResponse = new EvaluateCriteriaResponse
        {
            CriteriaResults =
            [
                new CriterionResult()
                {
                    Id = criteria[0].Id.ToString(),
                    Title = criteria[0].Title,
                    Score = 80,
                    Weight = criteria[0].Weight,
                    Justification = "Good clarity",
                    Strengths = ["Clear language"],
                    Improvements = ["Could be more concise"]
                }
            ]
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
                It.IsAny<EvaluateCriteriaRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteriaResponse);

        // Setup empty summary response
        var emptySummaryResponse = new GenerateSummaryResponse
        {
            Summary = string.Empty
        };

        _promptServiceExecutorMock.Setup(x => x.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(
                It.IsAny<GenerateSummaryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySummaryResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _aiService.AnalyzeDocumentAsync(document, criteria));
            
        Assert.Contains("Failed to generate summary", exception.Message);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WithDocumentServiceFailure_ShouldPropagate()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var criteria = new List<GradingCriteria>
        {
            new()
            { 
                Id = Guid.NewGuid(), 
                Title = "Clarity", 
                Description = "Document should be clear", 
                MaxGrade = 100,
                Weight = 1.0f
            }
        };

        // Setup document service to fail
        _documentServiceMock.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Document service failure"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _aiService.AnalyzeDocumentAsync(document, criteria));
            
        Assert.Contains("Document service failure", exception.Message);
    }

    private class TestResult<T>(bool isSuccess, T? data, string? error)
        : Result<T>(isSuccess, data, error);
}