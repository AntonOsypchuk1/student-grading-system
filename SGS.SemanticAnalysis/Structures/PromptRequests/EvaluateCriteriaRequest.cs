namespace SGS.SemanticAnalysis.Structures.PromptRequests;

/// <summary>
/// Request model for batch evaluation of all criteria for a document
/// </summary>
public class EvaluateCriteriaRequest
{
    /// <summary>
    /// The full text of the student's document
    /// </summary>
    public string DocumentText { get; set; } = string.Empty;
        
    /// <summary>
    /// Collection of criteria to evaluate against
    /// </summary>
    public IEnumerable<CriterionInfo> Criteria { get; set; } = new List<CriterionInfo>();
}
