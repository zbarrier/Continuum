namespace Orleans.Configuration;

/// <summary>
///     Validates <see cref="KurrentDBStreamCacheBufferPoolOptions" />.
/// </summary>
public class KurrentDBStreamCacheBufferPoolOptionsValidator : IConfigurationValidator
{
    private readonly KurrentDBStreamCacheBufferPoolOptions _options;
    private readonly string _name;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBStreamCacheBufferPoolOptionsValidator" />.
    /// </summary>
    public KurrentDBStreamCacheBufferPoolOptionsValidator(KurrentDBStreamCacheBufferPoolOptions options, string name)
    {
        _options = options;
        _name = name;
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (_options.TrimWindow < TimeSpan.Zero && _options.TrimWindow != Timeout.InfiniteTimeSpan)
        {
            throw Invalid($"{nameof(KurrentDBStreamCacheBufferPoolOptions.TrimWindow)} must not be negative, but was {_options.TrimWindow}.");
        }
        if (_options.ExtraBlocks < 0)
        {
            throw Invalid($"{nameof(KurrentDBStreamCacheBufferPoolOptions.ExtraBlocks)} must not be negative, but was {_options.ExtraBlocks}.");
        }
        if (_options.Headroom < 0)
        {
            throw Invalid($"{nameof(KurrentDBStreamCacheBufferPoolOptions.Headroom)} must not be negative, but was {_options.Headroom}.");
        }
        if (_options.ConsecutiveWindows < 1)
        {
            throw Invalid($"{nameof(KurrentDBStreamCacheBufferPoolOptions.ConsecutiveWindows)} must be at least 1, but was {_options.ConsecutiveWindows}.");
        }
    }

    private OrleansConfigurationException Invalid(string reason) =>
        new($"{nameof(KurrentDBStreamCacheBufferPoolOptions)} on stream provider {_name} is invalid. {reason}");
}
