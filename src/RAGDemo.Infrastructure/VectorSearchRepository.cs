using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Exceptions;
using RAGDemo.Domain.Interfaces;
using RAGDemo.Domain.Models;
using RAGDemo.Infrastructure.Data;
using RAGDemo.Infrastructure.Data.Configurations;

namespace RAGDemo.Infrastructure;

/// <summary>
/// Entity Framework Core repository implementing vector, lexical, and hybrid search operations over document chunks.
/// </summary>
public sealed class VectorSearchRepository(ApplicationDbContext dbContext) : IVectorSearchRepository
{
    private const float RrfK = 60.0f;

    /// <summary>
    /// Performs dense vector similarity search using PostgreSQL pgvector cosine distance.
    /// </summary>
    public async Task<IReadOnlyList<ScoredChunk>> VectorSearchAsync(
        ReadOnlyMemory<float> embedding,
        int limit = 5,
        float minSimilarity = 0.0f,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        if (embedding.Length != DocumentChunkConfiguration.DefaultEmbeddingDimension)
        {
            throw new VectorDimensionMismatchException(DocumentChunkConfiguration.DefaultEmbeddingDimension,
                embedding.Length);
        }

        if (limit <= 0)
        {
            return [];
        }

        var targetVector = new Vector(embedding);

        var query = dbContext.DocumentChunks
            .AsNoTracking()
            .Where(c => c.Embedding != null);

        if (documentId.HasValue)
        {
            query = query.Where(c => c.DocumentId == documentId.Value);
        }

        var results = await query
            .Select(c => new
            {
                Chunk = c,
                Distance = (float)c.Embedding!.CosineDistance(targetVector)
            })
            .Where(r => 1.0f - r.Distance >= minSimilarity)
            .OrderBy(r => r.Distance)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return results
            .Select((r, index) => new ScoredChunk(
                Chunk: r.Chunk,
                Score: 1.0f - r.Distance,
                Rank: index + 1,
                SearchMode: SearchMode.DenseVector
            ))
            .ToList();
    }

    /// <summary>
    /// Performs full-text lexical search using PostgreSQL text search vectors and ts_rank ranking.
    /// </summary>
    public async Task<IReadOnlyList<ScoredChunk>> LexicalSearchAsync(
        string queryText,
        int limit = 5,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);

        if (limit <= 0)
        {
            return [];
        }
        
        var query = dbContext.DocumentChunks
            .AsNoTracking();

        if (documentId.HasValue)
        {
            query = query.Where(c => c.DocumentId == documentId.Value);
        }

        var results = await query
            .Where(c => EF.Functions.ToTsVector("english", c.Content)
                .Matches(EF.Functions.PlainToTsQuery("english", queryText)))
            .Select(c => new
            {
                Chunk = c,
                Rank = EF.Functions.ToTsVector("english", c.Content)
                    .Rank(EF.Functions.PlainToTsQuery("english", queryText))
            })
            .OrderByDescending(r => r.Rank)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return results
            .Select((r, index) => new ScoredChunk(
                Chunk: r.Chunk,
                Score: r.Rank,
                Rank: index + 1,
                SearchMode: SearchMode.Lexical
            ))
            .ToList();
    }

    /// <summary>
    /// Performs hybrid search combining dense vector similarity and lexical search using Reciprocal Rank Fusion (RRF).
    /// </summary>
    public async Task<IReadOnlyList<ScoredChunk>> HybridSearchAsync(
        string queryText,
        ReadOnlyMemory<float> embedding,
        int limit = 5,
        float minSimilarity = 0.0f,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryText);

        if (embedding.Length != DocumentChunkConfiguration.DefaultEmbeddingDimension)
        {
            throw new VectorDimensionMismatchException(DocumentChunkConfiguration.DefaultEmbeddingDimension,
                embedding.Length);
        }

        if (limit <= 0)
        {
            return [];
        }

        var candidateLimit = Math.Max(limit * 2, 20);

        var vectorResults = await VectorSearchAsync(
            embedding,
            limit: candidateLimit,
            minSimilarity: minSimilarity,
            documentId: documentId,
            cancellationToken: cancellationToken);

        var lexicalResults = await LexicalSearchAsync(
            queryText,
            limit: candidateLimit,
            documentId: documentId,
            cancellationToken: cancellationToken);

        var scoreMap = new Dictionary<long, (DocumentChunk Chunk, float RrfScore)>();

        foreach (var item in vectorResults)
        {
            var score = 1.0f / (RrfK + item.Rank);
            scoreMap[item.Chunk.Id] = (item.Chunk, score);
        }

        foreach (var item in lexicalResults)
        {
            var score = 1.0f / (RrfK + item.Rank);
            if (scoreMap.TryGetValue(item.Chunk.Id, out var existing))
            {
                scoreMap[item.Chunk.Id] = (existing.Chunk, existing.RrfScore + score);
            }
            else
            {
                scoreMap[item.Chunk.Id] = (item.Chunk, score);
            }
        }

        return scoreMap.Values
            .OrderByDescending(x => x.RrfScore)
            .Take(limit)
            .Select((x, index) => new ScoredChunk(
                Chunk: x.Chunk,
                Score: x.RrfScore,
                Rank: index + 1,
                SearchMode: SearchMode.Hybrid
            ))
            .ToList();
    }
}