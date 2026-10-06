namespace Continuum.Streaming;

/// <summary>
/// The parts a stream name decomposes into.
/// </summary>
/// <remarks>
/// <para>
///     A stream name is a single string that encodes the category the stream belongs to and the key of the
///     individual stream within it.
/// </para>
/// <para>
///     This is returned as one value rather than exposing a member per part, because the parts are recovered by a
///     single pass over the name. Asking for the key and the topic separately would parse the same string twice.
/// </para>
/// </remarks>
/// <param name="Topic">
///     The category the stream belongs to. For <c>snack-123</c> this is <c>snack</c>. This is the grouping a
///     subscription filters on, and it is the outer scope a projection watermark is keyed by.
/// </param>
/// <param name="StreamKey">
///     The key identifying the individual stream within its <see cref="Topic"/>. For <c>snack-123</c> this is
///     <c>123</c>.
/// </param>
public readonly record struct StreamedName(string Topic, string StreamKey);
