using System.Collections.Immutable;
using System.Globalization;
using System.Net;

namespace Continuum.CSharpFunctionalExtensions.Tests.ErrorTests;

public class ErrorImmutabilityTests
{
    [Fact]
    public void ValidationError_FromList_SnapshotsEntries()
    {
        var entries = new List<ValidationErrorEntry> { new(ValidationSeverity.Error, "A", "code", "a", null) };
        var error = new ValidationError(entries);

        entries.Add(new ValidationErrorEntry(ValidationSeverity.Error, "B", "code", "b", null));

        Assert.Single(error.Entries);
    }

    [Fact]
    public void ValidationError_NullEntry_Throws() =>
        Assert.Throws<ArgumentException>(() => new ValidationError([null!]));

    [Fact]
    public void ValidationError_DefaultImmutableArray_Throws() =>
        Assert.Throws<ArgumentException>(() => new ValidationError(default(ImmutableArray<ValidationErrorEntry>)));

    [Fact]
    public void ValidationErrorEntry_NullArguments_IsEmpty()
    {
        var entry = new ValidationErrorEntry(ValidationSeverity.Error, "A", "code", "a", null);

        Assert.False(entry.Arguments.IsDefault);
        Assert.Empty(entry.Arguments);
    }

    [Fact]
    public void RequestError_ExposesArguments()
    {
        var error = new RequestError(HttpStatusCode.BadRequest, Grpc.Core.StatusCode.InvalidArgument, "code", "{0} {1}", "x", 1);

        Assert.Equal<ErrorArgument>(["x", 1], error.Arguments);
    }

    [Fact]
    public void RequestError_NullArguments_IsEmpty()
    {
        var error = new RequestError(HttpStatusCode.BadRequest, Grpc.Core.StatusCode.InvalidArgument, "code", null, null, null, null);

        Assert.False(error.Arguments.IsDefault);
        Assert.Empty(error.Arguments);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Error error = new RequestError(HttpStatusCode.BadRequest, Grpc.Core.StatusCode.InvalidArgument, "code");

        Assert.True(error.CompareTo(null) > 0);
    }

    [Fact]
    public void ValidationError_FormattedMessage_JoinsWithLineFeedWithoutTrailingNewline()
    {
        var error = new ValidationError(
        [
            new ValidationErrorEntry(ValidationSeverity.Error, "A", "code", "first", null),
            new ValidationErrorEntry(ValidationSeverity.Error, "B", "code", "second", null),
        ]);

        Assert.Equal("first\nsecond", error.GetFormattedMessage(CultureInfo.InvariantCulture));
        Assert.Equal("only", new ValidationError("A", "code", "only").GetFormattedMessage(CultureInfo.InvariantCulture));
    }
}
