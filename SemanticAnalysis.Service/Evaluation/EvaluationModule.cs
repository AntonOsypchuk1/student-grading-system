using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Evaluation.Abstractions;
using SemanticAnalysis.Service.Prompts.Abstractions;
using SemanticAnalysis.Service.Prompts;
using SGS.Grading;

namespace SemanticAnalysis.Service.Evaluation;

public static class EvaluationModule
{
    public static IServiceCollection AddEvaluation(this IServiceCollection services)
    {
        services.AddHttpClient("GradingModel", client => client.Timeout = TimeSpan.FromSeconds(60));
        services.AddSingleton<ISemanticKernelFactory, SemanticKernelFactory>();
        services.AddScoped<IStructuredPromptRunner, SemanticKernelPromptRunner>();
        services.AddScoped<IGradingEvidenceProvider, LlmRubricEvidenceProvider>();
        services.AddScoped<IGradingEvidenceProvider, LexicalConceptEvidenceProvider>();
        services.AddScoped<IGradingEvidenceProvider, CriterionWiseLlmEvidenceProvider>();
        services.AddScoped<IGradeFusionStrategy, AdaptiveGradeFusion>();
        services.AddScoped<IRiskEstimator, HeuristicRiskEstimator>();
        foreach (var method in GradingStrategies.All)
        {
            services.AddScoped<IEvaluationPipeline>(sp => new OpenAnswerEvaluationPipeline(
                sp.GetServices<IGradingEvidenceProvider>(),
                method == GradingStrategies.LegacyHybrid
                    ? sp.GetRequiredService<IGradeFusionStrategy>()
                    : new ComparativeGradeFusion(method),
                sp.GetRequiredService<IRiskEstimator>(),
                sp.GetRequiredService<ILogger<OpenAnswerEvaluationPipeline>>(), method));
        }
        services.AddScoped<EvaluationDispatcher>();
        return services;
    }
}
