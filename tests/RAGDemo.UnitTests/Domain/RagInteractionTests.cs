using RAGDemo.Domain.Entities;
using RAGDemo.Domain.Enums;
using RAGDemo.Domain.Models;

namespace RAGDemo.UnitTests.Domain;

public class RagInteractionTests
{
    [Fact]
    public void NewRagInteraction_ShouldHaveDefaultValues()
    {
        // Act
        var interaction = new RagInteraction();

        // Assert
        Assert.NotEqual(Guid.Empty, interaction.Id);
        Assert.Equal(string.Empty, interaction.SessionId);
        Assert.Equal(string.Empty, interaction.UserPrompt);
        Assert.Equal(string.Empty, interaction.GeneratedResponse);
        Assert.Empty(interaction.RetrievedChunkIds);
        Assert.Equal(TimeSpan.Zero, interaction.Latency);
        Assert.Equal(0, interaction.PromptTokens);
        Assert.Equal(0, interaction.CompletionTokens);
        Assert.Empty(interaction.EvaluationMetrics);
        Assert.True(interaction.CreatedAtUtc <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CustomInitialization_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var customId = Guid.NewGuid();
        var retrievedIds = new List<long> { 101, 102, 103 }.AsReadOnly();
        var latency = TimeSpan.FromMilliseconds(450);

        // Act
        var interaction = new RagInteraction
        {
            Id = customId,
            SessionId = "session-123",
            UserPrompt = "Explain RAG architecture",
            GeneratedResponse = "RAG combines retrieval with generative LLMs.",
            RetrievedChunkIds = retrievedIds,
            Latency = latency,
            PromptTokens = 50,
            CompletionTokens = 120
        };

        // Assert
        Assert.Equal(customId, interaction.Id);
        Assert.Equal("session-123", interaction.SessionId);
        Assert.Equal("Explain RAG architecture", interaction.UserPrompt);
        Assert.Equal("RAG combines retrieval with generative LLMs.", interaction.GeneratedResponse);
        Assert.Equal(retrievedIds, interaction.RetrievedChunkIds);
        Assert.Equal(latency, interaction.Latency);
        Assert.Equal(50, interaction.PromptTokens);
        Assert.Equal(120, interaction.CompletionTokens);
    }

    [Fact]
    public void AddEvaluationMetric_ShouldAppendMetricToEvaluationMetrics()
    {
        // Arrange
        var interaction = new RagInteraction();
        var metric = new EvaluationMetric(
            EvaluationMetricType.Faithfulness,
            0.95,
            "Response is fully supported by retrieved context.",
            isPassing: true
        );

        // Act
        interaction.AddEvaluationMetric(metric);

        // Assert
        Assert.Single(interaction.EvaluationMetrics);
        var added = interaction.EvaluationMetrics[0];
        Assert.Equal(EvaluationMetricType.Faithfulness, added.MetricType);
        Assert.Equal(0.95, added.Score);
        Assert.Equal("Response is fully supported by retrieved context.", added.Reason);
        Assert.True(added.IsPassing);
    }

    [Fact]
    public void AddEvaluationMetric_WhenNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var interaction = new RagInteraction();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => interaction.AddEvaluationMetric(null!));
    }

    [Fact]
    public void AddEvaluationMetrics_ShouldAppendMultipleMetrics()
    {
        // Arrange
        var interaction = new RagInteraction();
        var metrics = new List<EvaluationMetric>
        {
            new(EvaluationMetricType.Faithfulness, 0.90, "High faithfulness"),
            new(EvaluationMetricType.AnswerRelevance, 0.85, "Relevant answer"),
            new(EvaluationMetricType.ContextPrecision, 0.80, "Precise context")
        };

        // Act
        interaction.AddEvaluationMetrics(metrics);

        // Assert
        Assert.Equal(3, interaction.EvaluationMetrics.Count);
        Assert.Equal(EvaluationMetricType.Faithfulness, interaction.EvaluationMetrics[0].MetricType);
        Assert.Equal(EvaluationMetricType.AnswerRelevance, interaction.EvaluationMetrics[1].MetricType);
        Assert.Equal(EvaluationMetricType.ContextPrecision, interaction.EvaluationMetrics[2].MetricType);
    }

    [Fact]
    public void AddEvaluationMetrics_WhenNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var interaction = new RagInteraction();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => interaction.AddEvaluationMetrics(null!));
    }
}