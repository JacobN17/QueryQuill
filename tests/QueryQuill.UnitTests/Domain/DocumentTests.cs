using QueryQuill.Domain.Entities;
using QueryQuill.Domain.Enums;
using Xunit;

namespace QueryQuill.UnitTests.Domain;

public class DocumentTests
{
    [Fact]
    public void NewDocument_ShouldHavePendingStatus_AndDefaultValues()
    {
        var doc = new Document
        {
            Title = "Architecture Overview",
            SourceUri = "https://docs.queryquill.io/arch.md"
        };

        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal(DocumentStatus.Pending, doc.Status);
        Assert.Equal("Architecture Overview", doc.Title);
        Assert.Equal("https://docs.queryquill.io/arch.md", doc.SourceUri);
        Assert.Equal("text/plain", doc.ContentType);
        Assert.Equal(0, doc.ChunkCount);
        Assert.Equal(0, doc.TotalTokens);
        Assert.Null(doc.ErrorMessage);
    }

    [Fact]
    public void MarkAsProcessing_ShouldUpdateStatus()
    {
        var doc = new Document { Title = "Test Doc" };
        doc.MarkAsProcessing();

        Assert.Equal(DocumentStatus.Processing, doc.Status);
        Assert.NotNull(doc.UpdatedAtUtc);
        Assert.Null(doc.ErrorMessage);
    }

    [Fact]
    public void MarkAsIndexed_ShouldUpdateStatusAndCounts()
    {
        var doc = new Document { Title = "Test Doc" };
        doc.MarkAsIndexed(chunkCount: 12, totalTokens: 4500);

        Assert.Equal(DocumentStatus.Indexed, doc.Status);
        Assert.Equal(12, doc.ChunkCount);
        Assert.Equal(4500, doc.TotalTokens);
        Assert.NotNull(doc.UpdatedAtUtc);
        Assert.Null(doc.ErrorMessage);
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusAndErrorMessage()
    {
        var doc = new Document { Title = "Test Doc" };
        doc.MarkAsFailed("Failed to generate embeddings.");

        Assert.Equal(DocumentStatus.Failed, doc.Status);
        Assert.Equal("Failed to generate embeddings.", doc.ErrorMessage);
        Assert.NotNull(doc.UpdatedAtUtc);
    }
}
