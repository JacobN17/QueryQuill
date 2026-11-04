using QueryQuill.Domain.ValueObjects;

namespace QueryQuill.Domain.Entities;

public sealed record DocumentChunk
{
    public long Id { get; init; }
    public Guid DocumentId { get; init; }
    public int ChunkIndex { get; init; }
    public string Content { get; init; } = string.Empty;
    public int TokenCount { get; init; }
    public ReadOnlyMemory<float>? Embedding { get; init; }
    public ChunkMetadata Metadata { get; init; } = ChunkMetadata.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DocumentChunk()
    {
    }

    public DocumentChunk(
        long id,
        string content,
        string metadata = "")
    {
        Id = id;
        Content = content;
        Metadata = ChunkMetadata.FromJson(metadata);
    }

    public DocumentChunk(
        long id,
        Guid documentId,
        int chunkIndex,
        string content,
        int tokenCount,
        ReadOnlyMemory<float>? embedding = null,
        ChunkMetadata? metadata = null
    )
    {
        Id = id;
        DocumentId = documentId;
        ChunkIndex = chunkIndex;
        Content = content;
        TokenCount = tokenCount;
        Embedding = embedding;
        Metadata = metadata ?? ChunkMetadata.Empty;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
