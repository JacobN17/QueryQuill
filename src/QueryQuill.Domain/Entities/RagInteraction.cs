using QueryQuill.Domain.Models;

namespace QueryQuill.Domain.Entities;

public sealed class RagInteraction
{
    private readonly List<EvaluationMetric> _evaluationMetrics = [];

    public Guid Id { get; init; } = Guid.NewGuid();
    public string SessionId { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public string GeneratedResponse { get; init; } = string.Empty;
    public IReadOnlyList<long> RetrievedChunkIds { get; init; } = [];
    public TimeSpan Latency { get; init; } = TimeSpan.Zero;
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<EvaluationMetric> EvaluationMetrics => _evaluationMetrics.AsReadOnly();

    public void AddEvaluationMetric(EvaluationMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);
        _evaluationMetrics.Add(metric);
    }

    public void AddEvaluationMetrics(IEnumerable<EvaluationMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        _evaluationMetrics.AddRange(metrics);
    }
}