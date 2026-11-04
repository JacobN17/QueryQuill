using Microsoft.EntityFrameworkCore;
using QueryQuill.Domain.Entities;

namespace QueryQuill.Infrastructure.Data;

// Defines the contract for the application database context, exposing entity sets and persistence operations.
public interface IApplicationDbContext
{
    // Gets the database set for managing Document entities.
    DbSet<Document> Documents { get; }

    // Gets the database set for managing DocumentChunk entities with vector embeddings.
    DbSet<DocumentChunk> DocumentChunks { get; }

    // Gets the database set for managing RagInteraction telemetry and evaluation records.
    DbSet<RagInteraction> RagInteractions { get; }

    // Asynchronously saves all changes made in this context to the underlying database.
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
