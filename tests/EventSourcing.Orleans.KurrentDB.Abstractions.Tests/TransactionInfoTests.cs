namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions.Tests;

public class TransactionInfoTests
{
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(5, 5, 4)]
    [InlineData(5, 2, 1)]
    public void Valid_Shapes_Are_Accepted(int size, int partitionSize, int partitionIndex)
    {
        var transaction = new TransactionInfo(NewId.Next(), size, partitionSize, partitionIndex);

        Assert.True(transaction.IsValid);
        transaction.EnsureValid();
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 2, 0)]
    [InlineData(2, 2, -1)]
    [InlineData(2, 2, 2)]
    public void Invalid_Shapes_Are_Rejected(int size, int partitionSize, int partitionIndex)
    {
        var transaction = new TransactionInfo(NewId.Next(), size, partitionSize, partitionIndex);

        Assert.False(transaction.IsValid);
        var exception = Assert.Throws<ArgumentException>(() => transaction.EnsureValid("value"));
        Assert.Equal("value", exception.ParamName);
    }
}
