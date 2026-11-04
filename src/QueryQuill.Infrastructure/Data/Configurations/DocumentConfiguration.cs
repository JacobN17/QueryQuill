using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QueryQuill.Domain.Entities;

namespace QueryQuill.Infrastructure.Data.Configurations;

// Configures the database schema, mapping, and constraints for the Document entity.
public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    // Configures the entity properties, keys, relationships, and table mappings.
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        // Maps the entity to the "documents" table in the database.
        builder.ToTable("documents");

        // Sets the primary key for the table.
        builder.HasKey(d => d.Id);

        // Configures the Title property: required with maximum length of 500 characters.
        builder.Property(d => d.Title)
            .HasMaxLength(500)
            .IsRequired();

        // Configures the SourceUri property: required with maximum length of 2048 characters.
        builder.Property(d => d.SourceUri)
            .HasMaxLength(2048)
            .IsRequired();

        // Configures the ContentType property: required with maximum length of 100 characters.
        builder.Property(d => d.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        // Configures the Status enum property: converted to integer column for efficient storage and query indexing.
        builder.Property(d => d.Status)
            .HasConversion<int>()
            .IsRequired();

        // Configures the ChunkCount property representing the total number of chunks.
        builder.Property(d => d.ChunkCount)
            .IsRequired();

        // Configures the TotalTokens property representing total token usage across all chunks.
        builder.Property(d => d.TotalTokens)
            .IsRequired();

        // Configures the ErrorMessage property: optional text column for processing errors.
        builder.Property(d => d.ErrorMessage)
            .HasColumnType("text")
            .IsRequired(false);

        // Configures the CreatedAtUtc timestamp: required.
        builder.Property(d => d.CreatedAtUtc)
            .IsRequired();

        // Configures the UpdatedAtUtc timestamp: optional.
        builder.Property(d => d.UpdatedAtUtc)
            .IsRequired(false);

        // Configures one-to-many relationship with DocumentChunk entities.
        builder.HasMany(d => d.Chunks)
            .WithOne()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Creates an index on Status for fast filtering of pending or processing documents.
        builder.HasIndex(d => d.Status);

        // Creates an index on CreatedAtUtc for chronological sorting and query optimization.
        builder.HasIndex(d => d.CreatedAtUtc);
    }
}
