using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Structures.PromptRequests;

namespace SGS.SemanticAnalysis.Helpers;

public abstract class PromptBuilder
{
    public static KernelArguments BuildCriterionInputs(EvaluateCriteriaRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"DocumentText", request.DocumentText},
            {"Criteria", request.Criteria}
        };
    }

    public static KernelArguments BuildSummaryInputs(GenerateSummaryRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"EvaluationResult", request.EvaluationResult},
            {"Score", request.Score}
        };
    }
}