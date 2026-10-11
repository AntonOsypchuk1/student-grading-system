namespace SemanticAnalysis.Service.Evaluation;

public sealed class HeuristicRiskEstimator : IRiskEstimator
{
    public RiskAssessment Estimate(IReadOnlyList<CriterionEvaluation> criteria)
    {
        var factors = new List<string>();
        var disagreement = criteria.Average(x => x.Disagreement);
        var missing = criteria.Any(c => c.Evidence.Any(e => e.MissingRequiredConcepts.Count > 0));
        var retry = criteria.Any(c => c.Evidence.Any(e => e.HadRetry));
        
        if (disagreement > 0.25)
        {
            factors.Add("signal_disagreement");
        }
        if (missing)
        {
            factors.Add("missing_required_concept");
        }
        if (retry)
        {
            factors.Add("prompt_retry");
        }
        
        var risk = Math.Clamp(0.6 * disagreement + (missing ? 0.4 : 0), 0, 1);
        
        // Do not hide a severely disputed criterion inside an otherwise high grade.
        var review = missing || retry || risk >= 0.3 || criteria.Any(c => c.Disagreement >= 0.5);

        if (criteria.Any(c => c.Disagreement >= 0.5) && !factors.Contains("signal_disagreement"))
        {
            factors.Add("signal_disagreement");
        }
        
        return new RiskAssessment(risk, review, factors);
    }
}
