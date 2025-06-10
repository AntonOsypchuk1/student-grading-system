using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SGS.SemanticAnalysis.Helpers;
using SGS.SemanticAnalysis.Kernels;
using SGS.SemanticAnalysis.Services.Bases;
using SGS.SemanticAnalysis.Structures.PromptRequests;
using SGS.SemanticAnalysis.Structures.PromptResponses;

namespace SGS.SemanticAnalysis.Services;

public class GenerateSummaryService(SemanticKernelFactory semanticKernelFactory, IPromptBuilder promptBuilder)
    : BasePromptService, IPromptService<GenerateSummaryRequest, GenerateSummaryResponse>
{
    protected override string GetYamlFileName() => semanticKernelFactory.PromptLanguage == "en"
        ? "GenerateSummary.prompt.en.yaml"
        : "GenerateSummary.prompt.ua.yaml";
    
    public async Task<GenerateSummaryResponse?> ExecuteAsync(GenerateSummaryRequest request, CancellationToken cancellationToken = default)
    {
        var kernel = semanticKernelFactory.CreateBasic();
        var kernelFunction = kernel.CreateFunctionFromPromptYaml(
            PromptYaml,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        var input = promptBuilder.BuildSummaryInputs(request);

        var functionResult = await kernelFunction.InvokeAsync(kernel, input, cancellationToken);
        var result = functionResult.GetValue<string>();

        return DeserializeJsonFromResponse<GenerateSummaryResponse?>(result);
    }
}