namespace SGS.SemanticAnalysis.Structures.PromptRequests;

/// <summary>
/// Information about a single criterion to be evaluated
/// </summary>
public class CriterionInfo
{
    /// <summary>
    /// Unique identifier for the criterion
    /// </summary>
    public string Id { get; set; } = string.Empty;
        
    /// <summary>
    /// Title of the criterion
    /// </summary>
    public string Title { get; set; } = string.Empty;
        
    /// <summary>
    /// Detailed description of the criterion requirements
    /// </summary>
    public string Description { get; set; } = string.Empty;
        
    /// <summary>
    /// Maximum possible grade for this criterion
    /// </summary>
    public double MaxGrade { get; set; } = 100;
        
    /// <summary>
    /// Weight of this criterion in the overall assessment (0-1)
    /// </summary>
    public double Weight { get; set; } = 1.0;
}
