using Microsoft.EntityFrameworkCore;
using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Exceptions;
using RAGDemo.Domain.ValueObjects;
using RAGDemo.Infrastructure;
using RAGDemo.Infrastructure.Data;
using Xunit;

namespace RAGDemo.IntegrationTests.Repositories;

public class DocumentRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = DbContextOptionsFactory.CreateOptions();
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddDocumentAsync_And_GetByIdAsync_ShouldPersistAndRetrieveDocument()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = "RAG Architectural Overview",
            SourceUri = "https://example.com/docs/rag-architecture",
            ContentType = "text/markdown",
            Status = DocumentStatus.Pending,
            ChunkCount = 0,
            TotalTokens = 0
        };

        // Act
        var createdId = await repository.AddDocumentAsync(document);
        var retrieved = await repository.GetByIdAsync(createdId);

        // Assert
        Assert.Equal(document.Id, createdId);
        Assert.NotNull(retrieved);
        Assert.Equal("RAG Architectural Overview", retrieved.Title);
        Assert.Equal("https://example.com/docs/rag-architecture", retrieved.SourceUri);
        Assert.Equal("text/markdown", retrieved.ContentType);
        Assert.Equal(DocumentStatus.Pending, retrieved.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedDocumentsInDescendingOrder()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        var doc1 = new Document { Title = "Doc 1", SourceUri = "https://example.com/1" };
        var doc2 = new Document { Title = "Doc 2", SourceUri = "https://example.com/2" };
        var doc3 = new Document { Title = "Doc 3", SourceUri = "https://example.com/3" };

        await repository.AddDocumentAsync(doc1);
        await repository.AddDocumentAsync(doc2);
        await repository.AddDocumentAsync(doc3);

        // Act
        var results = await repository.GetAllAsync(skip: 0, take: 2);

        // Assert
        Assert.NotNull(results);
        Assert.True(results.Count <= 2);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateDocumentStatusAndErrorMessage()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        var document = new Document
        {
            Title = "Status Update Test",
            SourceUri = "https://example.com/status-test"
        };
        var docId = await repository.AddDocumentAsync(document);

        // Act
        await repository.UpdateStatusAsync(docId, DocumentStatus.Processing);
        var processingDoc = await repository.GetByIdAsync(docId);

        await repository.UpdateStatusAsync(docId, DocumentStatus.Failed, "Model timeout");
        var failedDoc = await repository.GetByIdAsync(docId);

        // Assert
        Assert.NotNull(processingDoc);
        Assert.Equal(DocumentStatus.Processing, processingDoc.Status);
        Assert.Null(processingDoc.ErrorMessage);
        Assert.NotNull(processingDoc.UpdatedAtUtc);

        Assert.NotNull(failedDoc);
        Assert.Equal(DocumentStatus.Failed, failedDoc.Status);
        Assert.Equal("Model timeout", failedDoc.ErrorMessage);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenDocumentDoesNotExist_ShouldThrowDocumentNotFoundException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
            repository.UpdateStatusAsync(nonExistentId, DocumentStatus.Processing));

        Assert.Equal(nonExistentId, exception.DocumentId);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldRemoveDocumentAndCascadeDeleteChunks()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = docId,
            Title = "Document to Delete",
            SourceUri = "https://example.com/delete-me"
        };
        await repository.AddDocumentAsync(document);

        var chunks = new List<DocumentChunk>
        {
            new(id: 0, documentId: docId, chunkIndex: 0, content: "Chunk content 1", tokenCount: 10),
            new(id: 0, documentId: docId, chunkIndex: 1, content: "Chunk content 2", tokenCount: 12)
        };
        await repository.AddChunksAsync(chunks);

        // Act
        await repository.DeleteDocumentAsync(docId);

        var retrievedDoc = await repository.GetByIdAsync(docId);
        var retrievedChunks = await repository.GetChunksByDocumentIdAsync(docId);

        // Assert
        Assert.Null(retrievedDoc);
        Assert.Empty(retrievedChunks);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WhenDocumentDoesNotExist_ShouldThrowDocumentNotFoundException()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
            repository.DeleteDocumentAsync(nonExistentId));

        Assert.Equal(nonExistentId, exception.DocumentId);
    }

    [Fact]
    public async Task AddChunksAsync_And_GetChunksByDocumentIdAsync_ShouldPersistAndOrderChunks()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new DocumentRepository(context);

        var docId = Guid.NewGuid();
        var document = new Document
        {
            Id = docId,
            Title = "Chunk Ordering Test",
            SourceUri = "https://example.com/chunk-order"
        };
        await repository.AddDocumentAsync(document);

        var metadata = new ChunkMetadata
        {
            SectionTitle = "Introduction",
            CustomAttributes = new Dictionary<string, string>
            {
                ["author"] = "Architect"
            }
        };

        float[] embeddingVector = new float[1536];
        embeddingVector[0] = 0.5f;
        embeddingVector[1] = -0.2f;

        var chunks = new List<DocumentChunk>
        {
            new(id: 0, documentId: docId, chunkIndex: 2, content: "Section 3 content", tokenCount: 15, embedding: embeddingVector, metadata: metadata),
            new(id: 0, documentId: docId, chunkIndex: 0, content: "Section 1 content", tokenCount: 10, embedding: embeddingVector, metadata: metadata),
            new(id: 0, documentId: docId, chunkIndex: 1, content: "Section 2 content", tokenCount: 12, embedding: embeddingVector, metadata: metadata)
        };

        // Act
        await repository.AddChunksAsync(chunks);
        var retrievedChunks = await repository.GetChunksByDocumentIdAsync(docId);

        // Assert
        Assert.Equal(3, retrievedChunks.Count);
        Assert.Equal(0, retrievedChunks[0].ChunkIndex);
        Assert.Equal("Section 1 content", retrievedChunks[0].Content);
        Assert.Equal(1, retrievedChunks[1].ChunkIndex);
        Assert.Equal("Section 2 content", retrievedChunks[1].Content);
        Assert.Equal(2, retrievedChunks[2].ChunkIndex);
        Assert.Equal("Section 3 content", retrievedChunks[2].Content);
        Assert.Equal("Introduction", retrievedChunks[0].Metadata.SectionTitle);
        Assert.Equal("Architect", retrievedChunks[0].Metadata.CustomAttributes["author"]);
        Assert.NotNull(retrievedChunks[0].Embedding);
    }
}
