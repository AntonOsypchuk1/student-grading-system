using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticAnalysis.Service.Helpers;

public class PromptBuilder : IPromptBuilder
{
    public KernelArguments BuildCriterionInputs(Structures.PromptRequests.EvaluateCriteriaRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"DocumentText", request.DocumentText},
            {"Criteria", request.Criteria}
        };
    }

    public KernelArguments BuildSummaryInputs(Structures.PromptRequests.GenerateSummaryRequest request)
    {
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = null };
        return new KernelArguments(settings)
        {
            {"EvaluationResult", request.EvaluationResult},
            {"Score", request.Score}
        };
    }
}
