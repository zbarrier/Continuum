namespace Orleans.Configuration;

/// <summary>
///     Options for trimming the pool of 2 MB blocks shared by every queue cache of a KurrentDB stream provider on a silo.
/// </summary>
/// <remarks>
///     The pool grows to whatever a catch-up needs. Once demand has stayed lower for <see cref="ConsecutiveWindows" />
///     windows in a row, free blocks above the target are released. The target is the larger of the active queue cache
///     count plus <see cref="ExtraBlocks" />, and the window's peak in-use count plus <see cref="Headroom" />. Blocks that
///     hold cached events are never released. Released blocks are reclaimed by the next gen 2 garbage collection.
/// </remarks>
public sealed class KurrentDBStreamCacheBufferPoolOptions
{
    /// <summary>The default <see cref="TrimWindow" />.</summary>
    public static readonly TimeSpan DefaultTrimWindow = TimeSpan.FromMinutes(20);

    /// <summary>The default <see cref="ExtraBlocks" />.</summary>
    public const int DefaultExtraBlocks = 2;

    /// <summary>The default <see cref="Headroom" />.</summary>
    public const int DefaultHeadroom = 2;

    /// <summary>The default <see cref="ConsecutiveWindows" />.</summary>
    public const int DefaultConsecutiveWindows = 2;

    /// <summary>
    ///     The length of the window over which peak block usage is measured. <see cref="TimeSpan.Zero" /> or
    ///     <see cref="Timeout.InfiniteTimeSpan" /> turns trimming off.
    /// </summary>
    public TimeSpan TrimWindow { get; set; } = DefaultTrimWindow;

    /// <summary>
    ///     The number of blocks kept on top of one per active queue cache.
    /// </summary>
    public int ExtraBlocks { get; set; } = DefaultExtraBlocks;

    /// <summary>
    ///     The number of blocks kept on top of the window's peak usage.
    /// </summary>
    public int Headroom { get; set; } = DefaultHeadroom;

    /// <summary>
    ///     The number of windows in a row that must need fewer blocks than are allocated before the pool is trimmed.
    /// </summary>
    public int ConsecutiveWindows { get; set; } = DefaultConsecutiveWindows;

    internal bool TrimmingEnabled => TrimWindow > TimeSpan.Zero && TrimWindow != Timeout.InfiniteTimeSpan;
}
