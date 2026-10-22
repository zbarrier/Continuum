namespace Orleans.Configuration;

/// <summary>
/// </summary>
public class KurrentDBStreamCheckpointerOptions : KurrentDBOperationOptions
{
    /// <summary>
    ///     Interval to write checkpoints.  Prevents spamming storage.
    /// </summary>
    public TimeSpan PersistInterval { get; set; } = TimeSpan.FromSeconds(30);
}
