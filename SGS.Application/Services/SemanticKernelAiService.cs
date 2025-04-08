using SGS.Application.Interfaces;

namespace SGS.Application.Services;

public class SemanticKernelAiService : IAIService
{
    public async Task<(string summary, int generatedGrade)> AnalyzeDocumentAsync(string filePath,
        CancellationToken cancellationToken = default)
    {
        // Simulate reading the file, processing with AI, and generating a summary/grade.
        // In a real implementation, you might load the file, extract text, and then call your AI model.
        await Task.Delay(500, cancellationToken); // simulate processing delay

        // For this example, return a sample summary and grade.
        var summary = "The document meets the criteria with minor errors.";
        var generatedGrade = 85;
        return (summary, generatedGrade);
    }
}