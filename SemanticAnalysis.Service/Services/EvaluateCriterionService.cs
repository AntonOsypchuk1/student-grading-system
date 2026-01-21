using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticAnalysis.Service.Helpers;
using SemanticAnalysis.Service.Kernels;
using SemanticAnalysis.Service.Services.Bases;
using SemanticAnalysis.Service.Structures.PromptResponses;

namespace SemanticAnalysis.Service.Services;

public class EvaluateCriterionService(SemanticKernelFactory semanticKernelFactory, IPromptBuilder promptBuilder)
    : BasePromptService, IPromptService<Structures.PromptRequests.EvaluateCriteriaRequest, Structures.PromptResponses.EvaluateCriteriaResponse>
{
    protected override string GetYamlFileName() => semanticKernelFactory.PromptLanguage == "en"
        ? "EvaluateCriterion.prompt.en.yaml"
        : "EvaluateCriterion.prompt.ua.yaml";

    public async Task<EvaluateCriteriaResponse?> ExecuteAsync(Structures.PromptRequests.EvaluateCriteriaRequest request, CancellationToken cancellationToken = default)
    {
        // var kernel = semanticKernelFactory.CreateBasic();
        // var kernelFunction = kernel.CreateFunctionFromPromptYaml(
        //     PromptYaml,
        //     promptTemplateFactory: new HandlebarsPromptTemplateFactory());
        //
        // var input = promptBuilder.BuildCriterionInputs(request);
        //
        // var functionResult = await kernelFunction.InvokeAsync(kernel, input, cancellationToken);
        // var result = functionResult.GetValue<string>();
        //
        // return DeserializeJsonFromResponse<Structures.PromptResponses.EvaluateCriteriaResponse?>(result);
        return new EvaluateCriteriaResponse();
    }
}
