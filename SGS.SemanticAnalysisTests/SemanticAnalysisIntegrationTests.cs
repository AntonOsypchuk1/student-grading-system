using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SGS.Application.Interfaces;
using SGS.Application.Models;
using SGS.Application.Services.Analysis;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;
using SGS.SemanticAnalysisTests.Bases;
using SGS.SemanticAnalysisTests.TestData;
using Xunit.Abstractions;

namespace SGS.SemanticAnalysisTests;

public class SemanticAnalysisIntegrationTests : IClassFixture<SemanticAnalysisTestFixture>
{
    private readonly SemanticAnalysisTestFixture _fixture;
    private readonly IPromptServiceExecutor _promptServiceExecutor;
    private readonly ILogger<SemanticAnalysisIntegrationTests> _logger;
    private readonly ITestOutputHelper _output;

    public SemanticAnalysisIntegrationTests(SemanticAnalysisTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        
        // Configure the fixture for this test run
        _fixture.ConfigureForTest(output);
        
        // Get services from the fixture
        _promptServiceExecutor = _fixture.GetService<IPromptServiceExecutor>();
        _logger = _fixture.GetService<ILogger<SemanticAnalysisIntegrationTests>>();
    }

    [Fact]
    public async Task EvaluateCriteriaService_ShouldEvaluateSimpleDocument()
    {
        // Arrange
        var request = new EvaluateCriteriaRequest
        {
            DocumentText = SemanticAnalysisTestData.Documents.WellWrittenDocument,
            Criteria = SemanticAnalysisTestData.Criteria.BasicDocumentCriteria
        };

        // Act
        var stopWatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting criteria evaluation test");
        var response = await _promptServiceExecutor.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            request, CancellationToken.None);
        stopWatch.Stop();

        // Log the response
        _output.WriteLine($"Criteria evaluation took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine($"Evaluation response received with {response?.CriteriaResults?.Count ?? 0} results");
        if (response?.CriteriaResults != null)
        {
            foreach (var result in response.CriteriaResults)
            {
                _output.WriteLine($"Criterion: {result.Title}, Score: {result.Score}/100");
                _output.WriteLine($"Justification: {result.Justification}");
                _output.WriteLine("Strengths:");
                foreach (var strength in result.Strengths)
                {
                    _output.WriteLine($"- {strength}");
                }
                _output.WriteLine("Improvements:");
                foreach (var improvement in result.Improvements)
                {
                    _output.WriteLine($"- {improvement}");
                }
                _output.WriteLine("------");
            }
        }

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.CriteriaResults);
        Assert.Equal(request.Criteria.Count(), response.CriteriaResults.Count);
            
        foreach (var result in response.CriteriaResults)
        {
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Title);
            Assert.InRange(result.Score, 0, 100);
            Assert.NotNull(result.Justification);
            Assert.NotNull(result.Strengths);
            Assert.NotNull(result.Improvements);
        }
    }

    [Fact]
    public async Task GenerateSummaryService_ShouldGenerateSummaryFromEvaluation()
    {
        // Arrange
        var request = new GenerateSummaryRequest
        {
            EvaluationResult = SemanticAnalysisTestData.EvaluationResults.SampleEvaluationDetails,
            Score = 82.2, // 85*0.6 + 78*0.4 = 82.2
            MaxScore = 100
        };

        // Act
        var stopWatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting summary generation test");
        var response = await _promptServiceExecutor.ExecuteAsync<GenerateSummaryRequest, GenerateSummaryResponse>(
            request, CancellationToken.None);
        stopWatch.Stop();

        // Log the response
        _output.WriteLine($"Summary generation took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine("Summary:");
        _output.WriteLine(response?.Summary ?? "No summary generated");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Summary);
        Assert.NotEmpty(response.Summary);
        Assert.NotEmpty(response.Improvements);
        Assert.NotEmpty(response.Strengths);

        // Check that the summary mentions the score
        // Assert.Contains("82", response.Summary);
    }

    [Fact]
    public async Task EvaluateCriteriaService_WithPoorQualityDocument_ShouldGiveLowerScores()
    {
        // Arrange
        var request = new EvaluateCriteriaRequest
        {
            DocumentText = SemanticAnalysisTestData.Documents.PoorQualityDocument,
            Criteria = SemanticAnalysisTestData.Criteria.GrammarAndSpellingCriteria
        };

        // Act
        var stopWatch = Stopwatch.StartNew();
        var response = await _promptServiceExecutor.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            request, CancellationToken.None);
        stopWatch.Stop();

        // Log
        _output.WriteLine($"Poor document evaluation took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine($"Poor document evaluation received with {response?.CriteriaResults.Count ?? 0} results");
        if (response?.CriteriaResults != null)
        {
            foreach (var result in response.CriteriaResults)
            {
                _output.WriteLine($"Criterion: {result.Title}, Score: {result.Score}/100");
            }
        }

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.CriteriaResults);
            
        // Validate that scores are lower than for a good document
        // We expect scores to be lower, but can't guarantee exact scores since it depends on the AI model
        foreach (var result in response.CriteriaResults)
        {
            // Check that scores are below 70 for poor quality document
            Assert.True(result.Score < 70, $"Expected score below 70 for poor document, got {result.Score} for {result.Title}");
                
            // Also verify that improvements list contains items
            Assert.True(result.Improvements.Count > 0, "Poor document should have suggested improvements");
        }
    }

    [Fact]
    public async Task FullWorkflow_DocumentAnalysis_EndToEnd()
    {
        // Create test scope to get a logger from the fixture
        using var scope = _fixture.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var aiServiceLogger = loggerFactory.CreateLogger<SemanticKernelAiService>();
        
        // Arrange - Set up a mock document service and the real analysis service
        var mockDocumentService = new Mock<IDocumentService>();
            
        var aiService = new SemanticKernelAiService(
            _promptServiceExecutor,
            mockDocumentService.Object,
            aiServiceLogger
        );

        // Create a test document and grading criteria
        var documentId = Guid.NewGuid();
        var document = new Document { Id = documentId };
        var criteria = SemanticAnalysisTestData.Criteria.FullAnalysisCriteria;

        // Setup mock to return our test document
        mockDocumentService.Setup(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult<string>(true, SemanticAnalysisTestData.Documents.ArtificialIntelligenceOverview, null));

        var stopWatch = Stopwatch.StartNew();
        // Act - Run the full analysis workflow
        _logger.LogInformation("Starting end-to-end document analysis test");
        var result = await aiService.AnalyzeDocumentAsync(document, criteria);
        stopWatch.Stop();
        
        // Log results
        _output.WriteLine($"Document analysis took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine($"Document analysis result:");
        _output.WriteLine($"Grade: {result.GeneratedGrade}/100");
        _output.WriteLine("Summary:");
        _output.WriteLine(result.Summary);

        // Assert
        Assert.NotNull(result.Summary);
        Assert.NotEmpty(result.Summary);
        Assert.InRange(result.GeneratedGrade, 0, 100); 

        // Verify method was called
        mockDocumentService.Verify(x => x.GetDocumentContentTextAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SpecializedCriteria_TechnicalDocument_ShouldEvaluateAppropriately()
    {
        // Arrange
        var request = new EvaluateCriteriaRequest
        {
            DocumentText = SemanticAnalysisTestData.Documents.TechnicalApiDocumentation,
            Criteria = SemanticAnalysisTestData.Criteria.TechnicalDocumentationCriteria
        };

        // Act
        var stopWatch = Stopwatch.StartNew();
        var response = await _promptServiceExecutor.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            request, CancellationToken.None);
        stopWatch.Stop();

        // Log
        _output.WriteLine($"Technical document evaluation took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine($"Technical document evaluation received with {response?.CriteriaResults?.Count ?? 0} results");
        if (response?.CriteriaResults != null)
        {
            foreach (var result in response.CriteriaResults)
            {
                _output.WriteLine($"Criterion: {result.Title}, Score: {result.Score}/100");
                _output.WriteLine($"Justification: {result.Justification}");
            }
        }

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.CriteriaResults);
        Assert.Equal(request.Criteria.Count(), response.CriteriaResults.Count);
            
        // Verify that the API documentation criterion was properly evaluated
        var apiDocsResult = response.CriteriaResults.FirstOrDefault(r => r.Title.Contains("API Documentation"));
        Assert.NotNull(apiDocsResult);
            
        // The response should include specific feedback about API documentation
        Assert.Contains("API", apiDocsResult.Justification, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("endpoint", apiDocsResult.Justification, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateCriteriaService_WithLongDocument_ShouldHandleEffectively()
    {
        // Arrange
        var request = new EvaluateCriteriaRequest
        {
            DocumentText = SemanticAnalysisTestData.Documents.LongDocument,
            Criteria = SemanticAnalysisTestData.Criteria.BasicDocumentCriteria
        };

        // Act
        var stopWatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting long document evaluation test");
        var response = await _promptServiceExecutor.ExecuteAsync<EvaluateCriteriaRequest, EvaluateCriteriaResponse>(
            request, CancellationToken.None);
        stopWatch.Stop();

        // Log
        _output.WriteLine($"Long document evaluation took {stopWatch.ElapsedMilliseconds / 1000} s");
        _output.WriteLine($"Long document evaluation received with {response?.CriteriaResults?.Count ?? 0} results");
        if (response?.CriteriaResults != null)
        {
            foreach (var result in response.CriteriaResults)
            {
                _output.WriteLine($"Criterion: {result.Title}, Score: {result.Score}/100");
            }
        }

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.CriteriaResults);
        Assert.Equal(request.Criteria.Count(), response.CriteriaResults.Count);
            
        // The longer document with proper structure should score reasonably well
        foreach (var result in response.CriteriaResults)
        {
            // A well-structured long document should score reasonably well
            Assert.True(result.Score >= 70, $"Expected score of at least 70 for well-structured long document, got {result.Score} for {result.Title}");
        }
    }
    
    private class TestResult<T>(bool isSuccess, T? data, string? error)
        : Result<T>(isSuccess, data, error);
}