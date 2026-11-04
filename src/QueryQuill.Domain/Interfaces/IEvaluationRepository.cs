using QueryQuill.Domain.Entities;

namespace QueryQuill.Domain.Interfaces;

public interface IEvaluationRepository
{
    Task SaveInteractionAsync(RagInteraction interaction, CancellationToken cancellationToken = default);
    Task<RagInteraction?> GetInteractionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RagInteraction>> GetInteractionsBySessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RagInteraction>> GetRecentInteractionsAsync(int count = 50, CancellationToken cancellationToken = default);
}
