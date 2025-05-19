using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SGS.SemanticAnalysis.Helpers;
using SGS.SemanticAnalysis.Kernels;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;

namespace SGS.SemanticAnalysis.Services;

public class EvaluateCriterionService(SemanticKernelFactory semanticKernelFactory, IPromptBuilder promptBuilder) 
    : BasePromptService, IPromptService<EvaluateCriteriaRequest, EvaluateCriteriaResponse>
{
    protected override string GetYamlFileName() => "EvaluateCriterion.prompt.yaml";
    
    public async Task<EvaluateCriteriaResponse?> ExecuteAsync(EvaluateCriteriaRequest request, CancellationToken cancellationToken = default)
    {
        var kernel = semanticKernelFactory.CreateBasic();
        var kernelFunction = kernel.CreateFunctionFromPromptYaml(
            PromptYaml,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        var input = promptBuilder.BuildCriterionInputs(request);

        var functionResult = await kernelFunction.InvokeAsync(kernel, input, cancellationToken);
        var result = functionResult.GetValue<string>();

        return DeserializeJsonFromResponse<EvaluateCriteriaResponse?>(result);
    }
}