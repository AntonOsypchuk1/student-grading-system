using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGS.SemanticAnalysis.Helpers;
using SGS.SemanticAnalysis.Kernels;
using SGS.SemanticAnalysis.RetryPolicies;
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
        services.Configure<SemanticKernelSettings>(options =>
        {
            configuration.GetSection("SemanticKernelSettings").Bind(options);
        });

        services.AddScoped<SemanticKernelFactory>();
        services.AddScoped<SemanticKernelBuilder>();

        services.AddScoped<ISemanticKernelRetryPolicy, SemanticKernelRetryPolicy>();

        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IPromptServiceExecutor, PromptServiceExecutor>();
        services.AddScoped<IPromptService<EvaluateCriteriaRequest, EvaluateCriteriaResponse>, EvaluateCriterionService>();
        services.AddScoped<IPromptService<GenerateSummaryRequest, GenerateSummaryResponse>, GenerateSummaryService>();
    }
}