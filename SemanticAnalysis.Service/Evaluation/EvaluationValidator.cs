namespace SemanticAnalysis.Service.Evaluation;

public static class EvaluationValidator
{
    public static void Validate(EvaluationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentText) || string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Document text and question are required.");
        if (request.DocumentText.Length > 100_000 || request.Question.Length > 10_000)
            throw new ArgumentException("Document or question exceeds the supported text limit.");
        if (!double.IsFinite(request.MaxScore) || request.MaxScore <= 0)
            throw new ArgumentException("MaxScore must be finite and positive.");
        if (request.Criteria.Count is < 1 or > 50 ||
            request.Criteria.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != request.Criteria.Count)
            throw new ArgumentException("Provide 1–50 criteria with unique IDs.");
        foreach (var c in request.Criteria)
        {
            if (string.IsNullOrWhiteSpace(c.Id) || string.IsNullOrWhiteSpace(c.Description) ||
                !double.IsFinite(c.MaxScore) || c.MaxScore <= 0 ||
                !double.IsFinite(c.Weight) || c.Weight <= 0)
                throw new ArgumentException("Each criterion needs an ID, description and positive finite scale/weight.");
            if (c.Concepts.Count is < 1 or > 100 ||
                c.Concepts.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != c.Concepts.Count ||
                c.Concepts.Any(x => string.IsNullOrWhiteSpace(x.Id) || x.Alternatives.Count is < 1 or > 20 ||
                    x.Alternatives.Any(string.IsNullOrWhiteSpace)))
                throw new ArgumentException("Each criterion needs unique concepts with nonempty lexical alternatives.");
        }
        if (!double.IsFinite(request.Criteria.Sum(c => c.Weight)))
            throw new ArgumentException("Total criterion weight is too large.");
    }
}
