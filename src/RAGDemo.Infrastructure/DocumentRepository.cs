using Microsoft.EntityFrameworkCore;
using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Exceptions;
using RAGDemo.Domain.Interfaces;
using RAGDemo.Infrastructure.Data;

namespace RAGDemo.Infrastructure;

// Provides Entity Framework Core implementation of IDocumentRepository for managing Document and DocumentChunk persistence.
public sealed class DocumentRepository(ApplicationDbContext dbContext) : IDocumentRepository
{
    // Retrieves a document by its unique identifier, including its associated chunks.
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Documents
            .Include(d => d.Chunks)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    // Retrieves a paginated list of documents sorted in descending chronological order.
    public async Task<IReadOnlyList<Document>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        return await dbContext.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    // Persists a new document entity to the database and returns its unique identifier.
    public async Task<Guid> AddDocumentAsync(Document document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        await dbContext.Documents.AddAsync(document, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return document.Id;
    }

    // Updates the processing status, error message, and timestamp of a document by its ID.
    public async Task UpdateStatusAsync(Guid id, DocumentStatus status, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(id);
        }

        document.Status = status;
        document.ErrorMessage = errorMessage;
        document.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Removes a document and cascades deletion to all associated document chunks.
    public async Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(id);
        }
        dbContext.Documents.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Persists a batch of document chunk entities containing text content and vector embeddings.
    public async Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        await dbContext.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Retrieves all chunks associated with a specific document, ordered by their sequence index.
    public async Task<IReadOnlyList<DocumentChunk>> GetChunksByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.DocumentChunks
            .AsNoTracking()
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);
    }
}
