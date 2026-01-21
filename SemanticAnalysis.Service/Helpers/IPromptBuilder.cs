using Microsoft.SemanticKernel;

namespace SemanticAnalysis.Service.Helpers;

public interface IPromptBuilder
{
    KernelArguments BuildCriterionInputs(Structures.PromptRequests.EvaluateCriteriaRequest request);

    KernelArguments BuildSummaryInputs(Structures.PromptRequests.GenerateSummaryRequest request);
}
