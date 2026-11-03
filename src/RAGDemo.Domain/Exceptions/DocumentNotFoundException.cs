namespace RAGDemo.Domain.Exceptions;

public sealed class DocumentNotFoundException(Guid documentId)
    : DomainException($"Document with ID '{documentId}' was not found.")
{
    public Guid DocumentId { get; } = documentId;
}
