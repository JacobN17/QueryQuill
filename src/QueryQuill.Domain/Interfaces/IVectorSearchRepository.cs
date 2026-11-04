using QueryQuill.Domain.Models;

namespace QueryQuill.Domain.Interfaces;

public interface IVectorSearchRepository
{
    Task<IReadOnlyList<ScoredChunk>> VectorSearchAsync(
        ReadOnlyMemory<float> embedding,
        int limit = 5,
        float minSimilarity = 0.0f,
        Guid? documentId = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ScoredChunk>> LexicalSearchAsync(
        string queryText,
        int limit = 5,
        Guid? documentId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScoredChunk>> HybridSearchAsync(
        string queryText,
        ReadOnlyMemory<float> embedding,
        int limit = 5,
        float minSimilarity = 0.0f,
        Guid? documentId = null,
        CancellationToken cancellationToken = default
    );
}
