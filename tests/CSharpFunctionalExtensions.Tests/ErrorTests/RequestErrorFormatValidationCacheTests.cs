using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions.Tests.ErrorTests;

public class RequestErrorFormatValidationCacheTests
{
    [Fact]
    public void Defaults_CacheEnabledWithDefaultSize()
    {
        Assert.True(RequestErrorFormatValidationCache.IsEnabled);
        Assert.Equal(RequestErrorFormatValidationCache.DefaultCacheSize, RequestErrorFormatValidationCache.MaxSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(16)]
    [InlineData(1024)]
    public void Validate_InvalidFormat_ThrowsEveryTime(int maxSize)
    {
        var format = "Invalid {1} " + Guid.NewGuid();
        ErrorArgument[] arguments = ["a"];

        Assert.Throws<FormatException>(() => RequestErrorFormatValidationCache.Validate(format, arguments, maxSize));
        Assert.Throws<FormatException>(() => RequestErrorFormatValidationCache.Validate(format, arguments, maxSize));
    }

    [Fact]
    public void Validate_CachedFormat_StillChecksArgumentKinds()
    {
        var format = "Value {0:X} " + Guid.NewGuid();

        RequestErrorFormatValidationCache.Validate(format, [ErrorArgument.From(255L)], 1024);

        Assert.Throws<FormatException>(() =>
            RequestErrorFormatValidationCache.Validate(format, [ErrorArgument.From(2.5d)], 1024));
    }

    [Fact]
    public void Validate_CachedFormat_StillChecksArgumentCount()
    {
        var format = "Order {0} " + Guid.NewGuid();

        RequestErrorFormatValidationCache.Validate(format, ["a"], 1024);

        Assert.Throws<FormatException>(() => RequestErrorFormatValidationCache.Validate(format, [], 1024));
    }

    [Fact]
    public void Validate_ManyArguments_BypassesCache()
    {
        var arguments = Enumerable.Range(0, 20).Select(i => ErrorArgument.From((long)i)).ToArray();
        var format = string.Concat(Enumerable.Range(0, 20).Select(i => $"{{{i}}}"));

        RequestErrorFormatValidationCache.Validate(format, arguments, 1024);

        Assert.Throws<FormatException>(() => RequestErrorFormatValidationCache.Validate(format + "{20}", arguments, 1024));
    }

    [Fact]
    public void Constructor_InvalidFormat_Throws()
    {
        var format = "Order {1} " + Guid.NewGuid();

        Assert.Throws<FormatException>(() => RequestErrors.NewNotFound(format, "a"));
    }

    [Theory]
    [InlineData(null, RequestErrorFormatValidationCache.DefaultCacheSize)]
    [InlineData("0", 0)]
    [InlineData("4096", 4096)]
    [InlineData(2048, 2048)]
    public void ReadSize_ValidValues(object? data, int expected)
    {
        Assert.Equal(expected, RequestErrorFormatValidationCache.ReadSize(data));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("large")]
    [InlineData(-5)]
    [InlineData(true)]
    public void ReadSize_InvalidValues_Throw(object data)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => RequestErrorFormatValidationCache.ReadSize(data));

        Assert.Contains(RequestErrorFormatValidationCache.CacheSizeDataName, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadSize_UsesInvariantDigitsOnly()
    {
        Assert.Throws<InvalidOperationException>(() => RequestErrorFormatValidationCache.ReadSize(1.5.ToString(CultureInfo.InvariantCulture)));
    }
}
