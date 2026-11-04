using QueryQuill.Domain.Entities;
using QueryQuill.Domain.Enums;

namespace QueryQuill.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetAllAsync(int skip = 0, int take = 50, CancellationToken cancellationToken = default);
    Task<Guid> AddDocumentAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, DocumentStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentChunk>> GetChunksByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
