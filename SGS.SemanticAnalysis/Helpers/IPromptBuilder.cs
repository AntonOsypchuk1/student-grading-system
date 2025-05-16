using Microsoft.SemanticKernel;
using SGS.Domain.Entities;
using SGS.SemanticAnalysis.Structures.PromptRequests;

namespace SGS.SemanticAnalysis.Helpers;

public interface IPromptBuilder
{
    KernelArguments BuildCriterionInputs(EvaluateCriteriaRequest request);

    KernelArguments BuildSummaryInputs(GenerateSummaryRequest request);
}