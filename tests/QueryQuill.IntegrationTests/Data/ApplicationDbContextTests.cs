using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using QueryQuill.Domain.Entities;
using QueryQuill.Infrastructure.Data;
using Xunit;

namespace QueryQuill.IntegrationTests.Data;

public class ApplicationDbContextTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = DbContextOptionsFactory.CreateOptions();
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void ModelBuilder_ShouldRegisterAllEntitySetsAndConfigurations()
    {
        using var context = CreateContext();
        var model = context.Model;

        // Verify entities are registered
        var docEntity = model.FindEntityType(typeof(Document));
        var chunkEntity = model.FindEntityType(typeof(DocumentChunk));
        var interactionEntity = model.FindEntityType(typeof(RagInteraction));

        Assert.NotNull(docEntity);
        Assert.NotNull(chunkEntity);
        Assert.NotNull(interactionEntity);

        // Verify table names
        Assert.Equal("documents", docEntity.GetTableName());
        Assert.Equal("document_chunks", chunkEntity.GetTableName());
        Assert.Equal("rag_interactions", interactionEntity.GetTableName());
    }

    [Fact]
    public void DocumentChunk_ShouldHaveVectorEmbeddingAndHnswIndex()
    {
        using var context = CreateContext();
        var chunkEntity = context.Model.FindEntityType(typeof(DocumentChunk));

        Assert.NotNull(chunkEntity);

        var embeddingProperty = chunkEntity.FindProperty(nameof(DocumentChunk.Embedding));
        Assert.NotNull(embeddingProperty);
        Assert.Equal("vector(1536)", embeddingProperty.GetColumnType());

        // Verify HNSW index
        var hnswIndex = chunkEntity.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(DocumentChunk.Embedding)));

        Assert.NotNull(hnswIndex);
        Assert.Contains(hnswIndex.Properties, p => p.Name == nameof(DocumentChunk.Embedding));
    }

    [Fact]
    public void Document_ShouldHaveCascadeDeleteRelationshipWithChunks()
    {
        using var context = CreateContext();
        var docEntity = context.Model.FindEntityType(typeof(Document));

        Assert.NotNull(docEntity);

        var foreignKey = docEntity.GetNavigations()
            .FirstOrDefault(n => n.Name == nameof(Document.Chunks))?
            .ForeignKey;

        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }
}