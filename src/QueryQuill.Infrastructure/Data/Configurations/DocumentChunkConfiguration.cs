using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using QueryQuill.Domain.Entities;
using QueryQuill.Domain.ValueObjects;

namespace QueryQuill.Infrastructure.Data.Configurations;

// Configures the database schema, mapping, vector indexing, and constraints for the DocumentChunk entity.
public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    // The default vector dimension for embedding models (e.g. text-embedding-3-small or nomic-embed-text: 1536).
    public const int DefaultEmbeddingDimension = 1536;

    // Configures the entity properties, keys, pgvector indexing, and JSON mappings.
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        // Maps the entity to the "document_chunks" table in the database.
        builder.ToTable("document_chunks");

        // Sets the primary key as an auto-incrementing identity column.
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        // Configures the foreign key reference to the parent Document.
        builder.Property(c => c.DocumentId)
            .IsRequired();

        // Configures the zero-based sequence index of the chunk within the parent document.
        builder.Property(c => c.ChunkIndex)
            .IsRequired();

        // Configures the text content of the chunk: stored as postgres text column.
        builder.Property(c => c.Content)
            .HasColumnType("text")
            .IsRequired();

        // Configures the token count of the chunk.
        builder.Property(c => c.TokenCount)
            .IsRequired();

        // Configures the vector embedding property using Pgvector's Vector type mapping.
        // Provides a bidirectional conversion between ReadOnlyMemory<float>? and Pgvector.Vector?.
        var vectorConverter = new ValueConverter<ReadOnlyMemory<float>?, Vector?>(
            v => v.HasValue ? new Vector(v.Value) : null,
            v => v == null ? null : v.Memory
        );

        builder.Property(c => c.Embedding)
            .HasConversion(vectorConverter)
            .HasColumnType($"vector({DefaultEmbeddingDimension})")
            .IsRequired(false);

        // Configures the ChunkMetadata value object to be serialized/deserialized as a PostgreSQL jsonb column.
        var metadataConverter = new ValueConverter<ChunkMetadata, string>(
            m => m.ToJson(),
            json => ChunkMetadata.FromJson(json)
        );

        builder.Property(c => c.Metadata)
            .HasConversion(metadataConverter)
            .HasColumnType("jsonb")
            .IsRequired();

        // Configures the creation timestamp in UTC.
        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        // Creates a composite index on DocumentId and ChunkIndex for fast document reconstruction.
        builder.HasIndex(c => new { c.DocumentId, c.ChunkIndex });

        // Creates a high-performance HNSW index on the vector embedding column for fast approximate nearest neighbor (ANN) search.
        builder.HasIndex(c => c.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
