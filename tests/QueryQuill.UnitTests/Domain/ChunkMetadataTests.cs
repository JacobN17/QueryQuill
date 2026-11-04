using QueryQuill.Domain.ValueObjects;

namespace QueryQuill.UnitTests.Domain;

public class ChunkMetadataTests
{
    [Fact]
    public void FromJson_WithValidJson_ShouldDeserializeProperly()
    {
        var json = """
        {
            "DocumentTitle": "Guide",
            "SourceUri": "https://example.com/guide.pdf",
            "SectionTitle": "Introduction",
            "PageNumber": 3,
            "CustomAttributes": {
                "category": "ai",
                "author": "dev"
            }
        }
        """;

        var metadata = ChunkMetadata.FromJson(json);

        Assert.Equal("Guide", metadata.DocumentTitle);
        Assert.Equal("https://example.com/guide.pdf", metadata.SourceUri);
        Assert.Equal("Introduction", metadata.SectionTitle);
        Assert.Equal(3, metadata.PageNumber);
        Assert.Equal("ai", metadata.CustomAttributes["category"]);
        Assert.Equal("dev", metadata.CustomAttributes["author"]);
    }

    [Fact]
    public void FromJson_WithInvalidOrNullJson_ShouldReturnEmpty()
    {
        var emptyFromNull = ChunkMetadata.FromJson(null);
        var emptyFromWhitespace = ChunkMetadata.FromJson("   ");
        var emptyFromMalformed = ChunkMetadata.FromJson("not-valid-json");

        Assert.Equal(ChunkMetadata.Empty, emptyFromNull);
        Assert.Equal(ChunkMetadata.Empty, emptyFromWhitespace);
        Assert.Equal(ChunkMetadata.Empty, emptyFromMalformed);
    }

    [Fact]
    public void ToJson_And_FromJson_ShouldRoundtrip()
    {
        var original = new ChunkMetadata
        {
            DocumentTitle = "PostgreSQL pgvector Guide",
            SourceUri = "https://pgvector.dev",
            SectionTitle = "HNSW Indexing",
            PageNumber = 1,
            CustomAttributes = new Dictionary<string, string>
            {
                ["index"] = "hnsw",
                ["dim"] = "1536"
            }
        };

        var json = original.ToJson();
        var roundtripped = ChunkMetadata.FromJson(json);

        Assert.Equal(original.DocumentTitle, roundtripped.DocumentTitle);
        Assert.Equal(original.SourceUri, roundtripped.SourceUri);
        Assert.Equal(original.SectionTitle, roundtripped.SectionTitle);
        Assert.Equal(original.PageNumber, roundtripped.PageNumber);
        Assert.Equal("hnsw", roundtripped.CustomAttributes["index"]);
    }
}
