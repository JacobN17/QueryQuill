using QueryQuill.Domain.Enums;

namespace QueryQuill.Domain.Models;

public sealed record EvaluationMetric
{
    public EvaluationMetricType MetricType { get; init; }
    public double Score { get; init; }
    public string? Reason { get; init; }
    public bool IsPassing { get; init; } = true;
    public DateTimeOffset EvaluatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public EvaluationMetric()
    {
    }

    public EvaluationMetric(
        EvaluationMetricType metricType,
        double score,
        string? reason = null,
        bool isPassing = true,
        DateTimeOffset? evaluatedAtUtc = null)
    {
        MetricType = metricType;
        Score = score;
        Reason = reason;
        IsPassing = isPassing;
        EvaluatedAtUtc = evaluatedAtUtc ?? DateTimeOffset.UtcNow;
    }
}
