using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Models;
using RAGDemo.Infrastructure;
using RAGDemo.Infrastructure.Data;

namespace RAGDemo.IntegrationTests.Repositories;

public class EvaluationRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = DbContextOptionsFactory.CreateOptions();
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SaveInteractionAsync_And_GetInteractionByIdAsync_ShouldPersistAndRetrieveInteraction()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        var interaction = new RagInteraction
        {
            Id = Guid.NewGuid(),
            SessionId = $"session-{Guid.NewGuid()}",
            UserPrompt = "What is the capital of France?",
            GeneratedResponse = "The capital of France is Paris.",
            RetrievedChunkIds = new List<long> { 101, 102, 103 }.AsReadOnly(),
            Latency = TimeSpan.FromMilliseconds(320),
            PromptTokens = 15,
            CompletionTokens = 8
        };

        interaction.AddEvaluationMetric(new EvaluationMetric(
            EvaluationMetricType.Faithfulness,
            0.98,
            "Direct statement backed by retrieved knowledge.",
            isPassing: true
        ));

        // Act
        await repository.SaveInteractionAsync(interaction);
        var retrieved = await repository.GetInteractionByIdAsync(interaction.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(interaction.Id, retrieved.Id);
        Assert.Equal(interaction.SessionId, retrieved.SessionId);
        Assert.Equal(interaction.UserPrompt, retrieved.UserPrompt);
        Assert.Equal(interaction.GeneratedResponse, retrieved.GeneratedResponse);
        Assert.Equal(interaction.RetrievedChunkIds, retrieved.RetrievedChunkIds);
        Assert.Equal(interaction.Latency, retrieved.Latency);
        Assert.Equal(15, retrieved.PromptTokens);
        Assert.Equal(8, retrieved.CompletionTokens);
        Assert.Single(retrieved.EvaluationMetrics);
        Assert.Equal(EvaluationMetricType.Faithfulness, retrieved.EvaluationMetrics[0].MetricType);
        Assert.Equal(0.98, retrieved.EvaluationMetrics[0].Score);
    }

    [Fact]
    public async Task GetInteractionByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        // Act
        var result = await repository.GetInteractionByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SaveInteractionAsync_WhenNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveInteractionAsync(null!));
    }

    [Fact]
    public async Task GetInteractionsBySessionAsync_ShouldReturnInteractionsChronologically()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);
        var targetSessionId = $"session-chronological-{Guid.NewGuid()}";
        var otherSessionId = $"session-other-{Guid.NewGuid()}";

        var interaction1 = new RagInteraction
        {
            SessionId = targetSessionId,
            UserPrompt = "Prompt 1",
            GeneratedResponse = "Response 1"
        };

        var interaction2 = new RagInteraction
        {
            SessionId = targetSessionId,
            UserPrompt = "Prompt 2",
            GeneratedResponse = "Response 2"
        };

        var otherInteraction = new RagInteraction
        {
            SessionId = otherSessionId,
            UserPrompt = "Other Prompt",
            GeneratedResponse = "Other Response"
        };

        await repository.SaveInteractionAsync(interaction1);
        await repository.SaveInteractionAsync(interaction2);
        await repository.SaveInteractionAsync(otherInteraction);

        // Act
        var sessionInteractions = await repository.GetInteractionsBySessionAsync(targetSessionId);

        // Assert
        Assert.NotNull(sessionInteractions);
        Assert.Equal(2, sessionInteractions.Count);
        Assert.All(sessionInteractions, i => Assert.Equal(targetSessionId, i.SessionId));
        Assert.True(sessionInteractions[0].CreatedAtUtc <= sessionInteractions[1].CreatedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetInteractionsBySessionAsync_WhenSessionIdInvalid_ShouldThrowArgumentException(
        string? invalidSessionId)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        // Act & Assert
        if (invalidSessionId is null)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repository.GetInteractionsBySessionAsync(invalidSessionId!));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.GetInteractionsBySessionAsync(invalidSessionId));
        }
    }

    [Fact]
    public async Task GetRecentInteractionsAsync_ShouldReturnRecentInteractionsInDescendingOrder()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        var interaction1 = new RagInteraction
        {
            SessionId = $"recent-{Guid.NewGuid()}",
            UserPrompt = "Recent Prompt 1",
            GeneratedResponse = "Recent Response 1"
        };
        var interaction2 = new RagInteraction
        {
            SessionId = $"recent-{Guid.NewGuid()}",
            UserPrompt = "Recent Prompt 2",
            GeneratedResponse = "Recent Response 2"
        };

        await repository.SaveInteractionAsync(interaction1);
        await repository.SaveInteractionAsync(interaction2);

        // Act
        var recentInteractions = await repository.GetRecentInteractionsAsync(count: 2);

        // Assert
        Assert.NotNull(recentInteractions);
        Assert.NotEmpty(recentInteractions);
        Assert.True(recentInteractions.Count <= 2);

        if (recentInteractions.Count >= 2)
        {
            Assert.True(recentInteractions[0].CreatedAtUtc >= recentInteractions[1].CreatedAtUtc);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetRecentInteractionsAsync_WhenCountZeroOrNegative_ShouldReturnEmptyList(int invalidCount)
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        // Act
        var results = await repository.GetRecentInteractionsAsync(invalidCount);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task ProgressiveEnrichment_ShouldAllowAddingMetricsToExistingInteraction()
    {
        // Arrange
        await using var context = CreateContext();
        var repository = new EvaluationRepository(context);

        var interaction = new RagInteraction
        {
            SessionId = $"async-eval-{Guid.NewGuid()}",
            UserPrompt = "Summarize quantum computing.",
            GeneratedResponse = "Quantum computing uses qubits."
        };

        await repository.SaveInteractionAsync(interaction);

        // Act: simulate out-of-band evaluation worker enriching the interaction record
        await using var workerContext = CreateContext();
        var trackedInteraction = await workerContext.RagInteractions.FindAsync(interaction.Id);
        Assert.NotNull(trackedInteraction);

        trackedInteraction.AddEvaluationMetric(new EvaluationMetric(
            EvaluationMetricType.AnswerRelevance,
            0.94,
            "Succinct and highly relevant definition."
        ));
        await workerContext.SaveChangesAsync();

        // Assert: re-fetch via repository and verify enriched metrics
        var updatedInteraction = await repository.GetInteractionByIdAsync(interaction.Id);
        Assert.NotNull(updatedInteraction);
        Assert.Single(updatedInteraction.EvaluationMetrics);
        Assert.Equal(EvaluationMetricType.AnswerRelevance, updatedInteraction.EvaluationMetrics[0].MetricType);
        Assert.Equal(0.94, updatedInteraction.EvaluationMetrics[0].Score);
    }
}