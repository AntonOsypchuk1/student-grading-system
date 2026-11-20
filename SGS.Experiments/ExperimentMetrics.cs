namespace SGS.Experiments;

public sealed record MetricGroup(ReferenceKind ReferenceKind, WorkType? WorkType, int Cases, int Attempts,
    int Scored, int Failed, int LabeledScored, double FailureRate, double ReviewRate, double AutomaticCoverage,
    double? Mae, double? Rmse, double? Within10Points, double? Bias, double? OverestimationRate, double? UnderestimationRate,
    double? MeanLatencyMs, double? MeanRepeatedScoreRange, int RepeatedCases);

public static class ExperimentMetrics
{
    public static IReadOnlyList<MetricGroup> Calculate(IReadOnlyList<EvaluationAttempt> attempts) =>
        attempts.GroupBy(a => a.ReferenceKind).SelectMany(group => new[] { Summarize(group.Key, null, group.ToArray()) }
            .Concat(group.GroupBy(a => a.WorkType).Select(type => Summarize(group.Key, type.Key, type.ToArray())))).ToArray();

    private static MetricGroup Summarize(ReferenceKind kind, WorkType? workType, EvaluationAttempt[] attempts)
    {
        var scored = attempts.Where(a => a.Status == AttemptStatus.Scored && a.Score.HasValue).ToArray();
        // Normalize to percentage points (0–100), independent of each original grading scale.
        var errors = scored.Where(a => a.ReferenceScore.HasValue)
            .Select(a => (a.Score!.Value - a.ReferenceScore!.Value) / a.MaxScore * 100).ToArray();
        var ranges = scored.GroupBy(a => a.CaseId).Where(g => g.Count() >= 2)
            .Select(g => g.Max(a => a.Score!.Value / a.MaxScore * 100) - g.Min(a => a.Score!.Value / a.MaxScore * 100)).ToArray();
        return new(kind, workType, attempts.Select(a => a.CaseId).Distinct().Count(), attempts.Length, scored.Length,
            attempts.Length - scored.Length, errors.Length,
            (double)(attempts.Length - scored.Length) / attempts.Length,
            (double)attempts.Count(a => a.RequiresReview) / attempts.Length,
            (double)scored.Count(a => !a.RequiresReview) / attempts.Length,
            Mean(errors.Select(Math.Abs)), errors.Length == 0 ? null : Math.Sqrt(errors.Average(e => e * e)),
            Fraction(errors, e => Math.Abs(e) <= 10 + 1e-9), Mean(errors),
            Fraction(errors, e => e > 1e-9), Fraction(errors, e => e < -1e-9),
            Mean(attempts.Where(a => a.LatencyMs.HasValue).Select(a => a.LatencyMs!.Value)), Mean(ranges), ranges.Length);
    }

    private static double? Mean(IEnumerable<double> values) => values.Select(v => (double?)v).Average();
    private static double? Fraction(double[] values, Func<double, bool> predicate) =>
        values.Length == 0 ? null : (double)values.Count(predicate) / values.Length;
}
