using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Exceptions;
using RAGDemo.Domain.ValueObjects;
using RAGDemo.Infrastructure;
using RAGDemo.Infrastructure.Data;
using RAGDemo.Infrastructure.Data.Configurations;

namespace RAGDemo.IntegrationTests.Repositories;

public class VectorSearchRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = DbContextOptionsFactory.CreateOptions();
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task VectorSearchAsync_WhenDimensionMismatch_ShouldThrowVectorDimensionMismatchException()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);
        var invalidEmbedding = new float[128]; // Expected 1536

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VectorDimensionMismatchException>(() =>
            repository.VectorSearchAsync(invalidEmbedding));

        Assert.Equal(DocumentChunkConfiguration.DefaultEmbeddingDimension, exception.ExpectedDimension);
        Assert.Equal(128, exception.ActualDimension);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task VectorSearchAsync_WhenLimitZeroOrNegative_ShouldReturnEmptyList(int invalidLimit)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);
        var validEmbedding = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];

        // Act
        var results = await repository.VectorSearchAsync(validEmbedding, limit: invalidLimit);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LexicalSearchAsync_WhenQueryInvalid_ShouldThrowArgumentException(string? invalidQuery)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);

        // Act & Assert
        if (invalidQuery is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.LexicalSearchAsync(invalidQuery!));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.LexicalSearchAsync(invalidQuery));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task LexicalSearchAsync_WhenLimitZeroOrNegative_ShouldReturnEmptyList(int invalidLimit)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);

        // Act
        var results = await repository.LexicalSearchAsync("sample query", limit: invalidLimit);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HybridSearchAsync_WhenQueryInvalid_ShouldThrowArgumentException(string? invalidQuery)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);
        var validEmbedding = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];

        // Act & Assert
        if (invalidQuery is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.HybridSearchAsync(invalidQuery!, validEmbedding));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.HybridSearchAsync(invalidQuery, validEmbedding));
        }
    }

    [Fact]
    public async Task HybridSearchAsync_WhenDimensionMismatch_ShouldThrowVectorDimensionMismatchException()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);
        var invalidEmbedding = new float[512];

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VectorDimensionMismatchException>(() =>
            repository.HybridSearchAsync("sample query", invalidEmbedding));

        Assert.Equal(DocumentChunkConfiguration.DefaultEmbeddingDimension, exception.ExpectedDimension);
        Assert.Equal(512, exception.ActualDimension);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task HybridSearchAsync_WhenLimitZeroOrNegative_ShouldReturnEmptyList(int invalidLimit)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new VectorSearchRepository(context);
        var validEmbedding = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];

        // Act
        var results = await repository.HybridSearchAsync("sample query", validEmbedding, limit: invalidLimit);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task VectorSearchAsync_ShouldPersistChunksAndRetrieveTopSimilar()
    {
        // Arrange
        await using var context = CreateContext();
        var docRepo = new DocumentRepository(context);
        var searchRepo = new VectorSearchRepository(context);

        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = docId,
            Title = "Vector Search Test Document",
            SourceUri = "https://example.com/vector-search"
        };
        await docRepo.AddDocumentAsync(document);

        var embeddingA = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];
        embeddingA[0] = 1.0f; // Vector [1, 0, 0, ...]

        var embeddingB = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];
        embeddingB[0] = 0.8f;
        embeddingB[1] = 0.6f;

        var chunks = new List<DocumentChunk>
        {
            new(0, docId, 0, "Chunk A exact match", 10, embeddingA, ChunkMetadata.Empty),
            new(0, docId, 1, "Chunk B partial match", 10, embeddingB, ChunkMetadata.Empty)
        };
        await docRepo.AddChunksAsync(chunks);

        // Act
        var results = await searchRepo.VectorSearchAsync(embeddingA, limit: 5, documentId: docId);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Equal(SearchMode.DenseVector, results[0].SearchMode);
        Assert.Equal(1, results[0].Rank);
        Assert.True(results[0].Score >= 0.99f); // Exact or near-exact match
    }

    [Fact]
    public async Task LexicalSearchAsync_ShouldRetrieveMatchingChunks()
    {
        // Arrange
        await using var context = CreateContext();
        var docRepo = new DocumentRepository(context);
        var searchRepo = new VectorSearchRepository(context);

        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = docId,
            Title = "Lexical Search Test Document",
            SourceUri = "https://example.com/lexical-search"
        };
        await docRepo.AddDocumentAsync(document);

        var chunks = new List<DocumentChunk>
        {
            new(0, docId, 0, "PostgreSQL supports powerful full-text search and inverted indexes.", 12),
            new(0, docId, 1, "Cooking recipes for delicious sourdough bread and pastries.", 10)
        };
        await docRepo.AddChunksAsync(chunks);

        // Act
        var results = await searchRepo.LexicalSearchAsync("PostgreSQL search indexes", limit: 5, documentId: docId);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("PostgreSQL supports powerful full-text search and inverted indexes.", results[0].Chunk.Content);
        Assert.Equal(SearchMode.Lexical, results[0].SearchMode);
        Assert.Equal(1, results[0].Rank);
    }

    [Fact]
    public async Task HybridSearchAsync_ShouldFuseVectorAndLexicalResults()
    {
        // Arrange
        await using var context = CreateContext();
        var docRepo = new DocumentRepository(context);
        var searchRepo = new VectorSearchRepository(context);

        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = docId,
            Title = "Hybrid Search Test Document",
            SourceUri = "https://example.com/hybrid-search"
        };
        await docRepo.AddDocumentAsync(document);

        var queryEmbedding = new float[DocumentChunkConfiguration.DefaultEmbeddingDimension];
        queryEmbedding[0] = 1.0f;

        var chunks = new List<DocumentChunk>
        {
            new(0, docId, 0, "Quantum algorithms and quantum annealing principles.", 10, queryEmbedding),
            new(0, docId, 1, "Classic annealing optimization algorithms and heuristics.", 10, queryEmbedding)
        };
        await docRepo.AddChunksAsync(chunks);

        // Act
        var results = await searchRepo.HybridSearchAsync(
            "quantum annealing",
            queryEmbedding,
            limit: 2,
            documentId: docId);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(SearchMode.Hybrid, r.SearchMode));
        Assert.Equal(1, results[0].Rank);
        Assert.True(results[0].Score > 0);
    }
}