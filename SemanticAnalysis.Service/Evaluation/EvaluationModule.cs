using SemanticAnalysis.Service.Evaluation.Providers;
using SemanticAnalysis.Service.Prompts;

namespace SemanticAnalysis.Service.Evaluation;

public static class EvaluationModule
{
    public static IServiceCollection AddEvaluation(this IServiceCollection services)
    {
        services.AddHttpClient("GradingModel", client => client.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<IStructuredPromptRunner, SemanticKernelPromptRunner>();
        services.AddScoped<IGradingEvidenceProvider, LlmRubricEvidenceProvider>();
        services.AddScoped<IGradingEvidenceProvider, LexicalConceptEvidenceProvider>();
        services.AddScoped<IGradeFusionStrategy, AdaptiveGradeFusion>();
        services.AddScoped<IRiskEstimator, HeuristicRiskEstimator>();
        services.AddScoped<IEvaluationPipeline, OpenAnswerEvaluationPipeline>();
        services.AddScoped<EvaluationDispatcher>();
        return services;
    }
}
