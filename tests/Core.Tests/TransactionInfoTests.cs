using Continuum.Events;

namespace Continuum.Tests;

public sealed class TransactionInfoTests
{
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(5, 3, 2)]
    [InlineData(5, 5, 0)]
    public void Consistent_counts_are_valid(int size, int partitionSize, int index)
    {
        var transaction = new TransactionInfo(Guid.NewGuid(), size, partitionSize, index);

        Assert.True(transaction.IsValid);
        transaction.EnsureValid();
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(2, 3, 0)]
    [InlineData(3, 2, 2)]
    [InlineData(3, 2, -1)]
    public void Inconsistent_counts_are_invalid(int size, int partitionSize, int index)
    {
        var transaction = new TransactionInfo(Guid.NewGuid(), size, partitionSize, index);

        Assert.False(transaction.IsValid);
        var ex = Assert.Throws<ArgumentException>(() => transaction.EnsureValid("transaction"));
        Assert.Equal("transaction", ex.ParamName);
    }
}
