using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.SemanticAnalysis.Kernels;
using SGS.SemanticAnalysis.Services;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Settings;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;

namespace SGS.SemanticAnalysis;

public static class SemanticAnalysisModule
{
    public static void AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SemanticKernelSettings>(_ => configuration.GetSection(nameof(SemanticKernelSettings)));

        services.AddScoped<SemanticKernelFactory>();
        services.AddScoped<SemanticKernelBuilder>();

        services.AddScoped<IPromptServiceExecutor, PromptServiceExecutor>();
        services.AddScoped<IPromptService<EvaluateCriteriaRequest, EvaluateCriteriaResponse>, EvaluateCriterionService>();
        services.AddScoped<IPromptService<GenerateSummaryRequest, GenerateSummaryResponse>, GenerateSummaryService>();
    }
}