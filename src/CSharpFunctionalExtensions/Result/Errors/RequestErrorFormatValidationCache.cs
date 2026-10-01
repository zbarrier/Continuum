#nullable enable

using System.Collections.Concurrent;
using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Caches successful <see cref="RequestError"/> format validations so each distinct format is validated once.
/// </summary>
/// <remarks>
///     <para>
///         Configured once at startup through <see cref="AppContext"/>. The cache is enabled by default with a maximum of
///         <see cref="DefaultCacheSize"/> (1024) entries. For example, to override the default of 1024 with 4096 entries, or to
///         disable the cache, add the following to the application project file:
///     </para>
///     <code>
///     &lt;ItemGroup&gt;
///       &lt;RuntimeHostConfigurationOption Include="Continuum.CSharpFunctionalExtensions.RequestError.UseFormatValidationCache" Value="false" Trim="true" /&gt;
///       &lt;RuntimeHostConfigurationOption Include="Continuum.CSharpFunctionalExtensions.RequestError.FormatValidationCacheSize" Value="4096" /&gt;
///     &lt;/ItemGroup&gt;
///     </code>
///     <para>
///         or with <c>AppContext.SetSwitch</c>/<c>AppContext.SetData</c> before the first <see cref="RequestError"/> is created.
///     </para>
/// </remarks>
internal static class RequestErrorFormatValidationCache
{
    internal const string UseCacheSwitchName = "Continuum.CSharpFunctionalExtensions.RequestError.UseFormatValidationCache";
    internal const string CacheSizeDataName = "Continuum.CSharpFunctionalExtensions.RequestError.FormatValidationCacheSize";
    internal const int DefaultCacheSize = 1024;

    private const int MaxCachedArgumentCount = 16;

    private static readonly ConcurrentDictionary<(string Format, int Count, ulong Kinds), bool> ValidatedFormats = new();
    private static int validatedFormatCount;

    /// <summary>Gets whether the cache is enabled. Defaults to true.</summary>
#if NET9_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.FeatureSwitchDefinition(UseCacheSwitchName)]
#endif
    internal static bool IsEnabled { get; } = !AppContext.TryGetSwitch(UseCacheSwitchName, out var enabled) || enabled;

    /// <summary>Gets the maximum number of cached entries. Defaults to <see cref="DefaultCacheSize"/>; 0 disables caching.</summary>
    internal static int MaxSize { get; } = ReadSize(AppContext.GetData(CacheSizeDataName));

    internal static int Count => Volatile.Read(ref validatedFormatCount);

    /// <summary>Validates <paramref name="format"/> against <paramref name="arguments"/>, throwing <see cref="FormatException"/> if invalid.</summary>
    internal static void Validate(string format, ErrorArgument[] arguments) =>
        Validate(format, arguments, IsEnabled ? MaxSize : 0);

    internal static void Validate(string format, ErrorArgument[] arguments, int maxSize)
    {
        if (format.Length == 0)
        {
            return;
        }

        // Validity depends on the format and on each argument's kind (for example "{0:X}" is invalid for a double),
        // so the cache key includes both. Only successful validations are cached.
        if (maxSize <= 0 || arguments.Length > MaxCachedArgumentCount)
        {
            FormatOrThrow(format, arguments);
            return;
        }

        ulong kinds = 0;
        for (var i = 0; i < arguments.Length; i++)
        {
            kinds |= (ulong)arguments[i].Kind << (i * 4);
        }

        var key = (format, arguments.Length, kinds);
        if (ValidatedFormats.ContainsKey(key))
        {
            return;
        }

        FormatOrThrow(format, arguments);

        if (Volatile.Read(ref validatedFormatCount) < maxSize && ValidatedFormats.TryAdd(key, true))
        {
            Interlocked.Increment(ref validatedFormatCount);
        }
    }

    internal static void Clear()
    {
        ValidatedFormats.Clear();
        Volatile.Write(ref validatedFormatCount, 0);
    }

    internal static int ReadSize(object? data) => data switch
    {
        null => DefaultCacheSize,
        int size when size >= 0 => size,
        string text when int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var size) => size,
        _ => throw new InvalidOperationException(
            $"AppContext data '{CacheSizeDataName}' must be a non-negative integer, but was '{data}'."),
    };

    // Validate, this will throw FormatException if invalid.
    private static void FormatOrThrow(string format, ErrorArgument[] arguments) =>
        _ = string.Format(CultureInfo.InvariantCulture, format, ErrorArgument.ToObjects(arguments));
}
