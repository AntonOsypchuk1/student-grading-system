using Microsoft.SemanticKernel;
using SGS.Domain.Entities;

namespace SGS.SemanticAnalysis.Helpers;

public interface IPromptBuilder
{
    /// <summary>
    /// Builds KernelArguments for EvaluateCriterion from a Criterion and full document text.
    /// </summary>
    KernelArguments BuildCriterionInputs(GradingCriteria criteria, string documentText);

    /// <summary>
    /// Builds KernelArguments for aggregate and summary steps.
    /// </summary>
    KernelArguments BuildAggregateInputs(string evaluationResult, double weight);

    KernelArguments BuildSummaryInputs(string evaluationResult, double score);
}