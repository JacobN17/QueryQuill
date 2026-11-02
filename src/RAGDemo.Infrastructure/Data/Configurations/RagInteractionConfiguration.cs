using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Models;

namespace RAGDemo.Infrastructure.Data.Configurations;

// Configures the database schema, mapping, JSON conversions, and constraints for the RagInteraction entity.
public sealed class RagInteractionConfiguration : IEntityTypeConfiguration<RagInteraction>
{
    // Configures the entity properties, keys, JSON column conversions, and indexing.
    public void Configure(EntityTypeBuilder<RagInteraction> builder)
    {
        // Maps the entity to the "rag_interactions" table in the database.
        builder.ToTable("rag_interactions");

        // Sets the primary key for the table.
        builder.HasKey(r => r.Id);

        // Configures the SessionId property: max 100 characters.
        builder.Property(r => r.SessionId)
            .HasMaxLength(100)
            .IsRequired();

        // Configures the UserPrompt property: stored as postgres text column.
        builder.Property(r => r.UserPrompt)
            .HasColumnType("text")
            .IsRequired();

        // Configures the GeneratedResponse property: stored as postgres text column.
        builder.Property(r => r.GeneratedResponse)
            .HasColumnType("text")
            .IsRequired();

        // Configures the RetrievedChunkIds collection: stored as bigint array in PostgreSQL.
        var chunkIdsConverter = new ValueConverter<IReadOnlyList<long>, long[]>(
            ids => ids.ToArray(),
            arr => arr.ToList()
        );

        var chunkIdsComparer = new ValueComparer<IReadOnlyList<long>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );

        builder.Property(r => r.RetrievedChunkIds)
            .HasConversion(chunkIdsConverter, chunkIdsComparer)
            .HasColumnType("bigint[]")
            .IsRequired();

        // Configures the Latency property: stored as interval/ticks.
        builder.Property(r => r.Latency)
            .IsRequired();

        // Configures PromptTokens and CompletionTokens for token economy and cost tracking.
        builder.Property(r => r.PromptTokens)
            .IsRequired();

        builder.Property(r => r.CompletionTokens)
            .IsRequired();

        // Configures the EvaluationMetrics list: serialized to PostgreSQL jsonb column.
        var metricsConverter = new ValueConverter<IReadOnlyList<EvaluationMetric>, string>(
            metrics => JsonSerializer.Serialize(metrics, JsonSerializerOptions.Default),
            json => string.IsNullOrWhiteSpace(json)
                ? new List<EvaluationMetric>()
                : JsonSerializer.Deserialize<List<EvaluationMetric>>(json, JsonSerializerOptions.Default) ??
                  new List<EvaluationMetric>()
        );

        var metricsComparer = new ValueComparer<IReadOnlyList<EvaluationMetric>>(
            (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );

        builder.Property(r => r.EvaluationMetrics)
            .HasField("_evaluationMetrics")
            .HasConversion(metricsConverter, metricsComparer)
            .HasColumnType("jsonb")
            .IsRequired();

        // Configures the CreatedAtUtc timestamp.
        builder.Property(r => r.CreatedAtUtc)
            .IsRequired();

        // Creates an index on SessionId for fast querying of session histories.
        builder.HasIndex(r => r.SessionId);

        // Creates an index on CreatedAtUtc for telemetry filtering, time-series analysis, and analytics.
        builder.HasIndex(r => r.CreatedAtUtc);
    }
}