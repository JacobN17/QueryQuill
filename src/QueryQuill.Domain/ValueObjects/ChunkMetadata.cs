using System.Text.Json;

namespace QueryQuill.Domain.ValueObjects;

public sealed record ChunkMetadata
{
    public static ChunkMetadata Empty { get; } = new();

    public string? DocumentTitle { get; init; }
    public string? SourceUri { get; init; }
    public string? SectionTitle { get; init; }
    public int? PageNumber { get; init; }
    public IReadOnlyDictionary<string, string> CustomAttributes { get; init; } = new Dictionary<string, string>();

    public static ChunkMetadata FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        try
        {
            return JsonSerializer.Deserialize<ChunkMetadata>(json) ?? Empty;
        }
        catch
        {
            return Empty;
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this);

    public bool Equals(ChunkMetadata? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DocumentTitle == other.DocumentTitle &&
               SourceUri == other.SourceUri &&
               SectionTitle == other.SectionTitle &&
               PageNumber == other.PageNumber &&
               CustomAttributesEquals(CustomAttributes, other.CustomAttributes);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DocumentTitle);
        hash.Add(SourceUri);
        hash.Add(SectionTitle);
        hash.Add(PageNumber);
        foreach (var kvp in CustomAttributes.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    private static bool CustomAttributesEquals(IReadOnlyDictionary<string, string>? a, IReadOnlyDictionary<string, string>? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        if (a.Count != b.Count) return false;

        foreach (var (key, value) in a)
        {
            if (!b.TryGetValue(key, out var otherVal) || otherVal != value)
            {
                return false;
            }
        }

        return true;
    }
}
