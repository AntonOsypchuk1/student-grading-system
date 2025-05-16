using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Structures.PromptRequests;

namespace SGS.SemanticAnalysis.Helpers;

public class PromptBuilder : IPromptBuilder
{
    public KernelArguments BuildCriterionInputs(EvaluateCriteriaRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"DocumentText", request.DocumentText},
            {"Criteria", request.Criteria}
        };
    }

    public KernelArguments BuildSummaryInputs(GenerateSummaryRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"EvaluationResult", request.EvaluationResult},
            {"Score", request.Score}
        };
    }
}