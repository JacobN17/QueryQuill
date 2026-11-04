using QueryQuill.Domain.Enums;

namespace QueryQuill.Domain.Models;

public sealed record RagQuery
{
    public string QueryText { get; init; } = string.Empty;
    public ReadOnlyMemory<float>? QueryEmbedding { get; init; }
    public int TopK { get; init; } = 5;
    public float MinRelevanceScore { get; init; } = 0.0f;
    public SearchMode SearchMode { get; init; } = SearchMode.Hybrid;
    public Guid? DocumentIdFilter { get; init; }
    public IReadOnlyDictionary<string, string>? MetadataFilters { get; init; }
}
