using QueryQuill.Domain.Entities;
using QueryQuill.Domain.Enums;

namespace QueryQuill.Domain.Models;

public sealed record ScoredChunk(
    DocumentChunk Chunk,
    float Score,
    int Rank = 1,
    SearchMode SearchMode = SearchMode.DenseVector
);
