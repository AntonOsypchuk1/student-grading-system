using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SemanticAnalysis.Service.Helpers;
using SemanticAnalysis.Service.Kernels;
using SemanticAnalysis.Service.Services.Bases;
using SemanticAnalysis.Service.Structures.PromptResponses;

namespace SemanticAnalysis.Service.Services;

public class GenerateSummaryService(SemanticKernelFactory semanticKernelFactory, IPromptBuilder promptBuilder)
    : BasePromptService, IPromptService<Structures.PromptRequests.GenerateSummaryRequest, Structures.PromptResponses.GenerateSummaryResponse>
{
    protected override string GetYamlFileName() => semanticKernelFactory.PromptLanguage == "en"
        ? "GenerateSummary.prompt.en.yaml"
        : "GenerateSummary.prompt.ua.yaml";

    public async Task<GenerateSummaryResponse?> ExecuteAsync(Structures.PromptRequests.GenerateSummaryRequest request, CancellationToken cancellationToken = default)
    {
        // var kernel = semanticKernelFactory.CreateBasic();
        // var kernelFunction = kernel.CreateFunctionFromPromptYaml(
        //     PromptYaml,
        //     promptTemplateFactory: new HandlebarsPromptTemplateFactory());
        //
        // var input = promptBuilder.BuildSummaryInputs(request);
        //
        // var functionResult = await kernelFunction.InvokeAsync(kernel, input, cancellationToken);
        // var result = functionResult.GetValue<string>();
        //
        // return DeserializeJsonFromResponse<Structures.PromptResponses.GenerateSummaryResponse?>(result);
        return new GenerateSummaryResponse();
    }
}
