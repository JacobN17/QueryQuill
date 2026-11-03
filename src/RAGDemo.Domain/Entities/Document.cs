using RAGDemo.Domain.Enums;

namespace RAGDemo.Domain.Entities;

public sealed class Document
{
    private readonly List<DocumentChunk> _chunks = [];

    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string SourceUri { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/plain";
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public int ChunkCount { get; set; }
    public long TotalTokens { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public IReadOnlyList<DocumentChunk> Chunks => _chunks;

    public void MarkAsProcessing()
    {
        Status = DocumentStatus.Processing;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsIndexed(int chunkCount, long totalTokens)
    {
        Status = DocumentStatus.Indexed;
        ChunkCount = chunkCount;
        TotalTokens = totalTokens;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = DocumentStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
