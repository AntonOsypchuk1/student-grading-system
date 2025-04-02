using SGS.Application.Interfaces;

namespace SGS.Application.Services;

public class SemanticKernelAIService : IAIService
{
    public async Task<(string summary, int generatedGrade)> AnalyzeDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Simulate reading the file, processing with AI, and generating a summary/grade.
        // In a real implementation, you might load the file, extract text, and then call your AI model.
        await Task.Delay(500, cancellationToken); // simulate processing delay

        // For this example, return a sample summary and grade.
        string summary = "The document meets the criteria with minor errors.";
        int generatedGrade = 85;
        return (summary, generatedGrade);
    }
}