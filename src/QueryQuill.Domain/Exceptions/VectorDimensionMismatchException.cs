namespace QueryQuill.Domain.Exceptions;

public sealed class VectorDimensionMismatchException(int expectedDimension, int actualDimension)
    : DomainException($"Vector dimension mismatch: expected {expectedDimension}, but got {actualDimension}.")
{
    public int ExpectedDimension { get; } = expectedDimension;
    public int ActualDimension { get; } = actualDimension;
}
