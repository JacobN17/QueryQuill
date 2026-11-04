namespace QueryQuill.Domain.Models;

public sealed record RagResponse
{
    public string Query { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public IReadOnlyList<ScoredChunk> Citations { get; init; } = [];
    public TimeSpan Latency { get; init; } = TimeSpan.Zero;
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens => PromptTokens + CompletionTokens;
    public IReadOnlyList<EvaluationMetric> EvaluationMetrics { get; init; } = [];
}
