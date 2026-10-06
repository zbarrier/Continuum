namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

/// <summary>
///     Verifies how a KurrentDB stream name decomposes into its category and key.
/// </summary>
/// <remarks>
///     The category separator is fixed by the store rather than chosen here: KurrentDB builds its <c>$ce-</c>
///     category projections from the first <c>-</c>, so these tests pin the parser to the same reading the store
///     uses. Getting this wrong is not a formatting problem, it silently regroups streams.
/// </remarks>
public class DefaultKurrentDBStreamNameParserTests
{
    private readonly DefaultKurrentDBStreamNameParser _parser = new();

    [Fact]
    public void Splits_A_Name_Into_Its_Category_And_Key()
    {
        var parsed = _parser.Parse("snack-123");

        Assert.Equal("snack", parsed.Topic);
        Assert.Equal("123", parsed.StreamKey);
    }

    [Fact]
    public void Keeps_Dashes_That_Belong_To_The_Key()
    {
        // Only the first dash separates the category, so a GUID key survives intact. Splitting on every dash
        // would truncate the key and collapse distinct streams onto one identity.
        var parsed = _parser.Parse("snack-a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        Assert.Equal("snack", parsed.Topic);
        Assert.Equal("a1b2c3d4-e5f6-7890-abcd-ef1234567890", parsed.StreamKey);
    }

    [Fact]
    public void Rejects_A_Name_With_No_Category_Separator()
    {
        // A name with no separator has no category, and guessing one would put events in a bucket that does not
        // correspond to anything the store groups.
        Assert.Throws<FormatException>(() => _parser.Parse("snack"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_A_Missing_Name(string? streamName)
    {
        Assert.ThrowsAny<ArgumentException>(() => _parser.Parse(streamName!));
    }
}
