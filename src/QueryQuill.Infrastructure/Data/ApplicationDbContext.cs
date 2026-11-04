using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using QueryQuill.Domain.Entities;

namespace QueryQuill.Infrastructure.Data;

// Represents the primary Entity Framework Core database context for the RAG platform with PostgreSQL and pgvector support.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    // Gets or sets the database set for managing Document entities.
    public DbSet<Document> Documents => Set<Document>();

    // Gets or sets the database set for managing DocumentChunk entities containing vector embeddings.
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    // Gets or sets the database set for managing RagInteraction telemetry and evaluation records.
    public DbSet<RagInteraction> RagInteractions => Set<RagInteraction>();

    // Initializes a new instance of ApplicationDbContext with configured DbContextOptions.

    // Configures the database model, registers pgvector PostgreSQL extensions, and applies entity type configurations.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registers the PostgreSQL vector extension (CREATE EXTENSION IF NOT EXISTS vector).
        modelBuilder.HasPostgresExtension("vector");

        // Applies all entity type configurations (IEntityTypeConfiguration<T>) from the infrastructure assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
