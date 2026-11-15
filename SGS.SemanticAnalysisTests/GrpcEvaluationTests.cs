using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Protos;
using GrpcService = SemanticAnalysis.Service.Protos.SemanticAnalysisService;

namespace SGS.SemanticAnalysisTests;

public sealed class GrpcEvaluationTests
{
    private static WebApplicationFactory<Program> Factory(IStructuredPromptRunner runner)
        => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Auth:ApiKey", "test-only-key");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IStructuredPromptRunner>();
                services.AddSingleton(runner);
            });
        });

    private static GrpcService.SemanticAnalysisServiceClient Client(WebApplicationFactory<Program> factory)
        => new(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions { HttpHandler = factory.Server.CreateHandler() }));
    private static Metadata Headers() => new() { { "x-api-key", "test-only-key" } };
    private static EvaluateDocumentRequest Request() => new()
    {
        DocumentText = "Independent encoding and vector similarity.", Question = "Explain a bi-encoder.",
        Criteria = { new EvaluationCriterion
        {
            Id = "meaning", Description = "Explain the mechanism.", MaxScore = 10, Weight = 1,
            Concepts = { new ExpectedConcept { Id = "encoding", Alternatives = { "independent encoding" }, Required = true } }
        } }
    };

    [Fact]
    public async Task RealGrpcTransportReturnsStructuredEvaluation()
    {
        using var factory = Factory(new FakePromptRunner(10));
        var response = await Client(factory).EvaluateDocumentAsync(Request(), Headers());
        Assert.True(response.HasScore);
        Assert.Equal(100, response.Score);
        Assert.False(response.RequiresTeacherReview);
        Assert.Equal("open-answer-v1", response.MethodVersion);
        Assert.Equal(2, response.Criteria.Single().Evidence.Count);
    }

    [Fact]
    public async Task RealGrpcTransportOmitsGradeOnModelFailure()
    {
        using var factory = Factory(new FakePromptRunner(10) { Failure = new HttpRequestException() });
        var response = await Client(factory).EvaluateDocumentAsync(Request(), Headers());
        Assert.False(response.HasScore);
        Assert.True(response.RequiresTeacherReview);
        Assert.Equal(1, response.RiskIndicator);
    }

    [Fact]
    public async Task InvalidRequestReturnsInvalidArgument()
    {
        using var factory = Factory(new FakePromptRunner(10));
        var error = await Assert.ThrowsAsync<RpcException>(async () =>
            await Client(factory).EvaluateDocumentAsync(new EvaluateDocumentRequest(), Headers()));
        Assert.Equal(StatusCode.InvalidArgument, error.StatusCode);
    }

    [Fact]
    public async Task MissingKeyReturnsUnauthenticated()
    {
        using var factory = Factory(new FakePromptRunner(10));
        var error = await Assert.ThrowsAsync<RpcException>(async () => await Client(factory).EvaluateDocumentAsync(Request()));
        Assert.Equal(StatusCode.Unauthenticated, error.StatusCode);
    }

    [Fact]
    public async Task LegacyCriteriaRpcKeepsItsScoreScale()
    {
        using var factory = Factory(new FakePromptRunner(80));
        var response = await Client(factory).EvaluateCriteriaAsync(new EvaluateCriteriaRequest
        {
            DocumentText = "Some student answer.", Criteria = { new CriterionInfoRequest
            { Id = "x", Description = "Explain the mechanism.", Title = "Meaning", MaxGrade = 10, Weight = 1 } }
        }, Headers());
        Assert.Equal(80, response.CriteriaResults.Single().Score);
        Assert.Equal("Meaning", response.CriteriaResults.Single().Title);
    }

    [Fact]
    public async Task LegacySummaryRpcReturnsSummaryAndFeedback()
    {
        using var factory = Factory(new FakePromptRunner(10));
        var response = await Client(factory).GenerateSummaryAsync(new GenerateSummaryRequest
        {
            EvaluationResult = "Clear mechanism explanation.", Score = 8, MaxScore = 10
        }, Headers());
        Assert.Equal("Deterministic summary.", response.Summary);
        Assert.Single(response.Strengths);
    }
}
