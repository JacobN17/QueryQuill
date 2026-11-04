using Microsoft.EntityFrameworkCore;
using QueryQuill.Domain.Entities;
using QueryQuill.Domain.Interfaces;
using QueryQuill.Infrastructure.Data;

namespace QueryQuill.Infrastructure;

public sealed class EvaluationRepository(ApplicationDbContext dbContext) : IEvaluationRepository
{
    public async Task SaveInteractionAsync(RagInteraction interaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(interaction);

        await dbContext.RagInteractions.AddAsync(interaction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RagInteraction?> GetInteractionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.RagInteractions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<RagInteraction>> GetInteractionsBySessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        return await dbContext.RagInteractions
            .AsNoTracking()
            .Where(i => i.SessionId == sessionId)
            .OrderBy(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RagInteraction>> GetRecentInteractionsAsync(
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return [];
        }

        return await dbContext.RagInteractions
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}