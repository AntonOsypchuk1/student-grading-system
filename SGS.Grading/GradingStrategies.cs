namespace SGS.Grading;

// Pure, versioned rules shared by the service and the offline benchmark.
public static class GradingStrategies
{
    public const string LlmOnly = "llm-only-v1";
    public const string LexicalOnly = "lexical-only-v1";
    public const string FixedHybrid = "fixed-hybrid-v1";
    public const string LegacyHybrid = "open-answer-v1";
    public const string AgreementHybrid = "agreement-hybrid-v1";
    public const string CriterionWise = "llm-criterionwise-v1";
    public const string LlmSource = "llm-rubric-v1";
    public const string CriterionWiseSource = "llm-criterionwise-evidence-v1";
    public const string LexicalSource = "lexical-concepts-v1";

    public static IReadOnlyList<string> ReplayMethods { get; } =
        Array.AsReadOnly([LlmOnly, LexicalOnly, FixedHybrid, LegacyHybrid, AgreementHybrid]);
    public static IReadOnlyList<string> All { get; } =
        Array.AsReadOnly([.. ReplayMethods, CriterionWise]);

    public static IReadOnlyList<string> RequiredSources(string method) => method switch
    {
        LlmOnly => [LlmSource],
        CriterionWise => [CriterionWiseSource],
        LexicalOnly => [LexicalSource],
        FixedHybrid or LegacyHybrid or AgreementHybrid => [LlmSource, LexicalSource],
        _ => throw new ArgumentException("Unsupported grading strategy.", nameof(method))
    };

    public static FusionResult Fuse(string method, double? llm, double? lexical)
    {
        var sources = RequiredSources(method);
        Validate(llm, sources.Contains(LlmSource) || sources.Contains(CriterionWiseSource), nameof(llm));
        Validate(lexical, sources.Contains(LexicalSource), nameof(lexical));
        if (method is LlmOnly or CriterionWise) return new(llm!.Value, 1, 0);
        if (method == LexicalOnly) return new(lexical!.Value, 0, 0);

        var difference = Math.Abs(llm!.Value - lexical!.Value);
        var weight = method switch
        {
            FixedHybrid => 0.8,
            LegacyHybrid => 0.8 - 0.3 * difference,
            // Lexical overlap is a weak proxy for correctness. Its influence
            // decreases as signals diverge; strong disagreement is a review
            // signal, not justification to trust keywords over semantic evidence.
            AgreementHybrid => 0.8 + 0.2 * Math.Min(difference / 0.25, 1),
            _ => throw new ArgumentException("Unsupported grading strategy.")
        };
        return new(weight * llm.Value + (1 - weight) * lexical.Value, weight, difference);
    }

    public static StrategyRisk Assess(string method, IReadOnlyList<CriterionRisk> criteria, bool partialExtraction)
    {
        RequiredSources(method);
        if (criteria.Count == 0)
        {
            throw new ArgumentException("Criteria are required.");
        }
        
        var disagreement = criteria.Average(c => c.Disagreement);
        var missing = criteria.Any(c => c.MissingRequiredConcept);
        var retry = criteria.Any(c => c.HadRetry);
        var severe = criteria.Any(c => c.Disagreement >= 0.5);
        var factors = new List<string>();
        
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
        
        var indicator = Math.Clamp(0.6 * disagreement + (missing ? 0.4 : 0), 0, 1);
        var review = missing || retry || indicator >= 0.3 || severe;
        if (severe && !factors.Contains("signal_disagreement"))
        {
            factors.Add("signal_disagreement");
        }

        // Keep legacy risk semantics frozen for comparison.
        if (method != LegacyHybrid)
        {
            if (partialExtraction)
            {
                factors.Add("partial_extraction");
                indicator = Math.Max(indicator, 0.4);
                review = true;
            }
            if (method == LexicalOnly)
            {
                factors.Add("lexical_only_not_semantically_verified");
                indicator = Math.Max(indicator, 0.4);
                review = true;
            }
            if (retry) indicator = Math.Max(indicator, 0.3);
            if (method == AgreementHybrid && criteria.Any(c => c.Disagreement >= 0.25))
            {
                factors.Add("agreement_gate");
                indicator = Math.Max(indicator, 0.3);
                review = true;
            }
        }
        
        return new(indicator, review, factors);
    }

    private static void Validate(double? value, bool required, string name)
    {
        if ((required && !value.HasValue) ||
            (value.HasValue && (!double.IsFinite(value.Value) || value.Value < 0 || value.Value > 1)))
        {
            throw new ArgumentException("Missing or invalid normalized evidence score.", name);
        }
    }
}

public sealed record FusionResult(double Score, double LlmWeight, double Disagreement);
public sealed record CriterionRisk(double Disagreement, bool MissingRequiredConcept, bool HadRetry);
public sealed record StrategyRisk(double Indicator, bool RequiresReview, IReadOnlyList<string> Factors);
